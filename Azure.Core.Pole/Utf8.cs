// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Text;

namespace Azure.Core.Pole
{
    public readonly struct Utf8 
    {
        readonly ReadOnlySequence<byte> _bytes;
        readonly int _address;

        public Utf8(ReadOnlySequence<byte> bytes, int address)
        {
            _bytes = bytes;
            _address = address;
        } 

        public Utf8(PoleHeap heap, string str)
        {
            var strLength = Encoding.UTF8.GetByteCount(str);
            Sequence<byte> buffer = heap.AllocateBuffer(strLength);
            _address = buffer.Address;

            Span<byte> span = buffer.First.Span.Slice(sizeof(int));
            if (!str.TryEncodeToUtf8(span, out var written))
            {
                throw new NotImplementedException();
            }
            _bytes = buffer.GetBytes();
        }

        public int Address => _address;

        public override string ToString()
        {
            ReadOnlySpan<byte> b = _bytes.ToArray().AsSpan();
            return b.ToStringAsciiNoAlloc();
        }
    }
}
