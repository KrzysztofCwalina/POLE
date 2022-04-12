using System;

namespace Azure.Core.Pole.TestModels.Definitions
{
    [Serializable]
    public class TestModel
    {
        public bool P1 { get; }
        public int P2 { get; }
        public Utf8 P3 { get; }
        public string P4 { get; }

        public ChildModel P5 { get; }
    }

    public class ChildModel
    {
        public int P1 { get; }
    }

    [Serializable]
    public class OtherModel
    {
        public bool P1 { get; }

        public ChildModel P5 { get; }
    }
}
