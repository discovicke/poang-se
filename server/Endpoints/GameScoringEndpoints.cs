using server.Dtos;
using server.Mappers;
using server.Models;
using server.Service;

namespace server.Endpoints;

/// <summary>
/// Endpoints för poäng: registrera och uppdatera poäng inom ett spel.
/// </summary>
public static class GameScoringEndpoints
{
    public static WebApplication MapGameScoringEndpoints(this WebApplication app)
    {
        app.MapPost("/api/games/{id:guid}/scores", async (Guid id, AddScoreToGameDto dto, GameScoringService svc) =>
            {
                var score = new Score
                {
                    Id = Guid.NewGuid(),
                    GameId = id,
                    PlayerId = dto.PlayerId,
                    TeamId = dto.TeamId,
                    Round = dto.Round,
                    Value = dto.Value,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await svc.AddScore(score);
                return Results.Created($"/games/{id}/scores/{result.Id}", result.ToScoreResponse());
            })
            .WithSummary("Registrera poäng")
            .WithTags("Poäng");

        app.MapPut("/api/games/{id:guid}/scores", async (Guid id, UpdateScoreValueDto dto, GameScoringService svc) =>
            {
                var result = await svc.UpdateScoreValue(id, dto.PlayerId, dto.Round, dto.Value);
                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result.ToScoreResponse());
            })
            .WithSummary("Uppdatera poäng för spelare i en runda")
            .WithTags("Poäng");

        return app;
    }
}
