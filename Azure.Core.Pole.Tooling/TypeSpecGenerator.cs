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
            sb.AppendLine("// Generated C# class from TypeSpec model");
            sb.AppendLine();
            
            // Generate class definition
            sb.AppendLine($"public class {modelInfo.Name}");
            sb.AppendLine("{");
            
            // Generate properties
            foreach (PropertyInfo property in modelInfo.Properties)
            {
                sb.AppendLine($"    public {property.Type} {property.Name} {{ get; set; }}");
            }
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }
    }
}
