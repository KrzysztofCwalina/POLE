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

        PoleReference IObject.Reference => _reference;

        public Utf8(PoleReference reference) => _reference = reference;
        public Utf8(PoleHeap heap, string str)
        {
            var strLength = Encoding.UTF8.GetByteCount(str);
            _reference = heap.AllocateByteBuffer(strLength);
            Span<byte> buffer = heap.GetBytes(_reference.Address + 4, strLength);
            if (!str.TryEncodeToUtf8(buffer, out var written))
            {
                throw new NotImplementedException("this should never happen");
            }               
        }

        public override string ToString()
        {
            var bytes = _reference.ReadByteBuffer(0);
            return bytes.ToStringAsciiNoAlloc();
        }
    }
}
