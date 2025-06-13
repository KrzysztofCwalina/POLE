// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;

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
                TypeSpecParser parser = new TypeSpecParser();
                ModelInfo modelInfo = parser.ParseModel(typeSpecContent);
                
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
