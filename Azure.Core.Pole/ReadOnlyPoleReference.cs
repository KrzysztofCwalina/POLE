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

        public ulong ReadTypeId()
            => BinaryPrimitives.ReadUInt64LittleEndian(_memory.Span.Slice(_address));

        public string ReadString(int offset)
        {
            var span = _memory.Span;
            int stringAddress = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(_address + offset));
            int stringLength = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(stringAddress, sizeof(int)));
            ReadOnlySpan<byte> stringBytes = span.Slice(stringAddress + sizeof(int), stringLength);
            return stringBytes.ToStringAsciiNoAlloc();
        }
    }
}
