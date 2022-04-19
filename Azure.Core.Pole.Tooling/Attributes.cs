using System;
using System.IO;
using System.Reflection;

namespace Azure.Core.Pole.Tooling
{
    public class DeserializableAttribute : Attribute { }
    public class AllocateAttribute : Attribute { }
    public class VersionAttribute : Attribute
    {
        public VersionAttribute(int version) => Version = version;

        public int Version { get; private set; }
    }
}
