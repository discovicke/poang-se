using server.Dtos;
using server.Service;

namespace server.Endpoints;

public static class PlayerEndpointMapper
{
    public static WebApplication PlayerEndpoints(this WebApplication app)
    {
        app.MapPost("/players", async (PlayerDto playerDto, PlayerServices playerServices) =>
        {
            var player = new Player
            {
                Id = Guid.NewGuid(),
                UserName = playerDto.UserName,
                CreatedAt = DateTime.UtcNow,
                Scores = []
            };

            await playerServices.CreatePlayer(player);
            return Results.Created($"/players/{player.Id}", player);
        });

        return app;
    }
}
