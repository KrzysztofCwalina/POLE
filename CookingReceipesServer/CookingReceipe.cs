﻿using Azure.Core.Pole;

namespace CookingReceipesServer
{
    public readonly struct CookingReceipe
    {
        private struct Schema
        {
            public const ulong SchemaId = 0xFFFFFFFFFFFFFFFF;
            public const int TitleOffset = 8;
            public const int IngredientsOffset = 12;
            public const int DirectionsOffset = 16;
            public const int Size = 20;
        }

        private readonly PoleReference _reference;
        private CookingReceipe(PoleReference reference) => _reference = reference;

        public static CookingReceipe Allocate(PoleHeap heap)
        {
            PoleReference reference = heap.Allocate(Schema.Size);
            reference.WriteTypeId(Schema.SchemaId);
            return new CookingReceipe(reference);
        }

        public ReadOnlySpan<byte> Title
        {
            get => _reference.ReadByteBuffer(Schema.TitleOffset);
            set
            {
                if (_reference.ReadInt32(Schema.TitleOffset) != 0) throw new InvalidOperationException("assign once property");
                _reference.WriteByteBuffer(Schema.TitleOffset, value);
            } 
        }
        public ReadOnlySpan<byte> Ingredients
        {
            get => _reference.ReadByteBuffer(Schema.IngredientsOffset);
            set
            {
                if (_reference.ReadInt32(Schema.IngredientsOffset) != 0) throw new InvalidOperationException("assign once property");
                _reference.WriteByteBuffer(Schema.IngredientsOffset, value);
            }
        }
        public ReadOnlySpan<byte> Directions
        {
            get => _reference.ReadByteBuffer(Schema.DirectionsOffset);
            set
            {
                if (_reference.ReadInt32(Schema.DirectionsOffset) != 0) throw new InvalidOperationException("assign once property");
                _reference.WriteByteBuffer(Schema.DirectionsOffset, value);
            }
        }
    }
}