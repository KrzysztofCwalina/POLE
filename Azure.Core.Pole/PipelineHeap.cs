// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;

namespace Azure.Core.Pole
{
    public class PipelineHeap : PoleHeap
    {
        PipeWriter _writer;
        int _written;
        Memory<byte> _buffer;

        public PipelineHeap(PipeWriter writer)
        {
            _writer = writer;
            _buffer = _writer.GetMemory();
            _written = RootOffset;
        }

        public override Reference AllocateObject(int size, ulong typeId)
        {
            var address = _written;
            _written += size + sizeof(ulong);
            var slice = _buffer.Span.Slice(address, size + sizeof(ulong));
            slice.Fill(0);
            BinaryPrimitives.WriteUInt64LittleEndian(slice, typeId);
            return new Reference(this, address);
        }

        public override Sequence<byte> AllocateBuffer(int length)
        {
            var address = _written;
            var totalLength = length + sizeof(int);
            _written += totalLength;
            var slice = _buffer.Slice(address, length + sizeof(int));
            BinaryPrimitives.WriteInt32LittleEndian(slice.Span, length);
            return new Sequence<byte>(slice, address);
        }

        public override Span<byte> GetBytes(int address, int length = -1)
            => _buffer.Span.Slice(address, length == -1 ? _buffer.Length - address : length);

        public int TotalWritten => _written;

        public override byte this[int address]
        {
            get => _buffer.Span[address];
            set => _buffer.Span[address] = value;
        }

        public void Complete()
        {
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Span, _written);
            _writer.Advance(_written);
            _writer.Complete();
        }

        public override bool TryComputeLength(out long length)
        {
            length = _written;
            return true;
        }

        public override ReadOnlySequence<byte> GetByteSequence(int address, int length)
        {
            return new ReadOnlySequence<byte>(_buffer.Slice(address, length));
        }
    }
}
