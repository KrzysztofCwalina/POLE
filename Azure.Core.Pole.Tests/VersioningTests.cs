using NUnit.Framework;
using System;
using System.ComponentModel;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void V1ModelV0Payload()
        {
            // V1 model parses V0 payload
            {
                var heap = new PoleHeap();
                var serverModel = ServerVersionedModel.Allocate(heap, 0);
                serverModel.Number = 5;

                var stream = new MemoryStream();
                heap.WriteTo(stream);
                stream.Position = 0;

                ClientV1Model deserialized = ClientV1Model.Deserialize(stream);
                Assert.AreEqual(5, deserialized.Number);
                Assert.AreEqual(null, deserialized.Multiplier);
            }
        }

        [Test]
        public void V0ModelV1Payload()
        {
            // V0 model parses V1 payload
            {
                var heap = new PoleHeap();
                var serverModel = ServerVersionedModel.Allocate(heap, 1);
                serverModel.Number = 5;

                var stream = new MemoryStream();
                heap.WriteTo(stream);
                stream.Position = 0;

                ClientV0Model deserialized = ClientV0Model.Deserialize(stream);
                Assert.AreEqual(5, deserialized.Number);
            }
        }
    }

    internal struct ModelSchema
    {
        const ulong SchemaId = 0xfe106fc3b2994200;

        public const int NumberOffset = 8;
        public const int MultiplierOffset = 12; // added in V2

        const int SizeV0 = 12;
        const int SizeV1 = 16;

        public static int GetSize(byte version)
        {
            if (version == 0) return SizeV0;
            if (version == 1) return SizeV1;
            throw new ArgumentOutOfRangeException();
        }
        public static bool SchemaMatches(ulong typeId)
        {
            var schema = (typeId & 0xFFFFFFFFFFFFFF00);
            return schema == SchemaId;
        }
       
        public static int GetVersion(ulong typeId) => (byte)typeId;
        public static ulong GetTypeId(byte version) => SchemaId | version;
    }

    public class ClientV0Model
    {
        private readonly PoleReference _reference;
        private ClientV0Model(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientV0Model Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            var type = reference.ReadTypeId();
            if (!ModelSchema.SchemaMatches(type)) throw new InvalidCastException();
            return new(reference);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientV0Model Deserialize(Stream stream)
        {
            var heap = PoleHeap.ReadFrom(stream);
            return Deserialize(heap);
        }

        public int Number => _reference.ReadInt32(ModelSchema.NumberOffset);
    }

    public class ClientV1Model
    {
        private readonly PoleReference _reference;
        private ClientV1Model(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientV1Model Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            var type = reference.ReadTypeId();
            if (!ModelSchema.SchemaMatches(type)) throw new InvalidCastException();
            return new(reference);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientV1Model Deserialize(Stream stream)
        {
            var heap = PoleHeap.ReadFrom(stream);
            return Deserialize(heap);
        }
        public int Number => _reference.ReadInt32(ModelSchema.NumberOffset);
        public int? Multiplier
        {
            get
            {
                if (ModelSchema.GetVersion(Type) > 0) return _reference.ReadInt32(ModelSchema.MultiplierOffset);
                return null;
            }
        }   
        private ulong Type => _reference.ReadUInt64(0);
    }

    public class ServerVersionedModel
    {
        private readonly PoleReference _reference;

        private ServerVersionedModel(PoleReference reference) => _reference = reference;

        public static ServerVersionedModel Allocate(PoleHeap heap, byte version)
        {
            int size = ModelSchema.GetSize(version);
            PoleReference reference = heap.Allocate(size);

            ulong typeId = ModelSchema.GetTypeId(version);
            reference.WriteUInt64(0, typeId);
            return new ServerVersionedModel(reference);
        }

        private ulong Type => _reference.ReadUInt64(0);
        public int Number
        {
            get => _reference.ReadInt32(ModelSchema.NumberOffset);
            set => _reference.WriteInt32(ModelSchema.NumberOffset, value);
        }

        // Added in V2
        public int Multiplier
        {
            get
            {
                if (ModelSchema.GetVersion(Type) < 1) throw new InvalidOperationException("this version does not have Number");
                return _reference.ReadInt32(ModelSchema.MultiplierOffset);
            }

            set
            {
                if (ModelSchema.GetVersion(Type) < 1) throw new InvalidOperationException("this version does not have Number");
                _reference.WriteInt32(ModelSchema.MultiplierOffset, value);
            }
        }
    }
}