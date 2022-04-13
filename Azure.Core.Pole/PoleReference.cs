using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
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
        
        public bool IsNull => _address == 0;
        public static PoleReference Null => new PoleReference(null, 0);

        public int ReadInt32(int offset) => BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(_address + offset));
        public void WriteInt32(int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(_heap.GetBytes(_address + offset), value);

        public ulong ReadUInt64(int offset) => BinaryPrimitives.ReadUInt64LittleEndian(_heap.GetBytes(_address + offset));
        public void WriteUInt64(int offset, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(_heap.GetBytes(_address + offset), value);

        public bool SchemaEquals(ulong idL, ulong idH)
        {
            if (ReadUInt64(0) != idL) return false;
            if (ReadUInt64(sizeof(ulong)) != idH) return false;
            return true;
        }
        public void WriteSchemaId(ulong idL, ulong idH)
        {
            WriteUInt64(0, idL);
            WriteUInt64(sizeof(ulong), idH);
        }

        public bool ReadBoolean(int offset) => _heap[_address + offset] != 0;
        public void WriteBoolean(int offset, bool value) => _heap[_address + offset] = value ? (byte)1 : (byte)0;
        public PoleReference ReadReference(int offset) => new PoleReference(_heap, BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(_address + offset)));
        public void WriteReference(int offset, PoleReference reference)
        {
            PoleReference existing = ReadReference(offset);
            if (!existing.IsNull) throw new InvalidOperationException("model property can be assigned only once");
            if (!ReferenceEquals(this._heap, reference._heap)) throw new InvalidOperationException("Cross-heap references are not supported");
            BinaryPrimitives.WriteInt32LittleEndian(_heap.GetBytes(_address + offset), reference._address);
        }
        public void WriteString(int offset, string value) => WriteUtf8(offset, Utf8.Allocate(_heap, value));
        public string ReadString(int offset) => ReadUtf8(offset).ToString();

        public void WriteObject<T>(int offset, T value) where T : IObject => WriteReference(offset, value.Reference);
        public Utf8 ReadUtf8(int offset) => new Utf8(this.ReadReference(offset));
        public void WriteUtf8(int offset, Utf8 value) => WriteObject<Utf8>(offset, value);
        public ReadOnlySpan<byte> ReadBytes(int offset)
        {
            var len = BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(_address + offset));
            return _heap.GetBytes(_address + sizeof(int), len);
        }
    }
}
