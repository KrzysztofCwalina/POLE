using Azure.Core.Pole.TestModels;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void ModelWithArrays()
        {
            var stream = new MemoryStream();

            {
                using PoleHeap heap = new PoleHeap();

                var model = TestModels.Server.ModelWithArray.Allocate(heap);
                var integers = PoleArray<int>.Allocate(heap, 2);
                model.Integers = integers;

                integers[0] = 5;
                integers[1] = 20;

                var strings = PoleArray<Utf8>.Allocate(heap, 2);
                model.Strings = strings;

                strings[0] = Utf8.Allocate(heap, "Hello, ");
                strings[1] = Utf8.Allocate(heap, "World!");

                heap.WriteTo(stream);
            }

            {
                stream.Position = 0;
                using var heap = PoleHeap.ReadFrom(stream);
                ModelWithArray model = ModelWithArray.Deserialize(heap);
                Assert.AreEqual(5, model.Integers[0]);
                Assert.AreEqual(20, model.Integers[1]);
                Assert.AreEqual("Hello, World!", model.Strings[0] + model.Strings[1]);
            }
        }

        [Test]
        public void StringArray()
        {
            var stream = new MemoryStream();
            {
                using PoleHeap heap = new();
                var utf8 = PoleArray<Utf8>.Allocate(heap, 2);
                utf8[0] = Utf8.Allocate(heap, "ABC");
                utf8[1] = Utf8.Allocate(heap, "DEF");

                heap.WriteTo(stream);
                stream.Position = 0;
            }
            {
                using PoleHeap heap = PoleHeap.ReadFrom(stream);
                var strings = new PoleArray<string>(heap.GetAt(0), new PoleType(typeof(string)));
                var s1 = strings[0];
                var s2 = strings[1];
                Assert.AreEqual("ABC", s1);
                Assert.AreEqual("DEF", s2);
            }
        }
        //[Test]
        //public void Dictionary()
        //{
        //    var stream = new MemoryStream();

        //    // write to stream
        //    {
        //        using PoleHeap heap = new PoleHeap();

        //        var model = TestModels.Server.ModelWithMap.Allocate(heap);
        //        IDictionary<string, int> map = heap.AllocateMap<string, int>(2);
        //        model.Map = map;

        //        map["foo"] = 5;
        //        map["bar"] = 345;               
        //    }

        //    {
        //        stream.Position = 0;
        //        using var heap = PoleHeap.ReadFrom(stream);
        //        ModelWithMap model = ModelWithMap.Deserialize(heap);
        //        Assert.AreEqual(5, model.Map["foo"]);
        //        Assert.AreEqual(5, model.Map["bar"]);
        //    }
        //}
    }
}