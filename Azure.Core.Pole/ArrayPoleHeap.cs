using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Reflection;
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
            _written = RootOffset;
        }
        
        public override PoleReference Allocate(int size)
        {
            var address = _written;
            _written += size;
            return new PoleReference(this, address);
        }

        public override Span<byte> GetBytes(int address, int length = -1)
            => _buffer.Span.Slice(address, length == -1 ? _buffer.Length - address : length);

        //public override ReadOnlySpan<byte> ReadBytes(int address, int length = -1)
        //    => GetBytes(address, length);

        public override byte this[int address]
        {
            get => _buffer.Span[address];
            set => _buffer.Span[address] = value;
        }
        
        public void Dispose()
        {
            ReadOnlyMemory<byte> memory = _buffer;
            _buffer = null;
            if (MemoryMarshal.TryGetArray(memory, out var segment)){
                ArrayPool<byte>.Shared.Return(segment.Array);
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
            int read = stream.Read(memory, RootOffset, memory.Length - RootOffset);
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
        //public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
        //{
        //    // TODO: this needs to be async
        //    stream.WriteByte((byte)_written);
        //    stream.WriteByte((byte)(_written >> 8));
        //    stream.WriteByte((byte)(_written >> 16));
        //    stream.WriteByte((byte)(_written >> 24));
        //    stream.Flush(); 
        //    await stream.WriteAsync(_buffer.Slice(0, _written), cancellationToken);
        //    await stream.FlushAsync(cancellationToken);
        //}

        //public static async Task<ArrayPoolHeap> ReadFromAsync(Stream stream, int segmentSize = 512, CancellationToken cancellationToken = default)
        //{
        //    var memory = ArrayPool<byte>.Shared.Rent(segmentSize);
        //    int read = await stream.ReadAsync(memory, 0, memory.Length);
        //    var len = BinaryPrimitives.ReadInt32LittleEndian(memory);
        //    var heap = new ArrayPoolHeap()
        //    {
        //        _buffer = memory,
        //        _written = len,
        //        _segmentSize = segmentSize
        //    };
        //    return heap;
        //}
        public void WriteTo(Stream stream, CancellationToken cancellationToken = default)
        {
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Span, _written);
            stream.Write(_buffer.Slice(0, _written), cancellationToken);
            stream.Flush();
        }
    }
}
