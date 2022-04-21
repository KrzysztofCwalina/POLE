// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Cooking.Receipes;
using Azure.Core.Pole.Tooling;
using NUnit.Framework;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void GenerateReceipesClient()
        {
            var fileHeader = File.ReadAllText("..\\..\\..\\FileHeader.cs");
            var generator = new PoleGenerator(fileHeader);
            generator.GenerateClientLibrary(typeof(CookingReceipesClient), "..\\..\\..\\..\\CookingReceipesClient");
        }
    }
}