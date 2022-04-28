// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Reflection;

namespace Azure.Core.Pole
{
    public readonly struct Reference : IObject
    {
        public const int TypeIdOffset = 0;
        public const int ObjectDataOffset = 8;

        readonly int _dataAddress;
        readonly PoleHeap _heap;
        readonly Memory<byte> _objectMemory;

        public Reference(PoleHeap heap, Memory<byte> fields, int address) 
        {
            _heap = heap;
            _objectMemory = fields;
            _dataAddress = address + ObjectDataOffset;
        }

        public int Address => _dataAddress - ObjectDataOffset;
        internal int DataAddress => _dataAddress;

        internal PoleHeap Heap => _heap;

        private Span<byte> At(int offset) => _objectMemory.Span.Slice(offset + Reference.ObjectDataOffset);
        public bool IsNull => _dataAddress == ObjectDataOffset;

        public ushort ReadUInt16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(At(offset));

        public void WriteUInt16(int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(At(offset), value);
        public int ReadInt32(int offset) => BinaryPrimitives.ReadInt32LittleEndian(At(offset));

        public void WriteInt32(int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(At(offset), value);
        public ulong ReadUInt64(int offset) => BinaryPrimitives.ReadUInt64LittleEndian(At(offset));
        public void WriteUInt64(int offset, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(At(offset), value);
        public bool ReadBoolean(int offset) => At(offset)[0] != 0;
        public void WriteBoolean(int offset, bool value) => At(offset)[0] = value ? (byte)1 : (byte)0;

        public ulong ReadTypeId() => BinaryPrimitives.ReadUInt64LittleEndian(_objectMemory.Span);

        public Reference ReadReference(int offset)
        {
            var address = ReadInt32(offset);
            return _heap.GetReference(address);
        } 
        public void WriteReference(int offset, Reference reference)
            => WriteAddress(offset, reference.Address, reference._heap);

        public void WriteAddress(int offset, int address, PoleHeap heap = null)
        {
            Reference existing = ReadReference(offset);
            if (!existing.IsNull) throw new InvalidOperationException("model property can be assigned only once");
            if (heap != null && !ReferenceEquals(this._heap, heap)) throw new InvalidOperationException("Cross-heap references are not supported");
            WriteInt32(offset, address);
        }

        public void WriteObject<T>(int offset, T value) where T : IObject => WriteAddress(offset, value.Address);

        public void WriteString(int offset, string value) => WriteUtf8(offset, new Utf8(_heap, value));
        public string ReadString(int offset) => ReadUtf8(offset).ToString();

        public Utf8 ReadUtf8(int offset)
        {
            var (buffer, address) = ReadByteBuffer(offset);
            return new Utf8(buffer, address);
        }
        public void WriteUtf8(int offset, Utf8 value) => WriteAddress(offset, value.Address);
        
        public void WriteByteBuffer(int offset, ReadOnlySpan<byte> value)
        {
            Sequence<byte> buffer = _heap.AllocateBuffer(value.Length);
            buffer.WriteBytes(value);
            this.WriteAddress(offset, buffer.Address);
        }
        public (ReadOnlySequence<byte> bytes, int address) ReadByteBuffer(int offset)
        {
            var address = ReadInt32(offset);

            var len = BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(address));

            var sequence = _heap.GetByteSequence(address + sizeof(int), len);

            return (sequence, address);
        }

        // TODO: can reflection be eliminated?
        public T Deserialize<T>()
        {
            var ctor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(Reference) }, Array.Empty<ParameterModifier>());
            var value = (T)ctor.Invoke(new object[] { this });
            return value;
        }
        public object Deserialize(Type type)
        {
            var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(Reference) }, Array.Empty<ParameterModifier>());
            var value = ctor.Invoke(new object[] { this });
            return value;
        }

        public override string ToString()
        {
            var typeId = ReadTypeId();
            switch (typeId)
            {
                case PoleType.Int32Id: return "Int32";
                case PoleType.ArrayId: return "object[]";
                default: return typeId.ToString();
            }
        }
    }
}
