// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Reflection;

namespace Azure.Core.Pole
{
    public static class PoleType
    {
        public const ulong SchemaIdMask = 0xFFFFFFFFFFFFFF00;

        public const ulong Int32Id      = 0x0000000000000100;
        public const ulong ArrayId      = 0x0000000000000200;
        public const ulong ByteBufferId = 0x0000000000000300;
        public const ulong Utf8BufferId = 0x0000000000000400;

        public static bool TryGetSize(Type type, out int size)
        {
            if (type == typeof(int)) size = 4;
            else if (type == typeof(bool)) size = 1;
            else if (type == typeof(string)) size = 4;
            else if (type == typeof(Utf8)) size = 4;
            else
            {
                var sizeField = type.GetField("Size", BindingFlags.NonPublic | BindingFlags.NonPublic | BindingFlags.Static);
                if (sizeField == null || sizeField.FieldType != typeof(int)) goto failed;
                size = (int)sizeField.GetValue(null);
                return true;

                failed:
                size = 0;
                return false;
            }
            return true;
        }
 
        public static T Deserialize<T>(PoleReference reference)
        {
            var type = typeof(T);
            var ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, s_ctorParams, null);
            if (ctor == null) throw new InvalidOperationException($"{typeof(T)} is not a POLE type.");
            return (T)ctor.Invoke(new object[] { reference });
        }
        static readonly Type[] s_ctorParams = new Type[] { typeof(PoleReference) };
    }
}
