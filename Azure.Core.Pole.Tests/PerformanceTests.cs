using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Azure.Core.Pole.Tests
{
    public class PerformanceTests
    {
        byte[] s_hello = Encoding.UTF8.GetBytes("Hello World!");
        [Test]
        public void PoleIsSmallerThanJson()
        {
            int poleLength = 0;
            int jsonLength = 0;
            {
                //const ushort fooId = 1;
                //const ushort barId = 2;
                //const ushort bazId = 3;
                //ReadOnlySpan<byte> hello = s_hello;

                //var writer = new PoleBuilder();
                //ulong helloId = writer.AddString(s_hello);

                //ushort outer = writer.StartObject();            //   {   
                //writer.AddInt32Property(fooId, 32);             //     foo: 32
                //writer.AddObjectProperty(barId);                //     bar : {
                //writer.AddBooleanProperty(bazId, true);         //       baz : true
                //writer.EndObject(outer, isList: false);         //     }
                //writer.AddStringProperty(bazId, helloId);       //     baz : "Hello World!"
                //writer.EndObject(0, isList: true);              //   }

                //StreamExtensions document = writer.Build();

                //MemoryStream stream = new MemoryStream();
                //document.WriteTo(stream);
                //document.Dispose();
                //poleLength = (int)stream.Length;
            }
            {
                var stream = new MemoryStream();
                var writer = new Utf8JsonWriter(stream);
                writer.WriteStartObject();
                writer.WriteNumber("foo", 32);
                writer.WriteStartObject("bar");
                writer.WriteBoolean("baz", true);
                writer.WriteEndObject();
                writer.WriteString("baz", "Hello World!");
                writer.WriteEndObject();
                writer.Flush();

                jsonLength = (int)stream.Length;
                stream.Position = 0;

                var document = JsonDocument.Parse(stream);
            }

            Assert.AreEqual(108, poleLength);
            Assert.AreEqual(50, jsonLength);
        }
    }
}