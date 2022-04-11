using Azure.Core.Pole.TestModels.Definitions;
using Azure.Core.Pole.Tooling;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;

namespace Azure.Core.Pole.Tests
{
    public partial class UnitTests
    {
        [Test]
        public void Generator()
        {
            var generator = new PoleGenerator();
            var stream = File.OpenWrite("model.cs");
            generator.Generate<HelloModel>(stream, serverSide: true);
            stream.Close();
        }
    }
}