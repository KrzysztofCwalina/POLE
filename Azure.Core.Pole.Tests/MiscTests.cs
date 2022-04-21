// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole.TestModels;
using NUnit.Framework;
using System;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void SchemaVerification()
        {
            var data = BinaryData.FromString("This is not valid POLE data.");
            Assert.Throws<InvalidCastException>(() => {
                HelloModel hello = new HelloModel(data);
            });
        }
    }
}