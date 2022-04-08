using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Core.Pole
{
    public class PoleHeap : IDisposable
    {
        Memory<byte> _memory;
        int _free = 0;
        int _segmentSize;

        public PoleHeap(int segmentSize = 512)
        {
            _segmentSize = segmentSize;
            _memory = ArrayPool<byte>.Shared.Rent(segmentSize);
        }

        public static async Task<PoleHeap> CreateAsync(Stream stream, int segmentSize = 512, CancellationToken cancellationToken = default)
        {
            var memory = ArrayPool<byte>.Shared.Rent(segmentSize);
            int read = await stream.ReadAsync(memory, 0, memory.Length);
            var len = BinaryPrimitives.ReadInt32LittleEndian(memory);
            var heap = new PoleHeap()
            {
                _memory = memory,
                _free = len,
                _segmentSize = segmentSize
            };
            return heap;
        }
        public static PoleHeap ReadFrom(Stream stream, int segmentSize = 512)
        {
            var lenMemory = ArrayPool<byte>.Shared.Rent(4);
            int lenRead = stream.Read(lenMemory, 0, 4);
            if (lenRead != 4) throw new NotImplementedException();
            int len = lenMemory[3] << 24 | lenMemory[2] << 16 | lenMemory[1] << 8 | lenMemory[0];
            ArrayPool<byte>.Shared.Return(lenMemory);

            var memory = ArrayPool<byte>.Shared.Rent(segmentSize);
            int read = stream.Read(memory, 0, memory.Length);
            var heap = new PoleHeap()
            {
                _memory = memory,
                _free = len,
                _segmentSize = segmentSize
            };
            return heap;
        }
        public static PoleHeap ReadFrom(Memory<byte> data)
        {
            if (data.Length < 4) throw new InvalidOperationException("bytes don't contain POLE data");
            var length = BinaryPrimitives.ReadInt32LittleEndian(data.Span);

            var heap = new PoleHeap()
            {
                _memory = data.Slice(4),
                _free = length,
                _segmentSize = data.Length
            };
            return heap;
        }
        public IList<T> AllocateArray<T>(int size) => PoleArray<T>.Allocate(this, size);

        public PoleReference Allocate(int size)
        {
            var address = _free;
            _free += size;
            return new PoleReference(this, address);
        }
        public PoleReference GetAt(int offset) => new PoleReference(this, offset);
        
        public Span<byte> GetBytes(int address, int length = -1)
            => _memory.Span.Slice(address, length == -1 ? _memory.Length - address : length);

        public void Dispose()
        {
            ReadOnlyMemory<byte> memory = _memory;
            _memory = null;
            if (MemoryMarshal.TryGetArray(memory, out var segment)){
                ArrayPool<byte>.Shared.Return(segment.Array);
            }
        }

        public byte this[int address]
        {
            get => _memory.Span[address];
            set => _memory.Span[address] = value;
        }

        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            // TODO: this needs to be async
            stream.WriteByte((byte)_free);
            stream.WriteByte((byte)(_free >> 8));
            stream.WriteByte((byte)(_free >> 16));
            stream.WriteByte((byte)(_free >> 24));
            stream.Flush(); 
            await stream.WriteAsync(_memory.Slice(0, _free), cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        public void WriteTo(Stream stream, CancellationToken cancellationToken = default)
        {
            stream.WriteByte((byte)_free);
            stream.WriteByte((byte)(_free >> 8));
            stream.WriteByte((byte)(_free >> 16));
            stream.WriteByte((byte)(_free >> 24));
            stream.Write(_memory.Slice(0, _free), cancellationToken);
            stream.Flush();
        }
    }
}
