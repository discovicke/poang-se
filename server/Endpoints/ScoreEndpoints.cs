using server.Service;

namespace server.Endpoints;

public static class ScoreEndpointsMapper
{
    public static WebApplication ScoreEndpoints(this WebApplication app)
    {
        app.MapGet("/scores", async (ScoreServices scoreServices) =>
        {
            var scores = await scoreServices.GetAllScores();
            return Results.Ok(scores);
        });

        app.MapPost("/scores", async (Score score, ScoreServices scoreServices) =>
        {
            var newScore = await scoreServices.AddScore(score);
            return Results.Created($"/scores/{newScore.Id}", newScore);
        });

        return app;
    }
}
