// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Azure.Core.Pole
{
    public abstract class PoleHeap : IDisposable
    {
        protected abstract PoleReference AllocateCore(int size);
        public PoleReference AllocateObject(int size, ulong typeId)
        {
            // TODO: throw if the ID conflicts with built-in IDs
            var reference = AllocateCore(size);
            reference.WriteTypeId(typeId);
            return reference;
        }
        public PoleReference AllocateByteBuffer(int size) // TODO: should byte buffer be an object, i.e. have type ID?
        {
            var reference = AllocateCore(size + 4);
            reference.WriteInt32(0, size);
            return reference;
        }

        public abstract Span<byte> GetBytes(int address, int length = -1);
        public abstract byte this[int address] { get; set; } // TODO: this is not efficient
        public PoleReference GetRoot() => new(this, RootOffset);

        public abstract bool TryComputeLength(out long length);

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool isDisposing) { }

        public const int RootOffset = 4; // TODO: should pole heap be a pole object (a dynamic object)?
    }
}
