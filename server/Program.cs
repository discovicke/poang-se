using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var connString = new Connection().ToString(); 

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connString));



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

app.UseCors();
app.UseStaticFiles();

app.MapGet("/", () => "Hello World!");

app.Run();
