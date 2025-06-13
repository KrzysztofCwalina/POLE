// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Azure.Core.Pole.Tooling
{
    public class TypeSpecParser
    {
        public ModelInfo ParseModel(string typeSpecContent)
        {
            if (string.IsNullOrWhiteSpace(typeSpecContent))
            {
                throw new ArgumentException("TypeSpec content cannot be null or empty");
            }

            // Remove comments and normalize whitespace
            string normalizedContent = RemoveComments(typeSpecContent);
            
            // Find model definition
            ModelInfo modelInfo = ExtractModelDefinition(normalizedContent);
            
            if (modelInfo.Properties.Count == 0)
            {
                throw new ArgumentException("No valid properties found in model definition");
            }

            return modelInfo;
        }

        private string RemoveComments(string content)
        {
            // Remove single-line comments (//)
            string[] lines = content.Split('\n');
            List<string> cleanLines = new List<string>();
            
            foreach (string line in lines)
            {
                int commentIndex = line.IndexOf("//");
                if (commentIndex >= 0)
                {
                    cleanLines.Add(line.Substring(0, commentIndex));
                }
                else
                {
                    cleanLines.Add(line);
                }
            }
            
            return string.Join("\n", cleanLines);
        }

        private ModelInfo ExtractModelDefinition(string content)
        {
            // Find the start of model definition
            int modelIndex = FindModelKeyword(content);
            if (modelIndex == -1)
            {
                throw new ArgumentException("No model definition found in TypeSpec file");
            }

            // Extract model name
            string modelName = ExtractModelName(content, modelIndex);
            
            // Find the model body within braces
            int openBraceIndex = content.IndexOf('{', modelIndex);
            if (openBraceIndex == -1)
            {
                throw new ArgumentException("Invalid model definition - missing opening brace");
            }

            int closeBraceIndex = FindMatchingBrace(content, openBraceIndex);
            if (closeBraceIndex == -1)
            {
                throw new ArgumentException("Invalid model definition - missing closing brace");
            }

            string modelBody = content.Substring(openBraceIndex + 1, closeBraceIndex - openBraceIndex - 1);
            
            ModelInfo modelInfo = new ModelInfo { Name = modelName };
            ExtractProperties(modelBody, modelInfo);
            
            return modelInfo;
        }

        private int FindModelKeyword(string content)
        {
            string lowerContent = content.ToLowerInvariant();
            int index = 0;
            
            while (index < lowerContent.Length)
            {
                int modelIndex = lowerContent.IndexOf("model", index);
                if (modelIndex == -1) return -1;
                
                // Check if "model" is a standalone word
                bool isStartOfWord = modelIndex == 0 || !char.IsLetterOrDigit(content[modelIndex - 1]);
                bool isEndOfWord = modelIndex + 5 >= content.Length || !char.IsLetterOrDigit(content[modelIndex + 5]);
                
                if (isStartOfWord && isEndOfWord)
                {
                    return modelIndex;
                }
                
                index = modelIndex + 1;
            }
            
            return -1;
        }

        private string ExtractModelName(string content, int modelIndex)
        {
            int nameStart = modelIndex + 5; // Skip "model"
            
            // Skip whitespace
            while (nameStart < content.Length && char.IsWhiteSpace(content[nameStart]))
            {
                nameStart++;
            }
            
            if (nameStart >= content.Length)
            {
                throw new ArgumentException("Invalid model definition - missing model name");
            }
            
            // Extract name until whitespace or '{'
            int nameEnd = nameStart;
            while (nameEnd < content.Length && 
                   char.IsLetterOrDigit(content[nameEnd]) || content[nameEnd] == '_')
            {
                nameEnd++;
            }
            
            if (nameEnd == nameStart)
            {
                throw new ArgumentException("Invalid model definition - empty model name");
            }
            
            return content.Substring(nameStart, nameEnd - nameStart);
        }

        private int FindMatchingBrace(string content, int openBraceIndex)
        {
            int braceCount = 1;
            int index = openBraceIndex + 1;
            
            while (index < content.Length && braceCount > 0)
            {
                if (content[index] == '{')
                {
                    braceCount++;
                }
                else if (content[index] == '}')
                {
                    braceCount--;
                }
                index++;
            }
            
            return braceCount == 0 ? index - 1 : -1;
        }

        private void ExtractProperties(string modelBody, ModelInfo modelInfo)
        {
            // Split the model body into potential property lines
            string[] lines = modelBody.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;
                
                PropertyInfo propertyInfo = ParsePropertyLine(trimmedLine);
                if (propertyInfo != null)
                {
                    modelInfo.Properties.Add(propertyInfo);
                }
            }
        }

        private PropertyInfo ParsePropertyLine(string line)
        {
            // Remove trailing semicolon or comma
            string cleanLine = line.TrimEnd(';', ',').Trim();
            
            // Find the colon separator
            int colonIndex = cleanLine.IndexOf(':');
            if (colonIndex == -1) return null;
            
            string propertyName = cleanLine.Substring(0, colonIndex).Trim();
            string propertyType = cleanLine.Substring(colonIndex + 1).Trim();
            
            // Validate property name (simple identifier)
            if (!IsValidIdentifier(propertyName)) return null;
            
            // Map TypeSpec type to C# type
            string csharpType = MapTypeSpecTypeToCSharp(propertyType);
            if (csharpType == null) return null;
            
            return new PropertyInfo
            {
                Name = ConvertToPascalCase(propertyName),
                Type = csharpType
            };
        }

        private bool IsValidIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) return false;
            if (!char.IsLetter(identifier[0]) && identifier[0] != '_') return false;
            
            return identifier.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        private string MapTypeSpecTypeToCSharp(string typeSpecType)
        {
            // Map TypeSpec types to C# types
            return typeSpecType.ToLowerInvariant() switch
            {
                "string" => "string",
                "int32" => "int",
                "uint8" => "byte",
                "boolean" => "bool",
                _ => null // Unsupported type
            };
        }

        private string ConvertToPascalCase(string camelCase)
        {
            // Convert camelCase to PascalCase (e.g., firstName -> FirstName)
            if (string.IsNullOrEmpty(camelCase))
                return camelCase;

            return char.ToUpperInvariant(camelCase[0]) + camelCase.Substring(1);
        }
    }

    // Helper classes for parsing
    public class ModelInfo
    {
        public string Name { get; set; } = string.Empty;
        public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
    }

    public class PropertyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}