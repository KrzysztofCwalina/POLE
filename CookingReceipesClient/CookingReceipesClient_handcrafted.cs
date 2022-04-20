using Azure;
using Azure.Core;
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
            CookingReceipe receipe = new CookingReceipe(response.Content);
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

        private readonly ReadOnlyPoleReference _reference;

        internal CookingReceipe(BinaryData poleData) {
            var heap = new SingleSegmentPoleMemory(poleData);
            var reference = heap.GetRoot();
            var typeId = reference.ReadTypeId();
            if (typeId != Schema.SchemaId) throw new InvalidCastException("invalid cast");
            _reference = reference;
        }

        public string Title => _reference.ReadString(Schema.TitleOffset);

        public string Ingredients => _reference.ReadString(Schema.IngredientsOffset);

        public string Directions => _reference.ReadString(Schema.DirectionsOffset);
    }
}
