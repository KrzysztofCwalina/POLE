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
            using PoleHeap heap = new PoleHeap();
            HelloModel model = HelloModel.Allocate(heap);

            model.Title = "Hello, ";

            Assert.Throws<InvalidOperationException>(()=> {
                model.Title = "World!";
            });
        }
    }
}