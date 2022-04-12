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
        const int __TitleOffset = 9;
        const int __Size = 13;

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
        public Utf8 Message
        {
            get => __reference.ReadUtf8(__MessageOffset);
            set => __reference.WriteUtf8(__MessageOffset, value);
        }
        public string Title
        {
            get => __reference.ReadString(__TitleOffset);
            set => __reference.WriteString(__TitleOffset, value);
        }
    }
}
