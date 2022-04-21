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

        readonly int _objectAddress; // TODO: should this be data address, and ObjectAddress (below) be computed?
        readonly ReadOnlyMemory<byte> _memory;

        public ReadOnlyPoleReference(BinaryData poleData, ulong schemaId)
        {
            _memory = poleData.ToMemory();
            _objectAddress = PoleHeap.RootOffset;

            var typeId = this.ReadTypeId();
            if ((typeId & PoleType.SchemaIdMask) != schemaId) throw new InvalidCastException("invalid cast");
        }

        internal int ObjectAddress => _objectAddress;
        internal int DataAddress => _objectAddress + ObjectDataOffset;

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
    }
}
