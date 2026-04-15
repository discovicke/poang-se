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
