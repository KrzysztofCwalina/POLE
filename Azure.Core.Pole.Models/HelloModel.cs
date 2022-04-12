using Azure.Core.Pole;
using System;

namespace Azure.Core.Pole.TestModels
{
    internal struct HelloModelSchema
    {
        public const int RepeatCountOffset = 0;
        public const int IsEnabledOffset = 4;
        public const int MessageOffset = 5;
        public const int Size = 9;
    }

    public struct HelloModel
    {
        private readonly PoleReference _reference;
        private HelloModel(PoleReference reference) => _reference = reference;

        internal static HelloModel Deserialize(PoleReference reference) => new(reference);

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

        public static HelloModel Allocate(PoleHeap heap) => new(heap.Allocate(HelloModelSchema.Size));

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
