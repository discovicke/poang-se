using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using server.Endpoints;
using server.Service;
using Microsoft.AspNetCore.SignalR;
using server.Hubs;

var builder = WebApplication.CreateBuilder(args);

var envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));
Env.Load(envPath);

var connString = new Connection().ToString();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connString));


builder.Services.AddScoped<PlayerServices>();
builder.Services.AddScoped<ScoreServices>();
builder.Services.AddScoped<GameServices>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<GameHub>("/gamehub");

await InitializeDatabase(app);

static async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Attempting to apply migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Migration failed. Falling back to EnsureCreated (recreating schema)...");

        try
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            logger.LogInformation("Database recreated successfully with EnsureCreated.");
        }
        catch (Exception innerEx)
        {
            logger.LogError(innerEx, "Failed to recreate database. Startup continues but DB may be broken.");
        }
    }
}


app.GameEndpoints();
app.PlayerEndpoints();
app.ScoreEndpoints();

app.UseCors();
app.UseStaticFiles();

app.MapGet("/", () => "Hello World!");

app.Run();
