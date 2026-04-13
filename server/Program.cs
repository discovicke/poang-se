var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});


app.UseStaticFiles();


app.MapGet("/", () => "Hello World!");

app.Run();
