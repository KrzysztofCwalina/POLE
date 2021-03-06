// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole.TestModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void Hierarchy()
        {
            var stream = new MemoryStream();
            {
                using ArrayPoolHeap heap = new ArrayPoolHeap();

                ParentModel parent = ParentModel.Allocate(heap);
                ChildModel child = ChildModel.Allocate(heap);
                parent.Child = child; // this will be simplified after https://github.com/dotnet/roslyn/issues/45284 is fixed.

                parent.Foo = 32;
                parent.Bar = true;
                parent.Bag = new Utf8(heap, "Hello World!");
                child.Bat = true;

                heap.WriteTo(stream);
            }
            {
                stream.Position = 0;
                using var heap = ArrayPoolHeap.ReadFrom(stream);

                ParentModel deserializedParent = ParentModel.Deserialize(heap);
                Assert.AreEqual(32, deserializedParent.Foo);
                Assert.AreEqual(true, deserializedParent.Bar);
                Assert.AreEqual(true, deserializedParent.Child.Bat);
                Assert.AreEqual("Hello World!", deserializedParent.Bag.ToString());
            }
        }
    }
}