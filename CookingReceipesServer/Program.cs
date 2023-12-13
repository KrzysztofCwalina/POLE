// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole;
using CookingReceipesServer;

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
    receipe.Title = "Polish Pierogi"u8;
    receipe.Directions = "Mix ingredients, make pierogi, and cook in a pot of hot water."u8;
    receipe.Ingredients = "Flour, water, salt, potatoes, white cheese, onion."u8;

    context.Response.ContentLength = heap.TotalWritten;
    heap.Complete();
});

app.MapPost("/receipes/", async (HttpContext context) =>
{
    var request = context.Request;
    if (request.ContentType == "application/pole")
    {
        CookingReceipeSubmission receipe = await CookingReceipeSubmission.ReadAsync(request.BodyReader);

        await request.BodyReader.CompleteAsync(); // TODO: receipe cannot be used past this point. Should there be a "detach" method?
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
