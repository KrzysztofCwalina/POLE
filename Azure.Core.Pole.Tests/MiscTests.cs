using Azure.Core.Pole.TestModels;
using NUnit.Framework;
using System;
using System.IO;

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