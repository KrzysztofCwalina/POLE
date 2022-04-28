// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole.TestModels;
using NUnit.Framework;
using System;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void String()
        {
            using var stream = new MemoryStream();
            {
                using ArrayPoolHeap heap = new ArrayPoolHeap();
                var hello = new StringModel(heap);
                hello.Name = "ABC"; 
                heap.WriteTo(stream);
            } 
            {
                stream.Position = 0;
                var hello = new StringModel(stream);         
                Assert.AreEqual("ABC", hello.Name);
            }
        }
    }

    public struct StringModel
    {
        const ulong SchemaId = 0xFFFFFFFFFFFFFFFF;
        const int NameOffset = 0;
        const int Size = 4;

        private readonly Reference _reference;
        private StringModel(Reference reference) => _reference = reference;

        public StringModel(Stream poleData)
        {
            var heap = ArrayPoolHeap.ReadFrom(poleData);
            _reference = heap.GetRoot();
        }

        public StringModel(PoleHeap heap)
            => _reference = heap.AllocateObject(Size, SchemaId);

        public string Name
        {
            get => _reference.ReadString(NameOffset);
            set => _reference.WriteString(NameOffset, value);
        }
    }
}