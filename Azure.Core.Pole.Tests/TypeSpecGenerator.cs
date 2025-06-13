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
                new[] { 
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Azure.Core.Pole.Reference).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                
            // Check for compilation errors
            System.Collections.Generic.IEnumerable<Diagnostic> diagnostics = compilation.GetDiagnostics();
            System.Collections.Generic.IEnumerable<Diagnostic> errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
            Assert.IsFalse(errors.Any(), $"Generated C# code should compile without errors. Errors: {string.Join(", ", errors.Select(e => e.GetMessage()))}");
            
            // Verify the generated struct structure using semantic model
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            INamedTypeSymbol? dogStruct = compilation.GetTypeByMetadataName("Dog");
            Assert.IsNotNull(dogStruct, "Dog struct should be found in the compilation");
            Assert.AreEqual(TypeKind.Struct, dogStruct!.TypeKind, "Dog should be a struct, not a class");
            
            // Verify struct members - should have properties and some fields/constants
            ISymbol[] properties = dogStruct!.GetMembers().Where(m => m.Kind == SymbolKind.Property).ToArray();
            Assert.AreEqual(3, properties.Length, "Dog struct should have exactly 3 properties");
            
            // Verify specific properties
            IPropertySymbol? nameProperty = properties.OfType<IPropertySymbol>().FirstOrDefault(p => p.Name == "Name");
            Assert.IsNotNull(nameProperty, "Name property should exist");
            Assert.AreEqual("String", nameProperty!.Type.Name, "Name property should be of type String");
            
            IPropertySymbol? ageProperty = properties.OfType<IPropertySymbol>().FirstOrDefault(p => p.Name == "Age");
            Assert.IsNotNull(ageProperty, "Age property should exist");
            Assert.AreEqual("Byte", ageProperty!.Type.Name, "Age property should be of type Byte");
            
            IPropertySymbol? isMaleProperty = properties.OfType<IPropertySymbol>().FirstOrDefault(p => p.Name == "IsMale");
            Assert.IsNotNull(isMaleProperty, "IsMale property should exist");
            Assert.AreEqual("Boolean", isMaleProperty!.Type.Name, "IsMale property should be of type Boolean");
            
            // Verify that there's a _reference field
            IFieldSymbol? referenceField = dogStruct!.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(f => f.Name == "_reference");
            Assert.IsNotNull(referenceField, "_reference field should exist");
            Assert.AreEqual("Reference", referenceField!.Type.Name, "_reference field should be of type Reference");
            
            // Verify that there's a Deserialize method
            IMethodSymbol? deserializeMethod = dogStruct!.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == "Deserialize");
            Assert.IsNotNull(deserializeMethod, "Deserialize method should exist");
            Assert.IsTrue(deserializeMethod!.IsStatic, "Deserialize method should be static");
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
                    MetadataReference.CreateFromFile(typeof(System.Runtime.GCSettings).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Azure.Core.Pole.Reference).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
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
                Assert.IsTrue(dogType!.IsValueType, "Dog should be a value type (struct)");
                
                // Verify that it has the expected properties
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
                
                // Verify that there's a _reference field
                System.Reflection.FieldInfo? referenceField = dogType!.GetField("_reference", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(referenceField, "_reference field should exist");
                
                // Verify that there's a Deserialize method
                System.Reflection.MethodInfo? deserializeMethod = dogType!.GetMethod("Deserialize", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                Assert.IsNotNull(deserializeMethod, "Deserialize method should exist");
                Assert.IsTrue(deserializeMethod!.IsStatic, "Deserialize method should be static");
                
                // Note: We don't test runtime behavior here because POLE structs require 
                // a proper Reference instance which needs a PoleHeap setup
            }
        }
    }
}