using System;

namespace Azure.Core.Pole.TestModels.Definitions
{
    [Serializable]
    public class HelloModel
    {
        public int RepeatCount { get; }
        public bool IsEnabled { get; }
        public string Message { get; }
    }
}
