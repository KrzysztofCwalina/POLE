using NUnit.Framework;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void ClientServer()
        {
            var client = new Client();

            ClientModel v1 = client.Get(1);
            Assert.AreEqual(5, v1.Number);
            Assert.AreEqual(null, v1.Multiplier);

            ClientModel v2 = client.Get(2);
            Assert.AreEqual(5, v2.Number);
            Assert.AreEqual(2, v2.Multiplier);
        }
    }

    class Client
    {
        public ClientModel Get(ushort apiVersion)
        {
            var stream = MockedGet(apiVersion);
            var model = ClientModel.Deserialize(stream);
            return model;
        }

        private Stream MockedGet(ushort apiVersions)
        {
            var heap = new PoleHeap();
            var model = ServerModel.Allocate(heap, apiVersions);
            model.Number = 5;
            if (apiVersions > 1)
            {
                model.Multiplier = 2;
            }
            var stream = new MemoryStream();
            heap.WriteTo(stream);
            stream.Position = 0;
            return stream;
        }
    }

    internal struct ClientServerSchema
    {
        public const ulong V1 = 0xfe106fc3b2994231;
        public const ulong V2 = 0xfe106fd3b2994231;

        public const int NumberOffset = 16;
        public const int MultiplierOffset = 20; // added in V2

        public const int SizeV1 = 20;
        public const int SizeV2 = 24;

        public static ulong GetSchema(ushort version)
        {
            if (version == 1) return V1;
            if (version == 2) return V2;
            throw new ArgumentOutOfRangeException();
        }
        public static int GetSize(ushort version)
        {
            if (version == 1) return SizeV1;
            if (version == 2) return SizeV2;
            throw new ArgumentOutOfRangeException();
        }
    }

    public class ClientModel
    {
        private readonly PoleReference _reference;
        private ClientModel(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static ClientModel Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            var type = reference.ReadTypeId();
            if (type != ClientServerSchema.V1 && type != ClientServerSchema.V2) throw new InvalidCastException();
            return new(reference);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientModel Deserialize(Stream stream)
        {
            var heap = PoleHeap.ReadFrom(stream);
            return Deserialize(heap);
        }
        public int Number => _reference.ReadInt32(ClientServerSchema.NumberOffset);
        public int? Multiplier
        {
            get
            {
                if (_reference.ReadTypeId() == ClientServerSchema.V1) return null;
                return _reference.ReadInt32(ClientServerSchema.MultiplierOffset);
            }
        }   
    }

    public class ServerModel
    {
        private readonly PoleReference _reference;

        private ServerModel(PoleReference reference) => _reference = reference;

        public static ServerModel Allocate(PoleHeap heap, ushort version)
        {
            int size = ClientServerSchema.GetSize(version);
            PoleReference reference = heap.Allocate(size);
            reference.WriteTypeId(ClientServerSchema.GetSchema(version));
            return new ServerModel(reference);
        }
        public int Number
        {
            get => _reference.ReadInt32(ClientServerSchema.NumberOffset);
            set => _reference.WriteInt32(ClientServerSchema.NumberOffset, value);
        }
        // Added in V2
        public int Multiplier
        {
            get
            {
                if (_reference.ReadTypeId() != ClientServerSchema.V1) return _reference.ReadInt32(ClientServerSchema.MultiplierOffset); 
                else throw new InvalidOperationException("this version does not have Number");

            }

            set
            {
                if (_reference.ReadTypeId() != ClientServerSchema.V1) _reference.WriteInt32(ClientServerSchema.MultiplierOffset, value);
                else throw new InvalidOperationException("this version does not have Number");
            }
        }
    }
}