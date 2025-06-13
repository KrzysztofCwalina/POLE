// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Azure.Core.Pole.Tooling
{
    public class TypeSpecGenerator
    {
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
                
                // Parse the TypeSpec content to extract model information
                var modelInfo = ParseTypeSpecModel(typeSpecContent);
                
                // Generate C# class code
                string csharpCode = GenerateCSharpClass(modelInfo);
                
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

        private static ModelInfo ParseTypeSpecModel(string typeSpecContent)
        {
            // Parse model definition - looking for pattern: model <Name> { ... }
            var modelMatch = Regex.Match(typeSpecContent, @"model\s+(\w+)\s*\{([^}]*)\}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            
            if (!modelMatch.Success)
            {
                throw new ArgumentException("No valid model definition found in TypeSpec file");
            }

            string modelName = modelMatch.Groups[1].Value;
            string modelBody = modelMatch.Groups[2].Value;

            var modelInfo = new ModelInfo { Name = modelName };

            // Parse properties - looking for pattern: <name>: <type>
            var propertyMatches = Regex.Matches(modelBody, @"(\w+)\s*:\s*(\w+)(?:\s*[,;]|\s*$)", RegexOptions.Multiline);
            
            foreach (Match propertyMatch in propertyMatches)
            {
                string propertyName = propertyMatch.Groups[1].Value;
                string propertyType = propertyMatch.Groups[2].Value;

                // Only support the required types and limit to 3 properties
                if (modelInfo.Properties.Count >= 3)
                    break;

                string csharpType = MapTypeSpecTypeToCSharp(propertyType);
                if (csharpType != null)
                {
                    modelInfo.Properties.Add(new PropertyInfo
                    {
                        Name = ConvertToPascalCase(propertyName),
                        Type = csharpType
                    });
                }
            }

            if (modelInfo.Properties.Count == 0)
            {
                throw new ArgumentException("No valid properties found in model definition");
            }

            return modelInfo;
        }

        private static string MapTypeSpecTypeToCSharp(string typeSpecType)
        {
            // Map TypeSpec types to C# types
            return typeSpecType.ToLowerInvariant() switch
            {
                "string" => "string",
                "int32" => "int",
                "boolean" => "bool",
                _ => null // Unsupported type
            };
        }

        private static string ConvertToPascalCase(string camelCase)
        {
            // Convert camelCase to PascalCase (e.g., firstName -> FirstName)
            if (string.IsNullOrEmpty(camelCase))
                return camelCase;

            return char.ToUpperInvariant(camelCase[0]) + camelCase.Substring(1);
        }

        private static string GenerateCSharpClass(ModelInfo modelInfo)
        {
            var sb = new StringBuilder();
            
            // Add file header comment
            sb.AppendLine("// Generated C# class from TypeSpec model");
            sb.AppendLine();
            
            // Generate class definition
            sb.AppendLine($"public class {modelInfo.Name}");
            sb.AppendLine("{");
            
            // Generate properties
            foreach (var property in modelInfo.Properties)
            {
                sb.AppendLine($"    public {property.Type} {property.Name} {{ get; set; }}");
            }
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        // Helper classes for parsing
        private class ModelInfo
        {
            public string Name { get; set; }
            public System.Collections.Generic.List<PropertyInfo> Properties { get; set; } = new System.Collections.Generic.List<PropertyInfo>();
        }

        private class PropertyInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }
    }
}
