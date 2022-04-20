using Azure.Cooking.Receipes;
using Azure.Core.Pole.TestModels.Definitions;
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
            var generator = new PoleGenerator();
            generator.GenerateClientLibrary(typeof(CookingReceipesClient), "..\\..\\..\\..\\CookingReceipesClient");
        }
    }
}