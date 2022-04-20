using System;
using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Azure.Core.Pole
{
    public readonly struct ReadOnlyPoleReference
    {
        readonly int _address;
        readonly PoleMemory _heap;

        public ReadOnlyPoleReference(PoleMemory memory, int address)
        {
            _heap = memory;
            _address = address;
        }

        public ulong ReadTypeId() => ReadUInt64(0);

        public ulong ReadUInt64(int offset) => BinaryPrimitives.ReadUInt64LittleEndian(_heap.ReadBytes(_address + offset));

        public string ReadString(int offset)
        {
            var bytes = this.ReadByteBuffer(0);
            return bytes.ToStringAsciiNoAlloc();
        }

        public ReadOnlySpan<byte> ReadByteBuffer(int offset)
        {
            var len = BinaryPrimitives.ReadInt32LittleEndian(_heap.ReadBytes(_address + offset));
            return _heap.ReadBytes(_address + sizeof(int), len);
        }
    }
}
