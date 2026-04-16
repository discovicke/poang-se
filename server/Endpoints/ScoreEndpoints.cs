using server.Service;

namespace server.Endpoints;

/// <summary>
/// Registrerar generella poäng-endpoints under prefixet <c>/api/scores</c>.
/// Spelspecifika poängoperationer hanteras av <see cref="GameEndpointMapper"/>.
/// </summary>
public static class ScoreEndpointsMapper
{
    /// <summary>
    /// Kopplar endpoints till <paramref name="app"/>:
    /// <list type="bullet">
    ///   <item><c>GET  /api/scores</c> – hämta alla poäng</item>
    ///   <item><c>POST /api/scores</c> – registrera en ny poäng direkt (utan spelkontext)</item>
    /// </list>
    /// </summary>
    public static WebApplication ScoreEndpoints(this WebApplication app)
    {
        app.MapGet("/api/scores", async (ScoreServices scoreServices) =>
        {
            var scores = await scoreServices.GetAllScores();
            return Results.Ok(scores);
        });

        app.MapPost("/api/scores", async (Score score, ScoreServices scoreServices) =>
        {
            var newScore = await scoreServices.AddScore(score);
            return Results.Created($"/api/scores/{newScore.Id}", newScore);
        });

        return app;
    }
}
