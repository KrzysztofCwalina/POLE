using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Core.Pole
{
    internal static class StreamExtensions 
    {
        public static void Write(this Stream stream, ReadOnlyMemory<byte> memory, CancellationToken cancellation = default)
        {
            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> segment))
            {
                stream.Write(segment.Array, segment.Offset, segment.Count);
                if (cancellation.IsCancellationRequested) throw new OperationCanceledException();
            }
            else
            {
                var array = memory.ToArray();
                stream.Write(array, 0, array.Length);
                if (cancellation.IsCancellationRequested) throw new OperationCanceledException();
            }
        }
        public static async Task WriteAsync(this Stream stream, ReadOnlyMemory<byte> memory, CancellationToken cancellation)
        {
            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> segment))
            {
                await stream.WriteAsync(segment.Array, segment.Offset, segment.Count, cancellation);
            }
            else
            {
                var array = memory.ToArray();
                await stream.WriteAsync(array, 0, array.Length, cancellation);
            }
        }
    }
}
