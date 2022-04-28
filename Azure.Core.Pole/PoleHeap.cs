// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;

namespace Azure.Core.Pole
{
    public abstract class PoleHeap : IDisposable
    {
        public const int RootAddress = 4; 

        public abstract Reference AllocateObject(int size, ulong typeId);
        public abstract Sequence<byte> AllocateBuffer(int size);
        protected abstract Memory<byte> GetMemoryCore(int address);

        public abstract Span<byte> GetBytes(int address, int length = -1);
        public Reference GetReference(int address) { 
            Memory<byte> memory = GetMemoryCore(address);
            return new Reference(this, memory, address);
        }
        public Reference GetRoot() => GetReference(RootAddress);

        public abstract ReadOnlySequence<byte> GetByteSequence(int address, int length);

        public abstract byte this[int address] { get; set; } // TODO: this is not efficient

        public abstract bool TryComputeLength(out long length);

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool isDisposing) { }
    }
}
