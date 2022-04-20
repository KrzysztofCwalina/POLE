using System;

namespace Azure.Core.Pole
{
    public abstract class PoleHeap 
    {
        public abstract PoleReference Allocate(int size);
        public abstract Span<byte> GetBytes(int address, int length = -1);
        public abstract byte this[int address] { get; set; } // TODO: this is not efficient
        public PoleReference GetRoot() => new(this, RootOffset);

        public const int RootOffset = 4;
    }
}
