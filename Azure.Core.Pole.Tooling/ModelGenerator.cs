// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;

namespace Azure.Core.Pole.Tooling
{
    public class PoleGenerator
    {
        string _fileHeader;
        public PoleGenerator(string fileHeader) => _fileHeader = fileHeader;

        public void GenerateClientLibrary(Type type, string folder)
        {
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            var filename = Path.Combine(folder, type.Name + ".cs");
            using var writer = new SourceWriter(filename, _fileHeader);

            GenerateClientType(type, writer);

            GenerateClientOptionsType(type, folder);
        }

        private void GenerateClientOptionsType(Type type, string folder)
        {
            var filename = Path.Combine(folder, type.Name + "Options.cs");
            using var writer = new SourceWriter(filename, _fileHeader);

            writer.WriteLine("using Azure;");
            writer.WriteLine("using Azure.Core;");
            writer.WriteLine();

            writer.WriteLine($"namespace {type.Namespace}");
            writer.WriteLine("{");
            writer.Indent++;

            writer.WriteLine($"public class {type.Name}Options : ClientOptions");
            writer.WriteLine("{");
            writer.Indent++;

            writer.Indent--;
            writer.WriteLine("}"); // EOT

            writer.Indent--;
            writer.WriteLine("}"); // EON
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>list of dependencies</returns>
        private Type[] GenerateClientType(Type type, SourceWriter writer)
        {
            writer.WriteLine("using Azure;");
            writer.WriteLine("using Azure.Core;");
            writer.WriteLine("using Azure.Core.Pipeline;");
            writer.WriteLine();

            writer.WriteLine($"namespace {type.Namespace}");
            writer.WriteLine("{");
            writer.Indent++;

            writer.WriteLine($"public partial class {type.Name}");
            writer.WriteLine("{");
            writer.Indent++;

            writer.WriteLine("private readonly HttpPipeline _pipeline;");
            writer.WriteLine();

            // ctor
            writer.WriteLine($"public {type.Name}({type.Name}Options options = default)");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine($"if (options == null) options = new {type.Name}Options();");
            writer.WriteLine("_pipeline = HttpPipelineBuilder.Build(options);");
            writer.Indent--;
            writer.WriteLine("}");

            foreach (var member in type.GetMembers())
            {
                                                                                    
            }
            writer.Indent--;
            
            writer.WriteLine("}"); // EOT

            writer.Indent--;
            writer.WriteLine("}"); // EON

            return Array.Empty<Type>();
        }

        public void GenerateService(Type type, string folder)
        {
            throw new NotImplementedException();
        }

        public void GenerateModel(Type type, Stream stream)
        {
            if (!type.Namespace.EndsWith("Definitions")) throw new NotSupportedException();
            var modelNamespace = type.Namespace.Substring(0, type.Namespace.Length - ".Definitions".Length);
            string visibility = type.IsVisible ? "public" : "internal";
            var writer = new SourceWriter(new StreamWriter(stream));

            var allocatable = type.GetCustomAttribute<AllocateAttribute>();
            var deserializable = type.GetCustomAttribute<DeserializableAttribute>();
            var serializable = type.GetCustomAttribute<SerializableAttribute>();

            writer.WriteLine("using Azure.Core.Pole;");
            writer.WriteLine("using System;");
            writer.WriteLine("");
            writer.WriteLine($"namespace {modelNamespace}");
            writer.WriteLine("{");
            writer.Indent++;

            writer.WriteLine($"public struct {type.Name}");
            writer.WriteLine($"{{");
            writer.Indent++;

            writer.WriteLine($"private readonly PoleReference _reference;");
            writer.WriteLine($"private {type.Name}(PoleReference reference) => _reference = reference;");
            writer.WriteLine();

            int offset = 0;
            foreach (var property in type.GetProperties())
            {
                writer.WriteLine($"const int __{property.Name}Offset = {offset};");
                var propertyType = property.PropertyType;
                offset += GetTypeSize(propertyType);
            }
            writer.WriteLine($"const int __Size = {offset};");
            writer.WriteLine();

            if (allocatable != null)
            {
                writer.WriteLine($"internal static {type.Name} Allocate(PoleHeap heap) => new (heap.Allocate({type.Name}.__Size));");
            }

            writer.WriteLine($"internal static {type.Name} Deserialize(PoleReference reference) => new (reference);");
            writer.WriteLine();

            foreach (var property in type.GetProperties())
            {
                var propertyType = property.PropertyType;
                writer.WriteLine($"public {GetTypeAlias(propertyType)} {property.Name}");
                writer.WriteLine($"{{");
                writer.Indent++;

                if (property.CanRead)
                {
                    writer.WriteLine($"get => _reference.Read{GetTypeName(property.PropertyType)}(__{property.Name}Offset);");
                }

                if (property.CanWrite)
                {
                    writer.WriteLine($"set => _reference.Write{GetTypeName(property.PropertyType)}(__{property.Name}Offset, value);");
                }

                writer.Indent--;
                writer.WriteLine($"}}");
            }

            writer.Indent--;
            writer.WriteLine($"}}");
            writer.WriteLine("}");
            writer.Flush();
        }
        public void Generate(Assembly definitions, Stream stream)
        {
            Type[] types = definitions.GetTypes();
            foreach(var type in types)
            {
                if (type.IsPublic && type.IsSerializable)
                {
                    GenerateModel(type, stream);
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
