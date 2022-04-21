using System;

namespace Azure.Core.Pole
{
    public abstract class PoleHeap 
    {
        protected abstract PoleReference Allocate(int size);
        public PoleReference AllocateObject(int size, ulong typeId)
        {
            var reference = Allocate(size);
            reference.WriteTypeId(typeId);
            return reference;
        }
        public PoleReference AllocateBuffer(int size)
        {
            var reference = Allocate(size + 4);
            reference.WriteInt32(0, size);
            return reference;
        }

        public abstract Span<byte> GetBytes(int address, int length = -1);
        public abstract byte this[int address] { get; set; } // TODO: this is not efficient
        public PoleReference GetRoot() => new(this, RootOffset);

        public const int RootOffset = 4;
    }
}
