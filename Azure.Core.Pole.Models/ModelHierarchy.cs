using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Azure.Core.Pole.TestModels
{
    public struct ParentModel : IObject // TODO: how can we remove this?
    {
        public const ulong SchemaId = 0xAFFFFFFFFFFFFF00;

        const int FooOffset = sizeof(ulong);            // int
        const int BarOffset = FooOffset + sizeof(int);  // bool
        const int BazOffset = BarOffset + sizeof(byte); // object
        const int BagOffset = BazOffset + sizeof(int);  // object
        const int Size = BagOffset + sizeof(int);

        readonly PoleReference _reference;

        PoleReference IObject.Reference => _reference; 
        private ParentModel(PoleReference reference) => _reference = reference;

        public static ParentModel Allocate(ArrayPoolHeap heap) => new (heap.AllocateObject(Size, SchemaId));
        public static ParentModel Deserialize(ArrayPoolHeap heap) => new (heap.GetRoot());

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

        public ChildModel Child
        {
            get => new ChildModel(_reference.ReadReference(BazOffset));
            set => _reference.WriteObject(BazOffset, value);
        }
    }

    public readonly struct ChildModel : IObject
    {
        public const ulong SchemaId = 0xBFFFFFFFFFFFFF00;
        const int batOffset = 0; // bool
        const int Size = sizeof(byte);

        readonly PoleReference _reference;
        PoleReference IObject.Reference => _reference;
        internal ChildModel(PoleReference reference) => _reference = reference;

        public static ChildModel Allocate(ArrayPoolHeap heap) => new(heap.AllocateObject(Size, SchemaId));

        public bool Bat
        {
            get => _reference.ReadBoolean(batOffset);
            set => _reference.WriteBoolean(batOffset, value);
        }
    }
}
