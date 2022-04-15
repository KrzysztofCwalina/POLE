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
                using PoleHeap heap = new PoleHeap();

                ModelHierarchy parent = ModelHierarchy.Allocate(heap);
                ChildModel child = ChildModel.Allocate(heap);
                parent.Child = child; // this will be simplified after https://github.com/dotnet/roslyn/issues/45284 is fixed.

                parent.Foo = 32;
                parent.Bar = true;
                parent.Bag = Utf8.Allocate(heap, "Hello World!");
                child.Bat = true;

                heap.WriteTo(stream);
            }
            {
                stream.Position = 0;
                using var heap = PoleHeap.ReadFrom(stream);

                ModelHierarchy model = ModelHierarchy.Deserialize(heap);
                Assert.AreEqual(32, model.Foo);
                Assert.AreEqual(true, model.Bar);
                Assert.AreEqual(true, model.Child.Bat);
                Assert.AreEqual("Hello World!", model.Bag.ToString());
            }
        }
    }
}