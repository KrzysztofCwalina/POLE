﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Core.Pole
{
    public class ArrayPoolHeap : PoleHeap, IDisposable
    {
        Memory<byte> _buffer;
        int _written = 0;
        int _segmentSize;

        public ArrayPoolHeap(int segmentSize = 512)
        {
            _segmentSize = segmentSize;
            var bytes = ArrayPool<byte>.Shared.Rent(_segmentSize);
            Array.Clear(bytes, 0, bytes.Length);
            _buffer = bytes;
            _written = RootAddress;
        }

        public override Reference AllocateObject(int size, ulong typeId)
        {
            var address = _written;
            _written += size + sizeof(ulong);
            var slice = _buffer.Slice(address, size + sizeof(ulong));
            BinaryPrimitives.WriteUInt64LittleEndian(slice.Span, typeId);
            return new Reference(this, slice, address);
        }

        public override Sequence<byte> AllocateBuffer(int length)
        {
            var address = _written;
            var totalLength = length + sizeof(int);
            _written += totalLength;
            var slice = _buffer.Slice(address, totalLength);
            BinaryPrimitives.WriteInt32LittleEndian(slice.Span, length);
            return new Sequence<byte>(slice, address);
        }

        public override Span<byte> GetBytes(int address, int length = -1)
            => _buffer.Span.Slice(address, length == -1 ? _buffer.Length - address : length);

        public override byte this[int address]
        {
            get => _buffer.Span[address];
            set => _buffer.Span[address] = value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                ReadOnlyMemory<byte> memory = _buffer;
                _buffer = null;
                if (MemoryMarshal.TryGetArray(memory, out var segment))
                {
                    ArrayPool<byte>.Shared.Return(segment.Array);
                }
            }
        }
 
        public static ArrayPoolHeap ReadFrom(Stream stream, int segmentSize = 512)
        {
            // read header
            var lenMemory = ArrayPool<byte>.Shared.Rent(4);
            int lenRead = stream.Read(lenMemory, 0, 4);
            if (lenRead != 4) throw new NotImplementedException();
            int len = lenMemory[3] << 24 | lenMemory[2] << 16 | lenMemory[1] << 8 | lenMemory[0];
            ArrayPool<byte>.Shared.Return(lenMemory);

            var memory = ArrayPool<byte>.Shared.Rent(segmentSize);
            BinaryPrimitives.WriteInt32LittleEndian(memory, len);
            int read = stream.Read(memory, RootAddress, memory.Length - RootAddress);
            var heap = new ArrayPoolHeap()
            {
                _buffer = memory,
                _written = len,
                _segmentSize = segmentSize
            };
            return heap;
        }
        public static ArrayPoolHeap ReadFrom(Memory<byte> data)
        {
            if (data.Length < 4) throw new InvalidOperationException("bytes don't contain POLE data");
            var heap = new ArrayPoolHeap()
            {
                _buffer = data,
                _written = data.Length,
                _segmentSize = data.Length
            };
            return heap;
        }
        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            // TODO: this needs to be async
            stream.WriteByte((byte)_written);
            stream.WriteByte((byte)(_written >> 8));
            stream.WriteByte((byte)(_written >> 16));
            stream.WriteByte((byte)(_written >> 24));
            stream.Flush();
            await stream.WriteAsync(_buffer.Slice(0, _written), cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        public void WriteTo(Stream stream, CancellationToken cancellationToken = default)
        {
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Span, _written);
            stream.Write(_buffer.Slice(0, _written), cancellationToken);
            stream.Flush();
        }

        public override bool TryComputeLength(out long length)
        {
            length = _written;
            return true;
        }

        public override ReadOnlySequence<byte> GetByteSequence(int address, int length)
            => new ReadOnlySequence<byte>(_buffer.Slice(address, length));

        protected override Memory<byte> GetMemoryCore(int address)
            => _buffer.Slice(address);
    }
}
