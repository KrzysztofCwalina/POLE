// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers.Binary;
using System.IO.Pipelines;

namespace Azure.Core.Pole
{
    public class PipelineHeap : PoleHeap
    {
        PipeWriter _writer;
        int _written;
        Memory<byte> _buffer;

        public PipelineHeap(PipeWriter writer)
        {
            _writer = writer;
            _buffer = _writer.GetMemory();
            _written = RootOffset;
        }

        protected override PoleReference AllocateCore(int size)
        {
            var reference = new PoleReference(this, _written);
            _buffer.Slice(_written, size).Span.Fill(0);
            _written += size;
            return reference;
        }

        public override Span<byte> GetBytes(int address, int length = -1)
            => _buffer.Span.Slice(address, length == -1 ? _buffer.Length - address : length);

        public int TotalWritten => _written;

        public override byte this[int address]
        {
            get => _buffer.Span[address];
            set => _buffer.Span[address] = value;
        }

        public void Complete()
        {
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Span, _written);
            _writer.Advance(_written);
            _writer.Complete();
        }

        public override bool TryComputeLength(out long length)
        {
            length = _written;
            return true;
        }
    }
}
