using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using server.Endpoints;
using server.Service;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var connString = new Connection().ToString();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connString));


builder.Services.AddScoped<PlayerServices>();
builder.Services.AddScoped<ScoreServices>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


app.PlayerEndpoints();
app.ScoreEndpoints();

app.UseCors();
app.UseStaticFiles();

app.MapGet("/", () => "Hello World!");

app.Run();
