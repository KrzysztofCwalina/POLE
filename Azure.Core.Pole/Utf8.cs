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
            BinaryPrimitives.WriteInt32LittleEndian(buffer, strLength);
            if (!str.TryEncodeToUtf8(buffer.Slice(sizeof(int)), out var written))
            {
                throw new NotImplementedException("this should never happen");
            }               
            return new Utf8(reference);
        }

        public override string ToString()
        {
            var bytes = _reference.ReadBytes(0);
            return bytes.ToStringAsciiNoAlloc();
        }
    }
}
