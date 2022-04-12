using Azure.Core.Pole;
using System;
using System.ComponentModel;

namespace Azure.Core.Pole.TestModels
{
    public struct HelloModel : IObject
    {
        private readonly PoleReference _reference;
        PoleReference IObject.Reference => _reference;
        private HelloModel(PoleReference reference) => _reference = reference;

        const int RepeatCountOffset = 0;
        const int IsEnabledOffset = 4;
        const int MessageOffset = 5;
        const int TitleOffset = 9;

        internal static HelloModel Create(PoleReference reference) => new(reference);

        public int RepeatCount
        {
            get => _reference.ReadInt32(RepeatCountOffset);
        }
        public bool IsEnabled
        {
            get => _reference.ReadBoolean(IsEnabledOffset);
        }
        public Utf8 Message
        {
            get => _reference.ReadUtf8(MessageOffset);
        }
        public string Title
        {
            get => _reference.ReadString(TitleOffset);
        }
    }
}
