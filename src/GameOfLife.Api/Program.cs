using GameOfLife.Api.Services;
using GameOfLife.Api.Repositories;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using GameOfLife.Api.Examples;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.File("./logs/GameOfLife.Logs.txt")
    .CreateLogger();

builder.Host.UseSerilog();

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
