using GameOfLife.Api.Services;
using GameOfLife.Api.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.File("GameOfLife.Api.Log.txt")
    .CreateLogger();

builder.Host.UseSerilog();

// Configure services (dependency injection)
builder.Services.AddControllers();

// Register your custom services and repositories.
// For example, if you're using an in-memory repository:
builder.Services.AddSingleton<IInMemoryBoardRepository, InMemoryBoardRepository>();
builder.Services.AddSingleton<IGameOfLifeService, GameOfLifeService>();

// Optional: Add Swagger for API documentation.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
