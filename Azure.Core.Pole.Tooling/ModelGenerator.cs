using System;
using System.IO;
using System.Reflection;

namespace Azure.Core.Pole.Tooling
{
    public class PoleGenerator
    {
        public void Generate(Type type, Stream stream, bool serverSide = false)
        {
            if (!type.Namespace.EndsWith("Definitions")) throw new NotSupportedException();
            var modelNamespace = type.Namespace.Substring(0, type.Namespace.Length - ".Definitions".Length);
            if (serverSide)
            {
                modelNamespace += ".Server";
            }
            var writer = new StreamWriter(stream);

            string indent = "";

            writer.WriteLine("using Azure.Core.Pole;");
            writer.WriteLine("using System;");
            writer.WriteLine("");
            writer.WriteLine($"namespace {modelNamespace}");
            writer.WriteLine("{");

            indent += "    ";
            writer.WriteLine($"{indent}public struct {type.Name} : IObject");
            writer.WriteLine($"{indent}{{");
            indent += "    ";

            writer.WriteLine($"{indent}private readonly PoleReference __reference;");
            writer.WriteLine($"{indent}PoleReference IObject.Reference => __reference;");
            writer.WriteLine($"{indent}private {type.Name}(PoleReference reference) => __reference = reference;");
            writer.WriteLine();

            int offset = 0;
            foreach (var property in type.GetProperties())
            {
                writer.WriteLine($"{indent}const int __{property.Name}Offset = {offset};");
                var propertyType = property.PropertyType;
                offset += GetTypeSize(propertyType);
            }
            if (serverSide)
            {
                writer.WriteLine($"{indent}const int __Size = {offset};");
            }
            writer.WriteLine();

            if (serverSide)
            {
                writer.WriteLine($"{indent}public static {type.Name} Allocate(PoleHeap heap) => new (heap.Allocate({type.Name}.Size));");
            }

            writer.WriteLine($"{indent}public static {type.Name} Deserialize(PoleReference reference) => new (reference);");
            writer.WriteLine();

            foreach (var property in type.GetProperties())
            {
                var propertyType = property.PropertyType;
                writer.WriteLine($"{indent}public {GetTypeAlias(propertyType)} {property.Name}");
                writer.WriteLine($"{indent}{{");
                indent += "    ";

                writer.WriteLine($"{indent}get => _reference.Read{GetTypeName(property.PropertyType)}({property.Name}Offset);");

                if (serverSide)
                {
                    writer.WriteLine($"{indent}set => _reference.Write{GetTypeName(property.PropertyType)}({property.Name}Offset, value);");
                }

                indent = indent.Substring(4);
                writer.WriteLine($"{indent}}}");
            }

            indent = indent.Substring(4);
            writer.WriteLine($"{indent}}}");
            writer.WriteLine("}");
            writer.Flush();
        }
        public void Generate(Assembly definitions, Stream stream, bool serverSide = false)
        {
            Type[] types = definitions.GetTypes();
            foreach(var type in types)
            {
                if (type.IsPublic && type.IsSerializable)
                {
                    Generate(type, stream, serverSide);
                }
            }
        }

        private int GetTypeSize(Type type)
        {
            if (type == typeof(string)) return 4;
            if (type == typeof(Utf8)) return 4;
            if (type == typeof(int)) return 4;
            if (type == typeof(bool)) return 1;
            throw new NotSupportedException();
        }

        private string GetTypeAlias(Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(Utf8)) return "Utf8";
            if (type == typeof(int)) return "int";
            if (type == typeof(bool)) return "bool";
            throw new NotSupportedException();
        }
        private string GetTypeName(Type type)
        {
            if (type == typeof(string)) return "String";
            if (type == typeof(Utf8)) return "Utf8";
            if (type == typeof(int)) return "Int32";
            if (type == typeof(bool)) return "Boolean";
            throw new NotSupportedException();
        }
    }
}
