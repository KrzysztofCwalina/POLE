using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Core.Pole;
using System;
using System.Threading;

namespace Azure.Cooking.Receipes
{
    public partial class CookingReceipesClient
    {
        public CookingReceipe GetReceipe(ulong id)
        {
            var request = _pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.Uri.Reset(new Uri("https://localhost:7043/receipes/1"));
            Response response = _pipeline.SendRequest(request, CancellationToken.None);
            var heap = PoleHeap.ReadFrom(response.ContentStream);
            CookingReceipe receipe = CookingReceipe.Deserialize(heap);
            return receipe;
        }
    }

    public class CookingReceipe
    {
        private struct Schema
        {
            public const ulong SchemaId = 0xFFFFFFFFFFFFFF00;
            public const int TitleOffset = 8;
            public const int IngredientsOffset = 12;
            public const int DirectionsOffset = 16;
            public const int Size = 20;
        }

        private readonly PoleReference _reference;
        internal CookingReceipe(PoleReference reference)
        {
            _reference = reference;
        }

        internal static CookingReceipe Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            var typeId = reference.ReadTypeId();
            if (typeId != Schema.SchemaId) throw new InvalidCastException();
            return new(reference);
        }

        public string Title
        {
            get => _reference.ReadString(Schema.TitleOffset);
        }
        public string Ingredients
        {
            get => _reference.ReadString(Schema.IngredientsOffset);
        }
        public string Directions
        {
            get => _reference.ReadString(Schema.DirectionsOffset);
        }
    }
}
