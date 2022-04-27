// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;

namespace Azure.Core.Pole
{
    public readonly struct Sequence<T>
    {
        readonly Memory<T> _bytes;
        readonly int _address;

        public ReadOnlySequence<T> GetBytes() => new ReadOnlySequence<T>(_bytes);

        public Memory<T> First => _bytes;
        public Sequence(Memory<T> bytes, int address)
        {
            _bytes = bytes;
            _address = address; 
        }

        public void WriteBytes(ReadOnlySpan<T> bytes)
        {
            Span<T> buffer = _bytes.Span.Slice(sizeof(int));
            bytes.CopyTo(buffer);
        }

        public int Address => _address;
        public int Length => _bytes.Length;
    }
}
