using System;

namespace Azure.Core.Pole.TestModels.Definitions
{
    public class HelloModel
    {
        public int RepeatCount { get; set; }
        public bool IsEnabled { get; set; }
        public Utf8 Message { get; set; }
        public string Title { get; set; }
    }
}
