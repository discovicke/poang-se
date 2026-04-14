using server.Service;

namespace server.Endpoints;

public static class ScoreEndpointsMapper
{
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
