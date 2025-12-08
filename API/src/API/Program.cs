using API.Infrastructure;
using API.Shared.Config;
using API.Mediator;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var cfg = builder.Configuration.Get<AppConfig>() ?? throw new InvalidOperationException("App configuration must be provided.");

cfg.Migrate = args.Contains("--migrate");

builder.Services.AddSingleton(cfg);

builder.AddServiceDefaults();

builder.Services.AddInfrastructure(cfg);
builder.Services.AddMediator(Assembly.GetExecutingAssembly());

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (cfg.Migrate)
{
    var scope = app.Services.CreateScope();
    await scope.MigrateDb();
    Console.WriteLine("Migration complete.");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => $"Works {DateTime.UtcNow}");

app.Run();
