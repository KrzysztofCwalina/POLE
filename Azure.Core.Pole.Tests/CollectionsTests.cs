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
                using ArrayPoolHeap heap = new ArrayPoolHeap();

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
                using var heap = ArrayPoolHeap.ReadFrom(stream);
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
                using ArrayPoolHeap heap = new();
                var utf8 = PoleArray<Utf8>.Allocate(heap, 2);
                utf8[0] = Utf8.Allocate(heap, "ABC");
                utf8[1] = Utf8.Allocate(heap, "DEF");

                heap.WriteTo(stream);
                stream.Position = 0;
            }
            {
                using ArrayPoolHeap heap = ArrayPoolHeap.ReadFrom(stream);
                var strings = new PoleArray<string>(heap.GetRoot());
                var s1 = strings[0];
                var s2 = strings[1];
                Assert.AreEqual("ABC", s1);
                Assert.AreEqual("DEF", s2);
            }
        }

        [Test]
        public void StructArray()
        {
            var stream = new MemoryStream();
            {
                using ArrayPoolHeap heap = new();
                var array = PoleArray<ServerStructModel>.Allocate(heap, 2);
                array[0].Set(1, 2);
                array[1].Set(3, 4);

                heap.WriteTo(stream);
                stream.Position = 0;
            }
            {
                using ArrayPoolHeap heap = ArrayPoolHeap.ReadFrom(stream);
                var array = new PoleArray<ServerStructModel>(heap.GetRoot());
                var item1 = array[0];
                var item2 = array[1];
                Assert.AreEqual(1, item1.A);
                Assert.AreEqual(2, item1.B);
                Assert.AreEqual(3, item2.A);
                Assert.AreEqual(4, item2.B);
            }
        }

        readonly struct ServerStructModel
        {
            public const int AOffset = 0;
            public const int BOffset = 4;
            const int Size = 8;

            readonly PoleReference _reference;

            private ServerStructModel(PoleReference reference) => _reference = reference;

            public void Set(int a, int b) {
                _reference.WriteInt32(AOffset, a);
                _reference.WriteInt32(BOffset, b);
            }
            public int A
            {
                get => _reference.ReadInt32(AOffset);
                set => _reference.WriteInt32(AOffset, value);
            }
            public int B
            {
                get => _reference.ReadInt32(BOffset);
                set => _reference.WriteInt32(BOffset, value);
            }
        }
    }
}