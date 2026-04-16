using server.Dtos;
using server.Service;

namespace server.Endpoints;

/// <summary>
/// Registrerar spelare-relaterade Minimal API-endpoints under prefixet <c>/api/players</c>.
/// </summary>
public static class PlayerEndpointMapper
{
    /// <summary>
    /// Kopplar endpoints till <paramref name="app"/>:
    /// <list type="bullet">
    ///   <item><c>POST /api/players</c> – skapa en ny spelare</item>
    ///   <item><c>GET  /api/players</c> – hämta alla spelare</item>
    /// </list>
    /// </summary>
    public static WebApplication PlayerEndpoints(this WebApplication app)
    {
        app.MapPost("/api/players", async (PlayerDto playerDto, PlayerServices playerServices) =>
        {
            var player = new Player
            {
                Id = Guid.NewGuid(),
                UserName = playerDto.UserName,
                CreatedAt = DateTime.UtcNow,
                Scores = []
            };

            await playerServices.CreatePlayer(player);
            return Results.Created($"/api/players/{player.Id}", player);
        });

        app.MapGet("/api/players", async (PlayerServices playerServices) =>
        {
            var players = await playerServices.GetAllPlayers();
            return Results.Ok(players);
        });

        return app;
    }
}
