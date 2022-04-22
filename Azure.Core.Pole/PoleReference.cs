// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers.Binary;
using System.Reflection;

namespace Azure.Core.Pole
{
    public readonly struct PoleReference : IObject
    {
        public const int TypeIdOffset = 0;
        public const int ObjectDataOffset = 8;

        readonly int _dataAddress;
        readonly PoleHeap _heap;

        public PoleReference(PoleHeap heap, int address) 
        {
            _heap = heap;
            _dataAddress = address + ObjectDataOffset;
        }

        public int Address => _dataAddress - ObjectDataOffset;
        internal int DataAddress => _dataAddress;

        internal PoleHeap Heap => _heap;
        
        public bool IsNull => _dataAddress == ObjectDataOffset;

        public ushort ReadUInt16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(_heap.GetBytes(DataAddress + offset));

        public void WriteUInt16(int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(_heap.GetBytes(DataAddress + offset), value);
        public int ReadInt32(int offset) => BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(DataAddress + offset));

        public void WriteInt32(int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(_heap.GetBytes(DataAddress + offset), value);
        public ulong ReadUInt64(int offset) => BinaryPrimitives.ReadUInt64LittleEndian(_heap.GetBytes(DataAddress + offset));
        public void WriteUInt64(int offset, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(_heap.GetBytes(DataAddress + offset), value);
        public bool ReadBoolean(int offset) => _heap[DataAddress + offset] != 0;
        public void WriteBoolean(int offset, bool value) => _heap[DataAddress + offset] = value ? (byte)1 : (byte)0;

        public ulong ReadTypeId() => BinaryPrimitives.ReadUInt64LittleEndian(_heap.GetBytes(Address));
        public void WriteTypeId(ulong typeId) => BinaryPrimitives.WriteUInt64LittleEndian(_heap.GetBytes(Address), typeId);

        public PoleReference ReadReference(int offset) => new PoleReference(_heap, ReadInt32(offset));
        public void WriteReference(int offset, PoleReference reference)
            => WriteAddress(offset, reference.Address, reference._heap);


        public void WriteAddress(int offset, int address, PoleHeap heap = null)
        {
            PoleReference existing = ReadReference(offset);
            if (!existing.IsNull) throw new InvalidOperationException("model property can be assigned only once");
            if (heap != null && !ReferenceEquals(this._heap, heap)) throw new InvalidOperationException("Cross-heap references are not supported");
            WriteInt32(offset, address);
        }

        public void WriteObject<T>(int offset, T value) where T : IObject => WriteAddress(offset, value.Address);

        public void WriteString(int offset, string value) => WriteUtf8(offset, new Utf8(_heap, value));
        public string ReadString(int offset) => ReadUtf8(offset).ToString();

        public Utf8 ReadUtf8(int offset) => new Utf8(ReadReference(offset));
        public void WriteUtf8(int offset, Utf8 value) => WriteObject<Utf8>(offset, value);
        
        public void WriteByteBuffer(int offset, ReadOnlySpan<byte> value, ulong typeId)
        {
            var reference = _heap.AllocateByteBuffer(value.Length, typeId);
            Span<byte> bytes = reference._heap.GetBytes(reference.DataAddress);
            var length = BinaryPrimitives.ReadInt32LittleEndian(bytes);
            var destination = bytes.Slice(4, length);
            value.CopyTo(destination);
            this.WriteReference(offset, reference);
        }
        public ReadOnlySpan<byte> ReadByteBuffer(int offset)
        {
            var len = BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(DataAddress + offset));
            return _heap.GetBytes(DataAddress + sizeof(int), len);
        }

        // TODO: can reflection be eliminated?
        public T Deserialize<T>()
        {
            var ctor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(PoleReference) }, Array.Empty<ParameterModifier>());
            var value = (T)ctor.Invoke(new object[] { this });
            return value;
        }
        public object Deserialize(Type type)
        {
            var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(PoleReference) }, Array.Empty<ParameterModifier>());
            var value = ctor.Invoke(new object[] { this });
            return value;
        }

        public override string ToString()
        {
            var typeId = ReadTypeId();
            switch (typeId)
            {
                case PoleType.Int32Id: return "Int32";
                case PoleType.ByteBufferId: return "byte[]";
                case PoleType.ArrayId: return "object[]";
                case PoleType.Utf8BufferId: return "Utf8";
                default: return typeId.ToString();
            }
        }
    }
}
