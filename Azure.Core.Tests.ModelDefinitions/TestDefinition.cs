using System;

namespace Azure.Core.Pole.TestModels.Definitions
{
    public class TestModel
    {
        public bool P1 { get; }
        public int P2 { get; }
        public Utf8 P3 { get; }
        public string P4 { get; }

        public ModelChild P5 { get; }
    }

    public class ModelChild
    {
        public int P1 { get; }
    }
}
