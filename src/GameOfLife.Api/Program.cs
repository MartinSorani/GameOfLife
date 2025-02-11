using GameOfLife.Api.Services;
using GameOfLifeAPI.Models;
using GameOfLifeAPI.Repositories;
using GameOfLifeAPI.Services;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.File("./logs/GameOfLife.Logs.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.AddSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Register services
builder.Services.AddSingleton<IBoardRepository, InMemoryBoardRepository>();
builder.Services.AddScoped<IGameOfLifeService, GameOfLifeService>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<BoardStateModelExample>();

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GameOfLife API", Version = "v1" });
    c.ExampleFilters();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GameOfLife API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.MapControllers();

// Handle application shutdown to save data
var boardRepository = app.Services.GetRequiredService<IBoardRepository>();
var lifetime = app.Lifetime;

lifetime.ApplicationStopping.Register(async () =>
{
    await boardRepository.SaveAsync();
});

app.Run();
