using Azure.Core.Pole.Tooling;
using System;

namespace Azure.Core.Pole.TestModels.Definitions
{
    [Serializable]
    [Generate(ModelVariants.ServerResponse | ModelVariants.ClientOutput)]
    public class HelloModel
    {
        public int RepeatCount { get; }
        public string Message { get; }

        [Version(2)]
        public bool? IsEnabled { get; }
    }
}
