// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole;
using CookingReceipesServer;
using System.Buffers.Binary;
using System.IO.Pipelines;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/receipes/{id}", (int id, HttpContext context) =>
{
    context.Response.StatusCode = 200;
    context.Response.ContentType = "application/pole";
    var writer = context.Response.BodyWriter;

    var heap = new PipelineHeap(writer);

    CookingReceipe receipe = new CookingReceipe(heap);
    receipe.Id = 303;
    receipe.Title = "Polish Pierogi";
    receipe.Directions = "Mix ingredients, make pierogi, and cook in a pot of hot water.";
    receipe.Ingredients = "Flour, water, salt, potatoes, white cheese, onion.";

    context.Response.ContentLength = heap.TotalWritten;
    heap.Complete();
});

app.MapPost("/receipes/", async (HttpContext context) =>
{
    var request = context.Request;
    if (request.ContentType == "application/pole")
    {
        PipeReader reader = request.BodyReader;
        ReadResult result = await reader.ReadAtLeastAsync(4);
        var len = BinaryPrimitives.ReadInt32LittleEndian(result.Buffer.FirstSpan);
        reader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
        result = await reader.ReadAtLeastAsync(len);
        if (!result.Buffer.IsSingleSegment) throw new NotImplementedException();
        var memory = result.Buffer.First;
        var data = BinaryData.FromBytes(memory);
        var receipe = new CookingReceipeSubmission(data);
        await reader.CompleteAsync();
    }

    context.Response.StatusCode = 200;
    context.Response.ContentType = "application/pole";
    var writer = context.Response.BodyWriter;

    int receipeId = 303;
    var heap = new PipelineHeap(writer);
    var reference = heap.AllocateObject(sizeof(int), PoleType.Int32Id);
    reference.WriteInt32(offset: 0, receipeId);
    context.Response.ContentLength = heap.TotalWritten;
    heap.Complete();
});

app.Run();
