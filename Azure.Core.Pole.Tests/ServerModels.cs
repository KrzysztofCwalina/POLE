// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole.TestModels.Server;
using NUnit.Framework;
using System;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void NoGarbage()
        {
            using var heap = new ArrayPoolHeap();
            HelloModel model = new HelloModel(heap);

            model.Message = "Hello, ";

            Assert.Throws<InvalidOperationException>(()=> {
                model.Message = "World!";
            });
        }
    }
}