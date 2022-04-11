using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Azure.Core.Pole.TestModels
{
    public struct FooModel : IObject // TODO: how can we remove this?
    {
        const int FooOffset = 0;                        // int
        const int BarOffset = FooOffset + sizeof(int);  // bool
        const int BazOffset = BarOffset + sizeof(byte); // object
        const int BagOffset = BazOffset + sizeof(int);  // object
        const int Size = BagOffset + sizeof(int);

        readonly PoleReference _reference;

        PoleReference IObject.Reference => _reference; 
        private FooModel(PoleReference reference) => _reference = reference;

        // TODO: it would be best if these were on the heap, i.e. heap.Allocate<T>();
        public static FooModel Allocate(PoleHeap heap) => new (heap.Allocate(FooModel.Size));
        public static FooModel Deserialize(PoleHeap heap) => new (heap.GetAt(0));

        public int Foo
        {
            get => _reference.ReadInt32(FooOffset);
            set => _reference.WriteInt32(FooOffset, value);
        }

        public bool Bar
        {
            get => _reference.ReadBoolean(BarOffset);
            set => _reference.WriteBoolean(BarOffset, value);
        }

        public Utf8 Bag
        {
            get => _reference.ReadUtf8(BagOffset);
            set => _reference.WriteUtf8(BagOffset, value);
        }

        public BazModel Baz
        {
            get => new BazModel(_reference.ReadReference(BazOffset));
            set => _reference.WriteObject(BazOffset, value);
        }
    }

    public readonly struct BazModel : IObject
    {
        const int batOffset = 0; // bool
        const int Size = sizeof(byte);

        readonly PoleReference _reference;
        PoleReference IObject.Reference => _reference;
        internal BazModel(PoleReference reference) => _reference = reference;

        public static BazModel Allocate(PoleHeap heap) => new(heap.Allocate(BazModel.Size));

        public bool Bat
        {
            get => _reference.ReadBoolean(batOffset);
            set => _reference.WriteBoolean(batOffset, value);
        }
    }
}
