using Azure.Core.Pole;
using System;

namespace Azure.Core.Pole.TestModels.Server
{
    public struct HelloModel
    {
        private readonly PoleReference __reference;
        private HelloModel(PoleReference reference) => __reference = reference;

        const int __RepeatCountOffset = 0;
        const int __IsEnabledOffset = 4;
        const int __MessageOffset = 5;
        const int __Size = 9;

        internal static HelloModel Allocate(PoleHeap heap) => new(heap.Allocate(HelloModel.__Size));
        internal static HelloModel Deserialize(PoleReference reference) => new(reference);

        public int RepeatCount
        {
            get => __reference.ReadInt32(__RepeatCountOffset);
            set => __reference.WriteInt32(__RepeatCountOffset, value);
        }
        public bool IsEnabled
        {
            get => __reference.ReadBoolean(__IsEnabledOffset);
            set => __reference.WriteBoolean(__IsEnabledOffset, value);
        }
        public string Message
        {
            get => __reference.ReadString(__MessageOffset);
            set => __reference.WriteString(__MessageOffset, value);
        }
    }
}
