using System;
using System.IO;
using System.Reflection;

namespace Azure.Core.Pole.Tooling
{
    internal class SourceWriter : IDisposable
    {
        TextWriter _writer;

        public SourceWriter(string path)
        {
            Stream stream = File.OpenWrite(path);
            _writer = new StreamWriter(stream);
        }
        public SourceWriter(TextWriter writer, bool close = false)
        {
            _writer = writer;
        }

        public int Indent;

        public void WriteLine(string line)
        {
            for (int indent = 0; indent < Indent; indent++) {
                _writer.Write("    ");
            }
            _writer.WriteLine(line);
        }

        public void Flush() => _writer.Flush();

        public void WriteLine() => _writer.WriteLine();

        public void Dispose()
        {
            _writer.Flush();
            _writer.Close();
        }
    }
}
