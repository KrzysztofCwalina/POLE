using NUnit.Framework;
using System;
using System.ComponentModel;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void V2ModelV1Payload()
        {
            // V2 model parses V1 payload
            {
                var heap = new PoleHeap();
                var serverModel = ServerVersionedModel.Allocate(heap, 1);
                serverModel.Number = 5;

                var stream = new MemoryStream();
                heap.WriteTo(stream);
                stream.Position = 0;

                ClientV2Model deserialized = ClientV2Model.Deserialize(stream);
                Assert.AreEqual(5, deserialized.Number);
                Assert.AreEqual(null, deserialized.Multiplier);
            }
        }

        [Test]
        public void V1ModelV2Payload()
        {
            // V1 model parses V2 payload
            {
                var heap = new PoleHeap();
                var serverModel = ServerVersionedModel.Allocate(heap, 2);
                serverModel.Number = 5;

                var stream = new MemoryStream();
                heap.WriteTo(stream);
                stream.Position = 0;

                ClientV1Model deserialized = ClientV1Model.Deserialize(stream);
                Assert.AreEqual(5, deserialized.Number);
            }
        }
    }

    internal struct ModelSchema
    {
        public const ulong V1 = 0xfe106fc3b2994231;
        public const ulong V2 = 0xa177d25283a579b5;

        public const int NumberOffset = 8;
        public const int MultiplierOffset = 12; // added in V2

        public const int SizeV1 = 12;
        public const int SizeV2 = 16;

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

    public class ClientV1Model
    {
        private readonly PoleReference _reference;
        private ClientV1Model(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientV1Model Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            var type = reference.ReadTypeId();
            if (type != ModelSchema.V1 && type != ModelSchema.V2) throw new InvalidCastException();
            return new(reference);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientV1Model Deserialize(Stream stream)
        {
            var heap = PoleHeap.ReadFrom(stream);
            return Deserialize(heap);
        }

        public int Number => _reference.ReadInt32(ModelSchema.NumberOffset);
    }

    public class ClientV2Model
    {
        private readonly PoleReference _reference;
        private ClientV2Model(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientV2Model Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            var type = reference.ReadTypeId();
            if (type != ModelSchema.V1 && type != ModelSchema.V2) throw new InvalidCastException();
            return new(reference);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientV2Model Deserialize(Stream stream)
        {
            var heap = PoleHeap.ReadFrom(stream);
            return Deserialize(heap);
        }
        public int Number => _reference.ReadInt32(ModelSchema.NumberOffset);
        public int? Multiplier
        {
            get
            {
                if (Type == ModelSchema.V2) return _reference.ReadInt32(ModelSchema.MultiplierOffset);
                return null;
            }
        }   
        private ulong Type => _reference.ReadUInt64(0);
    }

    public class ServerVersionedModel
    {
        private readonly PoleReference _reference;

        private ServerVersionedModel(PoleReference reference) => _reference = reference;

        public static ServerVersionedModel Allocate(PoleHeap heap, ushort version)
        {
            int size = ModelSchema.GetSize(version);
            PoleReference reference = heap.Allocate(size);

            ulong schemaId = ModelSchema.GetSchema(version);
            reference.WriteUInt64(0, schemaId);
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
                if (Type == ModelSchema.V1) throw new InvalidOperationException("this version does not have Number");
                return _reference.ReadInt32(ModelSchema.MultiplierOffset);
            }

            set
            {
                if (Type == ModelSchema.V1) throw new InvalidOperationException("this version does not have Number");
                _reference.WriteInt32(ModelSchema.MultiplierOffset, value);
            }
        }
    }
}