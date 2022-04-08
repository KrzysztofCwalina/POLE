using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Text;

namespace Azure.Core.Pole
{
    public readonly struct Utf8 : IObject
    {
        readonly PoleReference _reference;

        PoleReference IObject.Reference => _reference;

        public Utf8(PoleReference reference) => _reference = reference;
        public static Utf8 Allocate(PoleHeap heap, string str)
        {
            var strLength = Encoding.UTF8.GetByteCount(str);
            var bufferLength = strLength + sizeof(int);
            var reference = heap.Allocate(bufferLength);
            Span<byte> buffer = heap.GetBytes(reference.Address, bufferLength);
            byte[] utf8 = Encoding.UTF8.GetBytes(str); // TODO: this should encode directly into the buffer
            BinaryPrimitives.WriteInt32LittleEndian(buffer, strLength);
            utf8.AsSpan().CopyTo(buffer.Slice(sizeof(int)));
            return new Utf8(reference);
        }

        public override string ToString()
        {
            var bytes = _reference.ReadBytes(0);
            return Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}
