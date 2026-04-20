using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using server.Endpoints;
using server.Extensions;
using server.Hubs;

var builder = WebApplication.CreateBuilder(args);

var envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));
Env.Load(envPath);

var connString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "";

builder.Services
    .AddAppDatabase(connString)
    .AddAppServices()
    .AddAppCors()
    .AddSignalR();

var app = builder.Build();

app.MapHub<GameHub>("/gamehub");

await InitializeDatabase(app);

app.GameEndpoints();
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
