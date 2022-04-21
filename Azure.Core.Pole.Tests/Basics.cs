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
        public void Basics()
        {
            using var stream = new MemoryStream();
            {
                using ArrayPoolHeap heap = new ArrayPoolHeap(); // the heap rents buffers from a pool
                var hello = new TestModels.Server.HelloModel(heap);
                hello.RepeatCount = 5;
                hello.IsEnabled = true;
                hello.Message = "AAAA"; // this does not actually allocate anthing on the GC heap.
                heap.WriteTo(stream);
            } 
            {
                stream.Position = 0;
                var data = BinaryData.FromStream(stream);

                var hello = new HelloModel(data); 
                
                Assert.IsTrue(hello.IsEnabled); 
                Assert.AreEqual(5, hello.RepeatCount); 
                Assert.AreEqual("AAAA", hello.Message);
            }
        }
    }
}