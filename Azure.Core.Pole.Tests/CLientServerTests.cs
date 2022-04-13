using NUnit.Framework;
using System;
using System.ComponentModel;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void ClientServer()
        {
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
    }

    class Client
    {
        public ClientModel Get(ushort verion)
        {
            var stream = GetCore(verion);
            var model = ClientModel.Deserialize(stream);
            return model;
        }

        private Stream GetCore(ushort version)
        {
            var heap = new PoleHeap();
            var model = ServerModel.Allocate(heap, version);
            model.Number = 5;
            if (version > 1)
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
        public const ulong IdL = 0xfe106fc3b2994231;
        public const ulong IdH = 0xa177d25283a579b5;

        public const int VersionOffset = 16; // TODO: can version be part of ID?
        public const int NumberOffset = 18;
        public const int MultiplierOffset = 22; // added in V2

        public const int SizeV1 = 22;
        public const int SizeV2 = 26;
    }

    public class ClientModel
    {
        private readonly PoleReference _reference;
        private ClientModel(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientModel Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            if (!reference.SchemaEquals(ClientServerSchema.IdL, ClientServerSchema.IdH)) throw new InvalidCastException();
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
                if (Version > 1) return _reference.ReadInt32(ClientServerSchema.MultiplierOffset);
                return null;
            }
        }   
        private ushort Version => _reference.ReadUInt16(ClientServerSchema.VersionOffset);
    }

    public class ServerModel
    {
        private readonly PoleReference _reference;

        private ServerModel(PoleReference reference) => _reference = reference;

        public static ServerModel Allocate(PoleHeap heap, ushort version)
        {
            int size = 0;
            if (version == 1) size = ClientServerSchema.SizeV1;
            else if (version == 2) size = ClientServerSchema.SizeV2;
            else throw new ArgumentOutOfRangeException(nameof(version));

            PoleReference reference = heap.Allocate(size);
            reference.WriteSchemaId(ModelSchema.IdL, ModelSchema.IdH);
            reference.WriteUInt16(ModelSchema.VersionOffset, version);
            return new ServerModel(reference);
        }

        private ushort Version
        {
            get => _reference.ReadUInt16(ClientServerSchema.VersionOffset);
            set => _reference.WriteUInt16(ClientServerSchema.VersionOffset, value);
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
                if (Version < 2) throw new InvalidOperationException("this version does not have Number");
                return _reference.ReadInt32(ClientServerSchema.MultiplierOffset);
            }

            set
            {
                if (Version < 2) throw new InvalidOperationException("this version does not have Number");
                _reference.WriteInt32(ClientServerSchema.MultiplierOffset, value);
            }
        }
    }
}