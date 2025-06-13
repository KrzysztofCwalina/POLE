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
            // Read the TypeSpec content from dog.tsp file
            string tspFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tsp", "dog.tsp");
            string typeSpecContent = File.ReadAllText(tspFilePath);
            
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

        [Test]
        public void DogModelCompileAndInstantiate()
        {
            // Read the TypeSpec content from dog.tsp file
            string tspFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tsp", "dog.tsp");
            string typeSpecContent = File.ReadAllText(tspFilePath);
            
            // Generate C# code in-memory
            string generatedCode = TypeSpecGenerator.Generate(typeSpecContent);
            
            // Parse and compile the C# code using Roslyn
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
            CSharpCompilation compilation = CSharpCompilation.Create(
                "TestAssembly",
                new[] { syntaxTree },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Runtime.GCSettings).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                
            // Check for compilation errors
            System.Collections.Generic.IEnumerable<Diagnostic> diagnostics = compilation.GetDiagnostics();
            System.Collections.Generic.IEnumerable<Diagnostic> errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
            Assert.IsFalse(errors.Any(), $"Generated C# code should compile without errors. Errors: {string.Join(", ", errors.Select(e => e.GetMessage()))}");
            
            // Compile to assembly in memory
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Microsoft.CodeAnalysis.Emit.EmitResult emitResult = compilation.Emit(memoryStream);
                Assert.IsTrue(emitResult.Success, "Compilation should succeed");
                
                // Load the compiled assembly
                memoryStream.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(memoryStream.ToArray());
                
                // Get the Dog type from the assembly
                Type? dogType = assembly.GetType("Dog");
                Assert.IsNotNull(dogType, "Dog type should be found in the compiled assembly");
                
                // Create an instance of the Dog class
                object? dogInstance = Activator.CreateInstance(dogType!);
                Assert.IsNotNull(dogInstance, "Should be able to create an instance of Dog");
                
                // Get property info for all properties
                System.Reflection.PropertyInfo? nameProperty = dogType!.GetProperty("Name");
                System.Reflection.PropertyInfo? ageProperty = dogType!.GetProperty("Age");
                System.Reflection.PropertyInfo? isMaleProperty = dogType!.GetProperty("IsMale");
                
                Assert.IsNotNull(nameProperty, "Name property should exist");
                Assert.IsNotNull(ageProperty, "Age property should exist");
                Assert.IsNotNull(isMaleProperty, "IsMale property should exist");
                
                // Verify property types
                Assert.AreEqual(typeof(string), nameProperty!.PropertyType, "Name property should be of type string");
                Assert.AreEqual(typeof(byte), ageProperty!.PropertyType, "Age property should be of type byte");
                Assert.AreEqual(typeof(bool), isMaleProperty!.PropertyType, "IsMale property should be of type bool");
                
                // Set property values
                string testName = "Buddy";
                byte testAge = 5;
                bool testIsMale = true;
                
                nameProperty!.SetValue(dogInstance!, testName);
                ageProperty!.SetValue(dogInstance!, testAge);
                isMaleProperty!.SetValue(dogInstance!, testIsMale);
                
                // Read property values back and verify they match
                string? actualName = (string?)nameProperty!.GetValue(dogInstance!);
                byte actualAge = (byte)ageProperty!.GetValue(dogInstance!)!;
                bool actualIsMale = (bool)isMaleProperty!.GetValue(dogInstance!)!;
                
                Assert.AreEqual(testName, actualName, "Name property should return the set value");
                Assert.AreEqual(testAge, actualAge, "Age property should return the set value");
                Assert.AreEqual(testIsMale, actualIsMale, "IsMale property should return the set value");
            }
        }
    }
}