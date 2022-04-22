// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Text;

namespace Azure.Core.Pole
{
    public readonly struct Utf8 : IObject
    {
        readonly PoleReference _reference;
        readonly ReadOnlyPoleReference _roreference; // TODO: the fact that we have both references is a massive hack
        readonly bool _isRo;

        int IObject.Address => _reference.Address;

        public Utf8(ReadOnlyPoleReference reference)
        {
            _roreference = reference;
            _reference = default;
            _isRo = true;
        }

        public Utf8(PoleReference reference)
        {
            _reference = reference;
            _roreference = default;
            _isRo = false;
        } 

        public Utf8(PoleHeap heap, string str)
        {
            var strLength = Encoding.UTF8.GetByteCount(str);
            _reference = heap.AllocateByteBuffer(strLength, PoleType.Utf8BufferId);
            Span<byte> buffer = heap.GetBytes(_reference.DataAddress + 4, strLength);
            if (!str.TryEncodeToUtf8(buffer, out var written))
            {
                throw new NotImplementedException("this should never happen");
            }
            _roreference = default;
            _isRo = false;
        }

        public override string ToString()
        {
            if (!_isRo)
            {
                var bytes = _reference.ReadByteBuffer(0);
                return bytes.ToStringAsciiNoAlloc();
            }
            else
            {
                var bytes = _roreference.ReadByteBuffer(0);
                return bytes.ToStringAsciiNoAlloc();
            }
        }
    }
}
