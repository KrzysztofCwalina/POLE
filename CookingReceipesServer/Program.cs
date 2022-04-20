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

    var pole = new PipelineHeap(writer);
    var receipe = CookingReceipe.Allocate(pole);

    receipe.Title = "Polish Pierogi";
    receipe.Directions = "Mix ingredients, make pierogi, and cook in a pot of hot water.";
    receipe.Ingredients = "Flour, water, salt, potatoes, white cheese, onion.";

    context.Response.ContentLength = pole.TotalWritten;
    pole.Complete();
});

app.Run();
