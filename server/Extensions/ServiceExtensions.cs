using Microsoft.EntityFrameworkCore;
using server.Service;

namespace server.Extensions;

/// <summary>
/// Extension-metoder för <see cref="IServiceCollection"/> som samlar applikationens
/// service-registreringar i återanvändbara block.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registrerar <see cref="AppDbContext"/> med Npgsql och den angivna connection string.
    /// </summary>
    public static IServiceCollection AddAppDatabase(this IServiceCollection services, string connString)
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connString));
        return services;
    }

    /// <summary>
    /// Registrerar applikationens domän-tjänster (<see cref="PlayerServices"/>,
    /// <see cref="ScoreServices"/>, <see cref="GameServices"/>) som scoped-beroenden.
    /// </summary>
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<PlayerServices>();
        services.AddScoped<ScoreServices>();
        services.AddScoped<GameServices>();
        return services;
    }

    /// <summary>
    /// Lägger till en CORS-policy som tillåter alla headers, metoder och origins.
    /// Lämplig för lokal utveckling – skärp inför produktion.
    /// </summary>
    public static IServiceCollection AddAppCors(this IServiceCollection services)
    {
        services.AddCors(opt =>
            opt.AddDefaultPolicy(p => p
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()));
        return services;
    }
}
