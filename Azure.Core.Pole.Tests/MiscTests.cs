using Azure.Core.Pole.TestModels;
using NUnit.Framework;
using System;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void SchemaVerification()
        {
            using PoleHeap heap = new PoleHeap();
            Assert.Throws<InvalidCastException>(() => {
                HelloModel hello = HelloModel.Deserialize(heap);
            });
        }

        //[Test]
        //public void Allocations()
        //{
        //    {
        //        var stream = new MemoryStream();
        //        Memory<byte> data = stream.GetBuffer().AsMemory(0, (int)stream.Length);
        //        Assert.Less(data.Length, 64); // the payload is very small (~50 bytes)

        //        {
        //            var before = GC.GetAllocatedBytesForCurrentThread();
        //            var heap = PoleHeap.ReadFrom(data);
        //            var allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
        //            Assert.Less(allocatted, 256); // allocates the heap object (~40 bytes)

        //            before = GC.GetAllocatedBytesForCurrentThread();
        //            ModelHierarchy model = ModelHierarchy.Deserialize(heap);
        //            allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
        //            Assert.AreEqual(0, allocatted);

        //            before = GC.GetAllocatedBytesForCurrentThread();
        //            int foo = model.Foo;
        //            allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
        //            Assert.AreEqual(32, foo);
        //            Assert.AreEqual(0, allocatted);

        //            before = GC.GetAllocatedBytesForCurrentThread();
        //            bool bar = model.Bar;
        //            allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
        //            Assert.AreEqual(true, bar);
        //            Assert.AreEqual(0, allocatted);

        //            before = GC.GetAllocatedBytesForCurrentThread();
        //            bool bat = model.Child.Bat;
        //            allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
        //            Assert.AreEqual(true, bat);
        //            Assert.AreEqual(0, allocatted);

        //            before = GC.GetAllocatedBytesForCurrentThread();
        //            Utf8 bag = model.Bag;
        //            allocatted = GC.GetAllocatedBytesForCurrentThread() - before;
        //            Assert.AreEqual("Hello World!", bag.ToString());
        //            Assert.AreEqual(0, allocatted);
        //        }
        //    }
        //}
    }
}