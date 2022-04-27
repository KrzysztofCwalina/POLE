// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;

namespace Azure.Core.Pole
{
    public abstract class PoleHeap : IDisposable
    {
        public const int LengthOffset = 0;
        public const int RootOffset = 4; 

        public abstract Reference AllocateObject(int size, ulong typeId);
        public abstract ByteBuffer AllocateBuffer(int size);

        public abstract Span<byte> GetBytes(int address, int length = -1);
        public abstract ReadOnlySequence<byte> GetByteSequence(int address, int length);

        public abstract byte this[int address] { get; set; } // TODO: this is not efficient
        public Reference GetRoot() => new(this, RootOffset);

        public abstract bool TryComputeLength(out long length);

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool isDisposing) { }
    }
}
