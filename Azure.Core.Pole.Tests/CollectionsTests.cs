using Azure.Core.Pole.TestModels;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void ArrayOfInt32()
        {
            var stream = new MemoryStream();

            {
                using PoleHeap heap = new PoleHeap();

                var model = TestModels.Server.ModelWithArray.Allocate(heap);
                var list = PoleArray<int>.Allocate(heap, 2);
                model.All = list;

                list[0] = 5;
                list[1] = 20;

                heap.WriteTo(stream);
            }

            {
                stream.Position = 0;
                using var heap = PoleHeap.ReadFrom(stream);
                ModelWithArray model = ModelWithArray.Deserialize(heap);
                Assert.AreEqual(5, model.All[0]);
                Assert.AreEqual(20, model.All[1]);
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