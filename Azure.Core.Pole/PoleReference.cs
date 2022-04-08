using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Azure.Core.Pole
{
    public readonly struct PoleReference
    {
        readonly int _address;
        readonly PoleHeap _heap;

        public PoleReference(PoleHeap heap, int address) : this()
        {
            _heap = heap;
            _address = address;
        }

        internal int Address => _address;
        internal PoleHeap Heap => _heap;
        
        public int ReadInt32(int offset) => BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(_address + offset));
        public void WriteInt32(int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(_heap.GetBytes(_address + offset), value);
        public bool ReadBoolean(int offset) => _heap[_address + offset] != 0;
        public void WriteBoolean(int offset, bool value) => _heap[_address + offset] = value ? (byte)1 : (byte)0;
        public PoleReference ReadReference(int offset) => new PoleReference(_heap, BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(_address + offset)));
        public void WriteReference(int offset, PoleReference reference) => BinaryPrimitives.WriteInt32LittleEndian(_heap.GetBytes(_address + offset), reference._address);
        public void WriteObject<T>(int offset, T value) where T : IObject => WriteReference(offset, value.Reference);
        public Utf8 ReadString(int offset) => new Utf8(this.ReadReference(offset));
        public void WriteString(int offset, Utf8 value) => WriteObject<Utf8>(offset, value);
        public ReadOnlySpan<byte> ReadBytes(int offset)
        {
            var len = BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(_address + offset));
            return _heap.GetBytes(_address + sizeof(int), len);
        }
    }
}
