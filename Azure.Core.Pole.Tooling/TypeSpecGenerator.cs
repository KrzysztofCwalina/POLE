// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;

namespace Azure.Core.Pole.Tooling
{
    public class TypeSpecGenerator
    {
        public static string Generate(string typeSpecContent)
        {
            try
            {
                // Parse the TypeSpec content to extract model information
                TypeSpecParser parser = new TypeSpecParser();
                ModelInfo modelInfo = parser.ParseModel(typeSpecContent);
                
                // Generate and return C# class code
                return GenerateCSharpClass(modelInfo);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error generating C# code from TypeSpec: {ex.Message}", ex);
            }
        }

        public static void Generate(string source, string destinationFolder)
        {
            try
            {
                // Read the TypeSpec source file
                if (!File.Exists(source))
                {
                    throw new FileNotFoundException($"TypeSpec source file not found: {source}");
                }

                string typeSpecContent = File.ReadAllText(source);
                
                // Generate C# code using in-memory method
                string csharpCode = Generate(typeSpecContent);
                
                // Parse to get model name for filename
                TypeSpecParser parser = new TypeSpecParser();
                ModelInfo modelInfo = parser.ParseModel(typeSpecContent);
                
                // Ensure destination folder exists
                Directory.CreateDirectory(destinationFolder);
                
                // Write the generated C# class to destination folder
                string outputPath = Path.Combine(destinationFolder, $"{modelInfo.Name}.cs");
                File.WriteAllText(outputPath, csharpCode);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error generating C# code from TypeSpec: {ex.Message}", ex);
            }
        }

        private static string GenerateCSharpClass(ModelInfo modelInfo)
        {
            StringBuilder sb = new StringBuilder();
            
            // Add file header comment
            sb.AppendLine("// Generated C# POLE model from TypeSpec");
            sb.AppendLine();
            
            // Add using directives
            sb.AppendLine("using Azure.Core.Pole;");
            sb.AppendLine("using System;");
            sb.AppendLine();
            
            // Generate struct definition
            sb.AppendLine($"public struct {modelInfo.Name}");
            sb.AppendLine("{");
            
            // Generate private reference field
            sb.AppendLine("    private readonly Reference _reference;");
            sb.AppendLine($"    private {modelInfo.Name}(Reference reference) => _reference = reference;");
            sb.AppendLine();
            
            // Generate offset constants
            int offset = 0;
            foreach (PropertyInfo property in modelInfo.Properties)
            {
                sb.AppendLine($"    const int __{property.Name}Offset = {offset};");
                offset += GetTypeSize(property.Type);
            }
            sb.AppendLine($"    const int __Size = {offset};");
            sb.AppendLine();
            
            // Generate Deserialize method
            sb.AppendLine($"    internal static {modelInfo.Name} Deserialize(Reference reference) => new(reference);");
            sb.AppendLine();
            
            // Generate properties
            foreach (PropertyInfo property in modelInfo.Properties)
            {
                sb.AppendLine($"    public {property.Type} {property.Name}");
                sb.AppendLine("    {");
                sb.AppendLine($"        get => _reference.Read{GetReadWriteMethodName(property.Type)}(__{property.Name}Offset);");
                sb.AppendLine($"        set => _reference.Write{GetReadWriteMethodName(property.Type)}(__{property.Name}Offset, value);");
                sb.AppendLine("    }");
            }
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        private static int GetTypeSize(string type)
        {
            return type switch
            {
                "string" => 4,
                "int" => 4,
                "bool" => 1,
                "byte" => 1,
                _ => throw new NotSupportedException($"Type {type} is not supported")
            };
        }
        
        private static string GetReadWriteMethodName(string type)
        {
            return type switch
            {
                "string" => "String",
                "int" => "Int32", 
                "bool" => "Boolean",
                "byte" => "Byte",
                _ => throw new NotSupportedException($"Type {type} is not supported")
            };
        }
    }
}
