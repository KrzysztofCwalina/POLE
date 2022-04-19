using Azure.Cooking.Receipes;
using Azure.Core.Pole.TestModels.Server;
using NUnit.Framework;
using System;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void GetReceipe()
        {
            var client = new CookingReceipesClient();
            CookingReceipe receipe = client.GetReceipe(1);

            Assert.AreEqual("Polish Pierogi", receipe.Title);
        }
    }
}