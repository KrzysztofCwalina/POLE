// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole.Tooling;
using NUnit.Framework;
using System;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class TypeSpecGeneratorTests
    {
        [Test]
        public void Basics()
        {
            var source = Path.Combine(".", "tsp", "simple.tsp");
            var destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            TypeSpecGenerator.Generate(source, destination);
            
            // Verify the generated file exists and has correct content
            var generatedFile = Path.Combine(destination, "Person.cs");
            Assert.IsTrue(File.Exists(generatedFile), "Generated C# file should exist");
            
            var content = File.ReadAllText(generatedFile);
            Assert.IsTrue(content.Contains("public class Person"), "Should contain class definition");
            Assert.IsTrue(content.Contains("public string FirstName { get; set; }"), "Should contain FirstName property");
            Assert.IsTrue(content.Contains("public string LastName { get; set; }"), "Should contain LastName property");
            Assert.IsTrue(content.Contains("public int Age { get; set; }"), "Should contain Age property");
            
            // Cleanup
            Directory.Delete(destination, true);
        }

        [Test]
        public void SimpleRequirementFormat()
        {
            // Test with exact format from requirements
            var source = Path.Combine(".", "tsp", "simple-test.tsp");
            var destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            TypeSpecGenerator.Generate(source, destination);
            
            // Verify the generated file exists and has correct content
            var generatedFile = Path.Combine(destination, "Person.cs");
            Assert.IsTrue(File.Exists(generatedFile), "Generated C# file should exist");
            
            var content = File.ReadAllText(generatedFile);
            Assert.IsTrue(content.Contains("public class Person"), "Should contain class definition");
            Assert.IsTrue(content.Contains("public string Name { get; set; }"), "Should contain Name property");
            Assert.IsTrue(content.Contains("public int Age { get; set; }"), "Should contain Age property"); 
            Assert.IsTrue(content.Contains("public bool IsActive { get; set; }"), "Should contain IsActive property");
            
            // Cleanup
            Directory.Delete(destination, true);
        }

        [Test]
        public void ErrorHandling_FileNotFound()
        {
            var source = "nonexistent.tsp";
            var destination = Path.GetTempPath();
            
            var ex = Assert.Throws<InvalidOperationException>(() => 
                TypeSpecGenerator.Generate(source, destination));
            
            Assert.IsTrue(ex.Message.Contains("Error generating C# code from TypeSpec"));
            Assert.IsInstanceOf<FileNotFoundException>(ex.InnerException);
        }

        [Test]
        public void DogModelExample()
        {
            // Test with Dog model example from user feedback
            var source = Path.Combine(".", "tsp", "dog.tsp");
            var destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            TypeSpecGenerator.Generate(source, destination);
            
            // Verify the generated file exists and has correct content
            var generatedFile = Path.Combine(destination, "Dog.cs");
            Assert.IsTrue(File.Exists(generatedFile), "Generated C# file should exist");
            
            var content = File.ReadAllText(generatedFile);
            Assert.IsTrue(content.Contains("public class Dog"), "Should contain class definition");
            Assert.IsTrue(content.Contains("public string Name { get; set; }"), "Should contain Name property");
            Assert.IsTrue(content.Contains("public byte Age { get; set; }"), "Should contain Age property with byte type");
            Assert.IsTrue(content.Contains("public bool IsMale { get; set; }"), "Should contain IsMale property");
            
            // Cleanup
            Directory.Delete(destination, true);
        }

        [Test]
        public void ErrorHandling_InvalidContent()
        {
            // Create a temporary file with invalid content
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "invalid content");
            
            var destination = Path.GetTempPath();
            
            var ex = Assert.Throws<InvalidOperationException>(() => 
                TypeSpecGenerator.Generate(tempFile, destination));
            
            Assert.IsTrue(ex.Message.Contains("Error generating C# code from TypeSpec"));
            
            // Cleanup
            File.Delete(tempFile);
        }
    }
}