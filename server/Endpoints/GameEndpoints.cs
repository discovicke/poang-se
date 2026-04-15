using Microsoft.EntityFrameworkCore.Internal;
using server.Dtos;
using server.Models;
using server.Service;

namespace server.Endpoints;

public static class GameEndpointMapper
{
    public static WebApplication GameEndpoints(this WebApplication app)
    {
        // Lista alla matcher
        app.MapGet("/api/games", async (GameServices svc) =>
        {
            var games = await svc.GetAllGames();
            return Results.Ok(games);
        });

        // Hämta match (med all relevant data)
        app.MapGet("/api/games/{id:guid}", async (Guid id, GameServices svc) =>
        {
            var game = await svc.GetGameById(id);
            if (game is null) return Results.NotFound();

            return Results.Ok(new
            {
                game.Id,
                game.Name,
                Status = game.Status.ToString(),
                game.LowerIsBetter,
                game.MaxRounds,
                game.WinnerId,
                game.CreatedAt,
                game.FinishedAt,
                Teams = game.Teams.Select(t => new { t.Id, t.Name }),
                Players = game.GamePlayers.Select(gp => new
                {
                    gp.PlayerId,
                    PlayerName = gp.Player.UserName,
                    TeamId = gp.TeamId,
                    TeamName = gp.Team?.Name,
                    gp.JoinedAt
                }),
                Scores = game.Scores
                    .OrderBy(s => s.Round).ThenBy(s => s.CreatedAt)
                    .Select(s => new
                    {
                        s.Id,
                        s.PlayerId,
                        PlayerName = s.Player.UserName,
                        s.TeamId,
                        s.Round,
                        s.Value,
                        s.CumulativeValue,
                        s.CreatedAt
                    })
            });
        });

        // Skapa ny match
        app.MapPost("/api/games", async (CreateGameDto dto, GameServices svc) =>
        {
            var game = new Game
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                LowerIsBetter = dto.LowerIsBetter,
                MaxRounds = dto.MaxRounds,
                CreatedAt = DateTime.UtcNow
            };

            await svc.CreateGame(game);
            return Results.Created($"/games/{game.Id}", new
            {
                game.Id,
                game.Name,
                Status = game.Status.ToString(),
                game.LowerIsBetter,
                game.MaxRounds,
                game.CreatedAt
            });
        });

        // Lägga till lag till en match
        app.MapPost("/api/games/{id:guid}/teams", async (Guid id, AddTeamDto dto, GameServices svc) =>
        {
            var team = new Team
            {
                Id = Guid.NewGuid(),
                GameId = id,
                Name = dto.Name
            };

            await svc.AddTeam(team);
            return Results.Created($"/games/{id}/teams/{team.Id}", new { team.Id, team.Name });
        });

        app.MapPost("/api/games/{id:guid}/players", async (Guid id, AddPlayerToGameDto dto, GameServices svc) =>
        {
            var gp = new GamePlayer
            {
                GameId = id,
                PlayerId = dto.PlayerId,
                TeamId = dto.TeamId,
                JoinedAt = DateTime.UtcNow
            };

            var result = await svc.AddPlayerToGame(gp);
            if (result is null) return Results.Conflict("Player already in game");

            return Results.Created($"/games/{id}", new { gp.GameId, gp.PlayerId, gp.TeamId });
        });

        // Skapa en ny spelare och lägg direkt till i matchen
        app.MapPost("/api/games/{id:guid}/players/new", async (Guid id, CreatePlayerForGameDto dto, GameServices svc, PlayerServices playerSvc) =>
        {
            if (string.IsNullOrWhiteSpace(dto.UserName))
                return Results.BadRequest("UserName krävs");

            var player = new Player
            {
                Id = Guid.NewGuid(),
                UserName = dto.UserName.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            await playerSvc.CreatePlayer(player);

            var gp = new GamePlayer
            {
                GameId = id,
                PlayerId = player.Id,
                TeamId = dto.TeamId,
                JoinedAt = DateTime.UtcNow
            };
            await svc.AddPlayerToGame(gp);

            return Results.Created($"/games/{id}", new { player.Id, player.UserName, gp.GameId, gp.TeamId });
        });

        // Lägga till poäng till en match
        app.MapPost("/api/games/{id:guid}/scores", async (Guid id, AddScoreToGameDto dto, GameServices svc) =>
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
            return Results.Created($"/games/{id}/scores/{result.Id}", new
            {
                result.Id,
                result.PlayerId,
                result.Round,
                result.Value,
                result.CumulativeValue
            });
        });

        // Uppdatera poäng i en specifik runda och indexera om kumulativa värden
        app.MapPut("/api/games/{id:guid}/scores", async (Guid id, UpdateScoreValueDto dto, GameServices svc) =>
        {
            var result = await svc.UpdateScoreValue(id, dto.PlayerId, dto.Round, dto.Value);
            if (result == null)
                return Results.NotFound();
            return Results.Ok(new
            {
                result.Id,
                result.PlayerId,
                result.Round,
                result.Value,
                result.CumulativeValue
            });
        });


        // Starta en match (ändrar enum status till Started)
        app.MapPut("/api/games/{id:guid}/start", async (Guid id, GameServices svc) =>
        {
            var game = await svc.StartGame(id);
            if (game is null) return Results.NotFound();
            return Results.Ok(new { game.Id, Status = game.Status.ToString() });
        });

        // Starta en match (ändrar enum status till Finished)
        app.MapPut("/api/games/{id:guid}/finish", async (Guid id, GameServices svc) =>
        {
            var game = await svc.FinishGame(id);
            if (game is null) return Results.NotFound();
            return Results.Ok(new
            {
                game.Id,
                Status = game.Status.ToString(),
                game.WinnerId,
                game.FinishedAt
            });
        });

        return app;
    }
}
