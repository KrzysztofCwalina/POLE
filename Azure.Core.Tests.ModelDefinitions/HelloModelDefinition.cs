using System;

namespace Azure.Core.Pole.TestModels.Definitions
{
    [Serializable]
    public class HelloModel
    {
        public int RepeatCount { get; }
        public bool IsEnabled { get; }
        public Utf8 Message { get; }
        public string Title { get; }
    }
}
