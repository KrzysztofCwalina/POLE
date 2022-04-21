using Azure.Core.Pole;
using CookingReceipesServer;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/receipes/{id}", (HttpContext context) =>
{
    context.Response.StatusCode = 200;
    context.Response.ContentType = "application/pole";
    var writer = context.Response.BodyWriter;

    var heap = new PipelineHeap(writer);

    CookingReceipe receipe = new CookingReceipe(heap);
    receipe.Title = "Polish Pierogi";
    receipe.Directions = "Mix ingredients, make pierogi, and cook in a pot of hot water.";
    receipe.Ingredients = "Flour, water, salt, potatoes, white cheese, onion.";

    context.Response.ContentLength = heap.TotalWritten;
    heap.Complete();
});

app.MapPost("/receipes/", (HttpContext context) =>
{
    context.Response.StatusCode = 200;
    context.Response.ContentType = "application/pole";
    var writer = context.Response.BodyWriter;

    int receipeId = 303;
    var heap = new PipelineHeap(writer);
    var reference = heap.AllocateObject(12, PoleType.Int32Id);
    reference.WriteInt32(8, receipeId);
    context.Response.ContentLength = heap.TotalWritten;
    heap.Complete();
});

app.Run();
