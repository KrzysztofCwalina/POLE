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
        public const ulong IdL = 0xfe106fc3b2994231;
        public const ulong IdH = 0xa177d25283a579b5;

        public const int VersionOffset = 16; // TODO: can version be part of ID?
        public const int NumberOffset = 18;
        public const int MultiplierOffset = 22; // added in V2

        public const int SizeV1 = 22;
        public const int SizeV2 = 26;
    }

    public class ClientV1Model
    {
        private readonly PoleReference _reference;
        private ClientV1Model(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientV1Model Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            if (!reference.SchemaEquals(ModelSchema.IdL, ModelSchema.IdH)) throw new InvalidCastException();
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
            if (!reference.SchemaEquals(ModelSchema.IdL, ModelSchema.IdH)) throw new InvalidCastException();
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
                if (Version > 1) return _reference.ReadInt32(ModelSchema.MultiplierOffset);
                return null;
            }
        }   
        private ushort Version => _reference.ReadUInt16(ModelSchema.VersionOffset);
    }

    public class ServerVersionedModel
    {
        private readonly PoleReference _reference;

        private ServerVersionedModel(PoleReference reference) => _reference = reference;

        public static ServerVersionedModel Allocate(PoleHeap heap, ushort version)
        {
            int size = 0;
            if (version == 1) size = ModelSchema.SizeV1;
            else if (version == 2) size = ModelSchema.SizeV2;
            else throw new ArgumentOutOfRangeException(nameof(version));

            PoleReference reference = heap.Allocate(size);
            reference.WriteSchemaId(ModelSchema.IdL, ModelSchema.IdH);
            reference.WriteUInt16(ModelSchema.VersionOffset, version);
            return new ServerVersionedModel(reference);
        }

        private ushort Version
        {
            get => _reference.ReadUInt16(ModelSchema.VersionOffset);
            set => _reference.WriteUInt16(ModelSchema.VersionOffset, value);
        }
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
                if (Version < 2) throw new InvalidOperationException("this version does not have Number");
                return _reference.ReadInt32(ModelSchema.MultiplierOffset);
            }

            set
            {
                if (Version < 2) throw new InvalidOperationException("this version does not have Number");
                _reference.WriteInt32(ModelSchema.MultiplierOffset, value);
            }
        }
    }
}