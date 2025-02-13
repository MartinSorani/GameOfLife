using GameOfLife.Api.Examples;
using GameOfLife.Api.Repositories;
using GameOfLife.Api.Services;
using GameOfLife.Api.Utils;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Read configuration from appsettings.json
var configuration = builder.Configuration;

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(new FileLoggerProvider(configuration));

// Configure services
builder.Services.AddControllers();

// Register services and repositories.
builder.Services.AddSingleton<IGameOfLifeService, GameOfLifeService>();
builder.Services.AddSingleton<IBoardRepository, FileBoardRepository>();

// Register ILogger<T> for dependency injection
builder.Services.AddLogging();

// Add Swagger for API documentation.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<BoardDtoExample>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map controller routes.
app.MapControllers();

app.Run();

public partial class Program { }
