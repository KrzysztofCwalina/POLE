// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers.Binary;

namespace Azure.Core.Pole
{
    public readonly struct ReadOnlyPoleReference
    {
        public const int TypeIdOffset = 0;
        public const int ObjectDataOffset = 8;

        readonly int _dataAddress;
        readonly ReadOnlyMemory<byte> _memory;

        public ReadOnlyPoleReference(BinaryData poleData, ulong expectedSchemaId)
        {
            _memory = poleData.ToMemory();
            _dataAddress = PoleHeap.RootOffset + ObjectDataOffset;

            var typeId = this.ReadTypeId();
            if ((typeId & PoleType.SchemaIdMask) != expectedSchemaId) throw new InvalidCastException("invalid cast");
        }

        internal int ObjectAddress => _dataAddress - ObjectDataOffset;
        internal int DataAddress => _dataAddress;

        private ulong ReadTypeId()
            => BinaryPrimitives.ReadUInt64LittleEndian(_memory.Span.Slice(ObjectAddress));

        public int ReadInt32(int offset) => BinaryPrimitives.ReadInt32LittleEndian(_memory.Span.Slice(DataAddress + offset));

        public bool ReadBoolean(int offset) => _memory.Span[DataAddress + offset] != 0;

        public string ReadString(int offset)
        {
            var span = _memory.Span;
            int stringAddress = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(DataAddress + offset));
            int stringLength = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(stringAddress + ObjectDataOffset, sizeof(int)));
            ReadOnlySpan<byte> stringBytes = span.Slice(stringAddress + ObjectDataOffset + sizeof(int), stringLength);
            return stringBytes.ToStringAsciiNoAlloc();
        }

        public override string ToString()
        {
            var typeId = ReadTypeId();
            switch (typeId)
            {
                case PoleType.Int32Id: return "Int32";
                case PoleType.ByteBufferId: return "byte[]";
                case PoleType.ArrayId: return "object[]";
                default: return typeId.ToString();
            }
        }
    }
}
