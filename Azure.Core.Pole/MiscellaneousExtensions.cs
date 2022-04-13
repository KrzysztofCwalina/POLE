using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Core.Pole
{
    internal static class MiscellaneousExtensions
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

        // This is portable (NS2.0) implementation
        public static bool TryEncodeToUtf8(this string str, Span<byte> buffer, out int written)
        {
            ReadOnlySpan<char> chars = str.AsSpan();
            written = 0;
            foreach (char c in chars)
            {
                if (c < 128) buffer[written] = (byte)c;
                else
                {
                    var utf8 = Encoding.UTF8.GetBytes(str);
                    if (utf8.Length <= buffer.Length)
                    {
                        utf8.CopyTo(buffer);
                        written = utf8.Length;
                        return true;
                    }
                    else
                    {
                        written = 0;
                        return false;
                    }
                }
                written++;
            }
            return true;
        }

        public static bool IsAscii(this ReadOnlySpan<byte> buffer)
        {
            foreach (byte b in buffer) if (b > 127) return false;
            return true;
        }
        // This is portable (NS2.0) implementation
        public static string ToStringAsciiNoAlloc(this ReadOnlySpan<byte> buffer)
        {
            if (!buffer.IsAscii()) return Encoding.UTF8.GetString(buffer.ToArray());

            var result = new string(' ', buffer.Length);
            unsafe
            {
                fixed(char* chars = result)
                {
                    for(int i=0; i<buffer.Length; i++)
                    {
                        chars[i] = (char)buffer[i];
                    }
                }
            }
            return result;
        }
    }
}
