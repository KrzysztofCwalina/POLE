// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Azure.Core.Pole.Tests
{
    public partial class TypeSpecGeneratorTests
    {
        [Test]
        public void DogModelGeneration()
        {
            // Read the TypeSpec content directly
            string typeSpecContent = @"model Dog {
  name: string;
  age: uint8;
  isMale: boolean;
}";
            
            // Generate C# code in-memory
            string generatedCode = TypeSpecGenerator.Generate(typeSpecContent);
            
            // Parse the C# code using Roslyn
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
            CSharpCompilation compilation = CSharpCompilation.Create(
                "TestAssembly",
                new[] { syntaxTree },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                
            // Check for compilation errors
            System.Collections.Generic.IEnumerable<Diagnostic> diagnostics = compilation.GetDiagnostics();
            System.Collections.Generic.IEnumerable<Diagnostic> errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
            Assert.IsFalse(errors.Any(), $"Generated C# code should compile without errors. Errors: {string.Join(", ", errors.Select(e => e.GetMessage()))}");
            
            // Verify the generated class structure using semantic model
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            INamedTypeSymbol? dogClass = compilation.GetTypeByMetadataName("Dog");
            Assert.IsNotNull(dogClass, "Dog class should be found in the compilation");
            
            // Verify class members
            ISymbol[] members = dogClass!.GetMembers().Where(m => m.Kind == SymbolKind.Property).ToArray();
            Assert.AreEqual(3, members.Length, "Dog class should have exactly 3 properties");
            
            // Verify specific properties
            IPropertySymbol? nameProperty = members.OfType<IPropertySymbol>().FirstOrDefault(p => p.Name == "Name");
            Assert.IsNotNull(nameProperty, "Name property should exist");
            Assert.AreEqual("String", nameProperty!.Type.Name, "Name property should be of type String");
            
            IPropertySymbol? ageProperty = members.OfType<IPropertySymbol>().FirstOrDefault(p => p.Name == "Age");
            Assert.IsNotNull(ageProperty, "Age property should exist");
            Assert.AreEqual("Byte", ageProperty!.Type.Name, "Age property should be of type Byte");
            
            IPropertySymbol? isMaleProperty = members.OfType<IPropertySymbol>().FirstOrDefault(p => p.Name == "IsMale");
            Assert.IsNotNull(isMaleProperty, "IsMale property should exist");
            Assert.AreEqual("Boolean", isMaleProperty!.Type.Name, "IsMale property should be of type Boolean");
        }
    }
}