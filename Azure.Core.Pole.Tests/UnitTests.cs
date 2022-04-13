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
        public void E2E()
        {
            var stream = new MemoryStream();

            // write to stream
            {
                using PoleHeap heap = new PoleHeap();

                HierarchicalModel foo = HierarchicalModel.Allocate(heap);
                ChildModel baz = ChildModel.Allocate(heap);
                foo.Baz = baz; // this will be simplified after https://github.com/dotnet/roslyn/issues/45284 is fixed.

                foo.Foo = 32;
                foo.Bar = true;
                foo.Bag = Utf8.Allocate(heap, "Hello World!");
                baz.Bat = true;

                IList<int> ints = heap.AllocateArray<int>(1);
                ints[0] = 255;
                Assert.AreEqual(255, ints[0]);

                IList<HierarchicalModel> foos = heap.AllocateArray<HierarchicalModel>(1);
                foos[0] = foo;
                Assert.AreEqual("Hello World!", foos[0].Bag.ToString());

                heap.WriteTo(stream);
            }

            // read from stream
            // the following block is close to zero-alloc and close to zero-work
            {
                stream.Position = 0;
                using var heap = PoleHeap.ReadFrom(stream);

                HierarchicalModel model = HierarchicalModel.Deserialize(heap);
                Assert.AreEqual(32, model.Foo);
                Assert.AreEqual(true, model.Bar);
                Assert.AreEqual(true, model.Baz.Bat);
                Assert.AreEqual("Hello World!", model.Bag.ToString());
            }

            // read from byte[],
            // then access model properties (similar code to the block above but with allocation checks)
            // illustrates zero-alloc
            {
                Memory<byte> data = stream.GetBuffer().AsMemory(0, (int)stream.Length);
                Assert.Less(data.Length, 64); // the payload is very small (~50 bytes)

                {
                    var before = GC.GetAllocatedBytesForCurrentThread();
                    var heap = PoleHeap.ReadFrom(data);
                    var allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
                    Assert.Less(allocatted, 256); // allocates the heap object (~40 bytes)

                    before = GC.GetAllocatedBytesForCurrentThread();
                    HierarchicalModel model = HierarchicalModel.Deserialize(heap);
                    allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
                    Assert.AreEqual(0, allocatted);

                    before = GC.GetAllocatedBytesForCurrentThread();
                    int foo = model.Foo;
                    allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
                    Assert.AreEqual(32, foo);
                    Assert.AreEqual(0, allocatted);

                    before = GC.GetAllocatedBytesForCurrentThread();
                    bool bar = model.Bar;
                    allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
                    Assert.AreEqual(true, bar);
                    Assert.AreEqual(0, allocatted);

                    before = GC.GetAllocatedBytesForCurrentThread();
                    bool bat = model.Baz.Bat;
                    allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
                    Assert.AreEqual(true, bat);
                    Assert.AreEqual(0, allocatted);

                    before = GC.GetAllocatedBytesForCurrentThread();
                    Utf8 bag = model.Bag;
                    allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
                    Assert.AreEqual("Hello World!", bag.ToString());
                    Assert.AreEqual(0, allocatted);
                }
            }
        }

        [Test]
        public void SchemaVerification()
        {
            using PoleHeap heap = new PoleHeap();
            Assert.Throws<InvalidCastException>(() => {
                HelloModel hello = HelloModel.Deserialize(heap);
            });
        }
    }
}