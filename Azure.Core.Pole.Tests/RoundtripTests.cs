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
                ClientRequestModel model = new ClientRequestModel();
                model.Message = "Hello";
                model.IsEnabled = true;
                model.RepeatCount = 5;

                model.Serialize(stream);
            }

            // server
            {
                using PoleHeap heap = new PoleHeap();
                ServerResponseModel model = ServerResponseModel.Allocate(heap);

                model.Message = Utf8.Allocate(heap, "Hello");
                model.IsEnabled = true;
                model.RepeatCount = 5;

                heap.WriteTo(stream);
            }

            // client
            {
                stream.Position = 0;
                using var heap = PoleHeap.ReadFrom(stream);
                
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
                using var heap = PoleHeap.ReadFrom(stream);
                ServerRequestModel hello = ServerRequestModel.Deserialize(heap);

                Assert.AreEqual("Hi!", hello.Message.ToString());
                Assert.AreEqual(2, hello.RepeatCount);
                Assert.AreEqual(true, hello.IsEnabled);
            }
        }
    }
}