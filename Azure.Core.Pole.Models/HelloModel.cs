using Azure.Core.Pole;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace Azure.Core.Pole.TestModels
{
    internal struct HelloModelSchema
    {
        public const ulong IdL = 0xfe106fc3b2994232; // TODO: can Id be just one ulong? Today it's a GUID
        public const ulong IdH = 0xa177d25283a579b6;

        public const int RepeatCountOffset = 16;
        public const int IsEnabledOffset = 20;
        public const int MessageOffset = 21; // TODO: what about alignment?
        public const int Size = 25;
    }

    public struct HelloModel
    {
        private readonly PoleReference _reference;
        private HelloModel(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static HelloModel Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            if (!reference.SchemaEquals(HelloModelSchema.IdL, HelloModelSchema.IdH)) throw new InvalidCastException();
            return new (reference);
        } 
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static HelloModel Deserialize(Stream stream)
        {
            var heap = PoleHeap.ReadFrom(stream);
            return Deserialize(heap);
        }

        public int RepeatCount => _reference.ReadInt32(HelloModelSchema.RepeatCountOffset);

        public bool IsEnabled => _reference.ReadBoolean(HelloModelSchema.IsEnabledOffset);

        public string Message => _reference.ReadString(HelloModelSchema.MessageOffset);
    }
}

namespace Azure.Core.Pole.TestModels.Server
{
    public struct HelloModel
    {
        private readonly PoleReference _reference;
        private HelloModel(PoleReference reference) => _reference = reference;

        public static HelloModel Allocate(PoleHeap heap)
        {
            PoleReference reference = heap.Allocate(HelloModelSchema.Size);
            reference.WriteSchemaId(HelloModelSchema.IdL, HelloModelSchema.IdH);
            return new HelloModel(reference);
        }

        public int RepeatCount
        {
            get => _reference.ReadInt32(HelloModelSchema.RepeatCountOffset);
            set => _reference.WriteInt32(HelloModelSchema.RepeatCountOffset, value);
        }
        public bool IsEnabled
        {
            get => _reference.ReadBoolean(HelloModelSchema.IsEnabledOffset);
            set => _reference.WriteBoolean(HelloModelSchema.IsEnabledOffset, value);
        }
        public string Message
        {
            get => _reference.ReadString(HelloModelSchema.MessageOffset);
            set => _reference.WriteString(HelloModelSchema.MessageOffset, value);
        }
    }
}
