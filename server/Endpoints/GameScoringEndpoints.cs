using server.Dtos;
using server.Mappers;
using server.Models;
using server.Service;
using server.Helpers;
namespace server.Endpoints;

/// <summary>
/// Endpoints för poäng: registrera och uppdatera poäng inom ett spel.
/// </summary>
public static class GameScoringEndpoints
{
    public static WebApplication MapGameScoringEndpoints(this WebApplication app)
    {

        app.MapGet("/api/games/{id:guid}/score-chart", async (Guid id, GameLifecycleService svc, GameScoringService scoreSvc, CancellationToken ct) =>
           {
               var game = await svc.GetGameById(id, ct);
               if (game is null)
                   return Results.NotFound();
               var chartData = scoreSvc.BuildScoreChartData(game);
               return Results.Ok(chartData);
           })
           .WithSummary("Hämta data för poängdiagram")
           .WithTags("Poäng");

        app.MapPost("/api/games/{id:guid}/scores", async (Guid id, AddScoreToGameDto dto, GameScoringService svc, CancellationToken ct) =>
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
                var result = await svc.AddScore(score, ct);
                return Results.Created($"/games/{id}/scores/{result.Id}", result.ToScoreResponse());
            })
            .WithSummary("Registrera poäng")
            .WithTags("Poäng");

        app.MapPut("/api/games/{id:guid}/scores", async (Guid id, UpdateScoreValueDto dto, GameScoringService svc, CancellationToken ct) =>
            {
                var result = await svc.UpdateScoreValue(id, dto.PlayerId, dto.Round, dto.Value, ct);
                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result.ToScoreResponse());
            })
            .WithSummary("Uppdatera poäng för spelare i en runda")
            .WithTags("Poäng");

        return app;
    }
}
