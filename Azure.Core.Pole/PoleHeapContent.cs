// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Core.Pole
{
    public class PoleHeapContent : RequestContent
    {
        ArrayPoolHeap _heap;

        public PoleHeapContent(ArrayPoolHeap heap)
            => _heap = heap;

        public override void Dispose()
        {
            _heap.Dispose();
            _heap = null;
        }

        public override bool TryComputeLength(out long length)
            => _heap.TryComputeLength(out length);

        public override void WriteTo(Stream stream, CancellationToken cancellation)
            => _heap.WriteTo(stream);

        public override async Task WriteToAsync(Stream stream, CancellationToken cancellation)
            => await _heap.WriteToAsync(stream, cancellation).ConfigureAwait(false);
    }
}
