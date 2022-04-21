// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole;

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
            set => _reference.WriteByteBuffer(Schema.TitleOffset, value);
        }
        public ReadOnlySpan<byte> Ingredients
        {
            get => _reference.ReadByteBuffer(Schema.IngredientsOffset);
            set => _reference.WriteByteBuffer(Schema.IngredientsOffset, value);
        }
        public ReadOnlySpan<byte> Directions
        {
            get => _reference.ReadByteBuffer(Schema.DirectionsOffset);
            set => _reference.WriteByteBuffer(Schema.DirectionsOffset, value);
        }
        public int Id
        {
            get => _reference.ReadInt32(Schema.IdOffset);
            set => _reference.WriteInt32(Schema.IdOffset, value);
        }
    }
}
