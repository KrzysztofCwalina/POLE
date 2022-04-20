using System;
using System.Buffers.Binary;

namespace Azure.Core.Pole
{
    public readonly struct ReadOnlyPoleReference
    {
        readonly int _address;
        readonly ReadOnlyMemory<byte> _memory;

        public ReadOnlyPoleReference(ReadOnlyMemory<byte> memory, int address)
        {
            _memory = memory;
            _address = address;
        }

        public ulong ReadTypeId() => ReadUInt64(0);

        public ulong ReadUInt64(int offset) => BinaryPrimitives.ReadUInt64LittleEndian(_memory.Span.Slice(_address + offset));

        public string ReadString(int offset)
        {
            var bytes = this.ReadByteBuffer(0);
            return bytes.ToStringAsciiNoAlloc();
        }

        public ReadOnlySpan<byte> ReadByteBuffer(int offset)
        {
            var span = _memory.Span;
            var len = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(_address + offset));
            return span.Slice(_address + sizeof(int), len);
        }
    }
}
