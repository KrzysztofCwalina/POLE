// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Cooking.Receipes;
using NUnit.Framework;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        //[Ignore("live test")]
        public void GetReceipe()
        {
            var client = new CookingReceipesClient();
            CookingReceipe receipe = client.GetReceipe(303);

            Assert.AreEqual("Polish Pierogi", receipe.Title);
            Assert.AreEqual(303, receipe.Id);
        }

        [Test]
        //[Ignore("live test")]
        public void AddReceipe()
        {
            var client = new CookingReceipesClient();

            var receipe = new CookingReceipeSubmission();
            receipe.Title = "Polish Pierogi";
            receipe.Directions = "Mix ingredients, make pierogi, and cook in a pot of hot water.";
            receipe.Ingredients = "Flour, water, salt, potatoes, white cheese, onion.";

            int receipeId = client.AddReceipe(receipe);

            Assert.AreEqual(303, receipeId);
        }
    }
}