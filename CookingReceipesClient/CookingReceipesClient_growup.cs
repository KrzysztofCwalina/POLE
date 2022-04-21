// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Core.Pole;
using System;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Cooking.Receipes
{
    public partial class CookingReceipesClient
    {
        public Response<CookingReceipe> GetReceipe(ulong id)
        {
            var request = _pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.Uri.Reset(new Uri("https://localhost:7043/receipes/1"));

            Response response = _pipeline.SendRequest(request, CancellationToken.None);

            if (response.IsError) throw new RequestFailedException(response);

            CookingReceipe receipe = new CookingReceipe(response.Content);
            return Response.FromValue(receipe, response);
        }

        /// <returns>Receipe ID</returns>
        public Response<int> AddReceipe(CookingReceipeSubmission receipe)
        {
            var request = _pipeline.CreateRequest();
            request.Method = RequestMethod.Post;
            request.Uri.Reset(new Uri("https://localhost:7043/receipes"));
            request.Content = new PoleHeapContent(receipe);

            Response response = _pipeline.SendRequest(request, CancellationToken.None);

            if (response.IsError) throw new RequestFailedException(response);

            var reference = new ReadOnlyPoleReference(response.Content, PoleType.Int32Id);
            int id = reference.ReadInt32(8);

            return Response.FromValue(id, response);
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

        internal CookingReceipe(BinaryData poleData)
            => _reference = new ReadOnlyPoleReference(poleData, Schema.SchemaId);

        public string Title => _reference.ReadString(Schema.TitleOffset);

        public string Ingredients => _reference.ReadString(Schema.IngredientsOffset);

        public string Directions => _reference.ReadString(Schema.DirectionsOffset);
    }

    public class CookingReceipeSubmission
    {
        internal struct Schema
        {
            public const ulong SchemaId = 0xFFFFFFFFFFFFFE00;
            public const int TitleOffset = 8;
            public const int IngredientsOffset = 12;
            public const int DirectionsOffset = 16;
            public const int Size = 20;
        }

        public CookingReceipeSubmission()
        {}

        public string Title { get; set; }
        public string Ingredients { get; set; }
        public string Directions { get; set; }

        public static implicit operator ArrayPoolHeap(CookingReceipeSubmission receipe)
        {
            // TODO: this should write to pooled buffers and then to stream, or better to a pipe (but HttpClient)
            var heap = new ArrayPoolHeap();
            var reference = heap.AllocateObject(Schema.Size, Schema.SchemaId);
            reference.WriteString(Schema.TitleOffset, receipe.Title);
            reference.WriteString(Schema.IngredientsOffset, receipe.Ingredients);
            reference.WriteString(Schema.DirectionsOffset, receipe.Directions);
            return heap;
        }
    }
}
