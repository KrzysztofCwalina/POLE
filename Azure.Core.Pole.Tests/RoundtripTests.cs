// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole.TestModels;
using NUnit.Framework;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void Roundtrip()
        {
            var stream = new MemoryStream();

            // client request
            {
                ClientInputModel model = new ClientInputModel();
                model.Message = "Hello";
                model.IsEnabled = true;
                model.RepeatCount = 5;

                model.Serialize(stream);
            }

            // server
            {
                using ArrayPoolHeap heap = new ArrayPoolHeap();
                ServerResponseModel model = ServerResponseModel.Allocate(heap);

                model.Message = new Utf8(heap, "Hello");
                model.IsEnabled = true;
                model.RepeatCount = 5;

                heap.WriteTo(stream);
            }

            // client
            {
                stream.Position = 0;
                using var heap = ArrayPoolHeap.ReadFrom(stream);
                
                // TODO: add overload to read directly from stream? But who then returns buffers to pool?
                ClientRountripingModel hello = ClientRountripingModel.Deserialize(heap);
                hello.Message = "Hi!";
                hello.RepeatCount = 2;

                stream.Position = 0;
                hello.Serialize(stream);
            }

            // server again
            {
                stream.Position = 0;
                using var heap = ArrayPoolHeap.ReadFrom(stream);
                ServerRequestModel hello = ServerRequestModel.Deserialize(heap);

                Assert.AreEqual("Hi!", hello.Message.ToString());
                Assert.AreEqual(2, hello.RepeatCount);
                Assert.AreEqual(true, hello.IsEnabled);
            }
        }
    }
}