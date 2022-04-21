// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers.Binary;

namespace Azure.Core.Pole
{
    public readonly struct ReadOnlyPoleReference
    {
        readonly int _address;
        readonly ReadOnlyMemory<byte> _memory;

        public ReadOnlyPoleReference(BinaryData poleData, ulong schemaId)
        {
            _memory = poleData.ToMemory();
            _address = PoleHeap.RootOffset;

            var typeId = this.ReadTypeId();
            if ((typeId & 0xFFFFFFFFFFFFFF00) != schemaId) throw new InvalidCastException("invalid cast");
        }

        public ReadOnlyPoleReference(ReadOnlyMemory<byte> memory, int address)
        {
            _memory = memory;
            _address = address;
        }

        public ulong ReadTypeId()
            => BinaryPrimitives.ReadUInt64LittleEndian(_memory.Span.Slice(_address));

        public int ReadInt32(int offset) => BinaryPrimitives.ReadInt32LittleEndian(_memory.Span.Slice(_address + offset));

        public bool ReadBoolean(int offset) => _memory.Span[_address + offset] != 0;

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
