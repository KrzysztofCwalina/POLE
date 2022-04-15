using System;
using System.ComponentModel;

namespace Azure.Core.Pole
{
    public readonly struct PoleType
    {
        readonly Type _type;
        readonly int _size;

        public PoleType(Type type)
        {
            if (!TryGetSize(type, out _size)) throw new NotSupportedException($"Type {type.Name} is not supported");
            _type = type;
        }

        public int Size => _size;

        public static bool TryGetSize(Type poleType, out int size)
        {
            if (poleType == typeof(int)) size = 4;
            else if (poleType == typeof(bool)) size = 1;
            else if (poleType == typeof(string)) size = 4;
            else if (poleType == typeof(Utf8)) size = 4;
            else
            {
                size = 0;
                return false;
            }
            return true;
        }
    }
}
