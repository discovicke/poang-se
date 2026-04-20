using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using server.Endpoints;
using server.Extensions;
using server.Hubs;

var builder = WebApplication.CreateBuilder(args);

var envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));
Env.Load(envPath);

var connString = new Connection().ToString();

builder.Services
    .AddAppDatabase(connString)
    .AddAppServices()
    .AddAppCors()
    .AddOpenApi()
    .AddSignalR();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapHub<GameHub>("/gamehub");

await InitializeDatabase(app);

app.MapGameLifecycleEndpoints();
app.MapGameScoringEndpoints();
app.MapGamePlayerEndpoints();
app.PlayerEndpoints();
app.ScoreEndpoints();

app.UseCors();
app.UseStaticFiles();

app.MapGet("/", () => "Hello World!");

app.Run();

static async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Applying pending migrations...");
    await db.Database.MigrateAsync();
    logger.LogInformation("Database ready.");
}
