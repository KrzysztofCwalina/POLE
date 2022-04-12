using Azure.Core.Pole;
using System;
using System.IO;

namespace Azure.Core.Pole.TestModels
{
    public class InputModel 
    {
        public InputModel()
        {
        }

        public string Title { get; set; }

        public int RepeatCount { get; set; }

        public void WriteTo(Stream stream)
        {
            var heap = new PoleHeap();
            throw new NotImplementedException();
        }
    }

    public struct Child
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
