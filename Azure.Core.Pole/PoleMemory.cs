using System;

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
}
