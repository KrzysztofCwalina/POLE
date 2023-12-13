// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole.Tooling;
using NUnit.Framework;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class TypeSpecGeneratorTests
    {
        [Test]
        public void Basics()
        {
            var source = Path.Combine(".", "tsp", "simple.tsp");
            var destination = Path.Combine(Path.GetTempPath());
            TypeSpecGenerator.Generate(source, destination);
            Directory.Delete(destination, true);
        }
    }
}