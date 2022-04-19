using Azure.Core.Pole;
using CookingReceipesServer;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/receipes/{id}", (HttpContext context) =>
{
    context.Response.StatusCode = 200;
    context.Response.ContentType = "application/pole";

    var heap = new PoleHeap();
    var receipe = CookingReceipe.Allocate(heap);

    receipe.Title = "Polish Pierogi";
    receipe.Directions = "Mix ingredients, make pierogi, and cook in a pot of hot water.";
    receipe.Ingredients = "Flour, water, salt, potatoes, white cheese, onion.";

    var writer = context.Response.BodyWriter;
    long contentLength = heap.WriteTo(writer);
    context.Response.ContentLength = contentLength;
    writer.Complete();
});

app.Run();
