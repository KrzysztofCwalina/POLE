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
        public void GenerateHelloModel()
        {
            var generator = new PoleGenerator();
            var stream = File.OpenWrite("HelloModel_server.cs");
            generator.Generate(typeof(HelloModel), stream, serverSide: true);
            stream.Close();

            stream = File.OpenWrite("HelloModel.cs");
            generator.Generate(typeof(HelloModel), stream, serverSide: false);
        }

        //[Test]
        //public void GenerateTestModels()
        //{
            //var generator = new PoleGenerator();
            //var stream = File.OpenWrite("TestModel_server.cs");
            //generator.Generate(typeof(TestModel).Assembly, stream, serverSide: true);
            //stream.Close();

            //stream = File.OpenWrite("TestModel.cs");
            //generator.Generate(typeof(TestModel).Assembly, stream, serverSide: false);
        //}
    }
}