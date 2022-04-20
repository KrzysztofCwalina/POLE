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
    public abstract class PoleMemory
    {
        public abstract ReadOnlySpan<byte> ReadBytes(int address, int length = -1);

        public ReadOnlyPoleReference GetRoot() => new(this, HeaderSize);

        protected const int HeaderSize = 4;
    }
    public class SingleSegmentPoleMemory : PoleMemory
    {
        ReadOnlyMemory<byte> _buffer;

        public SingleSegmentPoleMemory(ReadOnlyMemory<byte> data)
        {
            if (data.Length < HeaderSize) throw new InvalidOperationException("bytes don't contain POLE data");
            _buffer = data;
        }

        public SingleSegmentPoleMemory(BinaryData data) : this(data.ToMemory()) { }

        public override ReadOnlySpan<byte> ReadBytes(int address, int length = -1)
            => _buffer.Span.Slice(address, length == -1 ? _buffer.Length - address : length);
    }

    public abstract class PoleHeap : PoleMemory
    {
        public abstract PoleReference Allocate(int size);
        public abstract Span<byte> GetBytes(int address, int length = -1);
        public abstract byte this[int address] { get; set; } // TODO: this is not efficient

        public new PoleReference GetRoot() => new(this, HeaderSize);
    }
    public class PipelineHeap : PoleHeap
    {
        PipeWriter _writer;
        int _written;
        Memory<byte> _buffer;

        public PipelineHeap(PipeWriter writer)
        {
            _writer = writer;
            _buffer = _writer.GetMemory();
            _written = HeaderSize;
        }

        public override PoleReference Allocate(int size)
        {
            var reference = new PoleReference(this, _written);
            _buffer.Slice(_written, size).Span.Fill(0);
            _written += size;
            return reference;
        }

        public override Span<byte> GetBytes(int address, int length = -1)
            => _buffer.Span.Slice(address, length == -1 ? _buffer.Length - address : length);

        public override ReadOnlySpan<byte> ReadBytes(int address, int length = -1)
            => GetBytes(address, length);

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
    }
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
            _written = HeaderSize;
        }
        
        public override PoleReference Allocate(int size)
        {
            var address = _written;
            _written += size;
            return new PoleReference(this, address);
        }

        public override Span<byte> GetBytes(int address, int length = -1)
            => _buffer.Span.Slice(address, length == -1 ? _buffer.Length - address : length);

        public override ReadOnlySpan<byte> ReadBytes(int address, int length = -1)
            => GetBytes(address, length);

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
            int read = stream.Read(memory, HeaderSize, memory.Length - HeaderSize);
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
