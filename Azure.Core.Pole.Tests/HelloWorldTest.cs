using Azure.Core.Pole.TestModels;
using NUnit.Framework;
using System.IO;

/*
 * Portalble Object Layout Encoding (POLE)
 * 
 * Format for encoding object graphs similar to JSON, but: 
 *  - encoding and decoding are close to zero-alloc, just write bytes into byte[]s
 *  - encoding is close to zero work, i.e. POLE objects have no data fields that need to be serialized; their "fields" are the output buffer.
 *  - decoding is close to zero work, i.e. POLE payload is exatly the memory representation of the POLE object graph
 *  - decoder can read from a sequence of buffers, as opposed to a single contiquous buffer. This enables efficient transimssion without buffer resize-copy.
 *  - decoded object graphs can be all structs, i.e. no heap allocations, but the structs have natural (easy to use) reference semantics
 *  - encoded payloads are typically much smaller than JSON
*/
namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void HelloWorld()
        {
            using var stream = new MemoryStream();

            // write to stream
            {
                using PoleHeap heap = new PoleHeap(); // the heap rents buffers from a pool

                var hello = heap.Allocate<TestModels.Server.HelloModel>();

                hello.Message = Utf8.Allocate(heap, "Hello World!"); // this does not actually allocate anthing on the GC heap.
                hello.RepeatCount = 5;
                SetIsEnabled(hello, true); // hello is a struct (no alloc), but has reference semantics, e.g. can passed to methods that mutate

                hello.Title = "Hello World Sample";

                heap.WriteTo(stream);

                // local method just to illustrate that POLE objects (structs) can be passed just like reference types
                void SetIsEnabled(TestModels.Server.HelloModel hello, bool value) => hello.IsEnabled = value;
            } // heap buffers are returned to the buffer pool here

            Assert.AreEqual(55, stream.Length);

            // read from stream
            {
                stream.Position = 0;
                using var heap = PoleHeap.ReadFrom(stream); // the heap rents buffers from a pool and reads the stream into the buffers

                HelloModel hello = heap.Deserialize<HelloModel>(); // this does not actually "deserialize". it just stores an heap address in the Hello struct 
                
                Assert.IsTrue(hello.IsEnabled); // this just dereferences a bool stored in the heap
                Assert.AreEqual(5, hello.RepeatCount); // same but with an int

                Assert.AreEqual("Hello World Sample", hello.Title);

                if (hello.IsEnabled)
                {
                    for(int i=0; i<hello.RepeatCount; i++)
                    {
                        Assert.AreEqual("Hello World!", hello.Message.ToString());
                    }
                }
            }
        }
    }
}