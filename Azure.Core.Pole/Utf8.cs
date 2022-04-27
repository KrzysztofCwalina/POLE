// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Text;

namespace Azure.Core.Pole
{
    public readonly struct ByteBuffer
    {
        readonly Memory<byte> _bytes;
        readonly int _address;

        public ReadOnlySequence<byte> GetBytes() => new ReadOnlySequence<byte>(_bytes);

        public ByteBuffer(Memory<byte> bytes, int address)
        {
            _bytes = bytes;
            _address = address; 
        }

        public void WriteString(string str)
        {
            Span<byte> buffer = _bytes.Span.Slice(sizeof(int));
            if (!str.TryEncodeToUtf8(buffer, out var written))
            {
                throw new NotImplementedException();
            }
        }
        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            Span<byte> buffer = _bytes.Span.Slice(sizeof(int));
            bytes.CopyTo(buffer);
        }

        public int Address => _address;
    }
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
            ByteBuffer buffer = heap.AllocateBuffer(strLength);
            _address = buffer.Address;
            buffer.WriteString(str);
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
