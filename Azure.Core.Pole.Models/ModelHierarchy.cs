﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Azure.Core.Pole.TestModels
{
    public struct ParentModel : IObject // TODO: how can we remove this?
    {
        public const ulong SchemaId = 0xFFFFFFFFFFFFFE00;

        const int FooOffset = 0;                         // int
        const int BarOffset = FooOffset + sizeof(int);  // bool
        const int BazOffset = BarOffset + sizeof(byte); // object
        const int BagOffset = BazOffset + sizeof(int);  // object
        const int Size = BagOffset + sizeof(int);

        readonly Reference _reference;

        int IObject.Address => _reference.Address; 

        private ParentModel(Reference reference) => _reference = reference;

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
        public const ulong SchemaId = 0xFFFFFFFFFFFFFD00;
        const int BatOffset = 0; // bool
        const int Size = sizeof(byte);

        readonly Reference _reference;
        int IObject.Address => _reference.Address;
        internal ChildModel(Reference reference) => _reference = reference;

        public static ChildModel Allocate(ArrayPoolHeap heap) => new(heap.AllocateObject(Size, SchemaId));

        public bool Bat
        {
            get => _reference.ReadBoolean(BatOffset);
            set => _reference.WriteBoolean(BatOffset, value);
        }
    }
}
