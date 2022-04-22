// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole;
using System.Buffers.Binary;
using System.IO.Pipelines;

namespace CookingReceipesServer
{
    internal readonly struct CookingReceipe
    {
        private struct Schema
        {
            public const ulong SchemaId = 0xFFFFFFFFFFFFFE00;
            public const int TitleOffset = 0;
            public const int IngredientsOffset = 4;
            public const int DirectionsOffset = 8;
            public const int IdOffset = 12;
            public const int Size = 16;
        }

        private readonly PoleReference _reference;

        public CookingReceipe(PoleHeap heap)
            => _reference = heap.AllocateObject(Schema.Size, Schema.SchemaId); 

        public ReadOnlySpan<byte> Title
        {
            get => _reference.ReadByteBuffer(Schema.TitleOffset);
            set => _reference.WriteByteBuffer(Schema.TitleOffset, value, PoleType.Utf8BufferId);
        }
        public ReadOnlySpan<byte> Ingredients
        {
            get => _reference.ReadByteBuffer(Schema.IngredientsOffset);
            set => _reference.WriteByteBuffer(Schema.IngredientsOffset, value, PoleType.Utf8BufferId);
        }
        public ReadOnlySpan<byte> Directions
        {
            get => _reference.ReadByteBuffer(Schema.DirectionsOffset);
            set => _reference.WriteByteBuffer(Schema.DirectionsOffset, value, PoleType.Utf8BufferId);
        }
        public int Id
        {
            get => _reference.ReadInt32(Schema.IdOffset);
            set => _reference.WriteInt32(Schema.IdOffset, value);
        }
    }

    internal struct CookingReceipeSubmission
    {
        internal struct Schema
        {
            public const ulong SchemaId = 0xFFFFFFFFFFFFFD00;
            public const int TitleOffset = 0;
            public const int IngredientsOffset = 4;
            public const int DirectionsOffset = 8;
            public const int Size = 12;
        }

        private readonly ReadOnlyPoleReference _reference;

        internal static async Task<CookingReceipeSubmission> ReadAsync(PipeReader reader)
        {
            // TODO: this needs to be moved to a library
            ReadResult result = await reader.ReadAtLeastAsync(4);
            var len = BinaryPrimitives.ReadInt32LittleEndian(result.Buffer.FirstSpan);
            reader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
            result = await reader.ReadAtLeastAsync(len);
            if (!result.Buffer.IsSingleSegment) throw new NotImplementedException();
            var memory = result.Buffer.First;
            var data = BinaryData.FromBytes(memory);
            return new CookingReceipeSubmission(data);
        }
        private CookingReceipeSubmission(BinaryData poleData)
            =>_reference = new ReadOnlyPoleReference(poleData, Schema.SchemaId);  

        public Utf8 Title => _reference.ReadUtf8(Schema.TitleOffset);
        public Utf8 Ingredients => _reference.ReadUtf8(Schema.IngredientsOffset);
        public Utf8 Directions => _reference.ReadUtf8(Schema.DirectionsOffset);
    }
}
