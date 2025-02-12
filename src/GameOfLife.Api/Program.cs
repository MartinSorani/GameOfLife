using GameOfLife.Api.Services;
using GameOfLife.Api.Repositories;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using GameOfLife.Api.Examples;

var builder = WebApplication.CreateBuilder(args);

// Read configuration from appsettings.json
var configuration = builder.Configuration;

// Configure Serilog to read from configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Register Serilog.ILogger so it can be injected where needed.
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);

// Configure services
builder.Services.AddControllers();

// Register services and repositories.
builder.Services.AddSingleton<IGameOfLifeService, GameOfLifeService>();
builder.Services.AddSingleton<IBoardRepository, FileBoardRepository>();

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
