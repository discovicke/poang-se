using server.Dtos;
using server.Models;
using server.Service;
using server.Helpers;
namespace server.Endpoints;

/// <summary>
/// Endpoints för spelare och lag inom ett spel: lägg till, byt namn, ta bort, tilldela lag.
/// </summary>
public static class GamePlayerEndpoints
{
    public static WebApplication MapGamePlayerEndpoints(this WebApplication app)
    {

        app.MapPost("/api/games/{id:guid}/teams", async (Guid id, AddTeamDto dto, GamePlayerService svc, CancellationToken ct) =>
            {

                var team = new Team { Id = Guid.NewGuid(), GameId = id, Name = dto.Name };
                await svc.AddTeam(team, ct);
                return Results.Created($"/games/{id}/teams/{team.Id}", new { team.Id, team.Name });
            })
            .WithSummary("Lägg till lag")
            .WithTags("Spelare & Lag");

        app.MapPut("/api/games/{id:guid}/teams/{teamId:guid}/rename",
                async (Guid id, Guid teamId, RenameDto dto, GamePlayerService svc, CancellationToken ct) =>
                {

                    if (string.IsNullOrWhiteSpace(dto.Name))
                        return Results.BadRequest("Namn saknas");
                    return await svc.RenameTeam(id, teamId, dto.Name)
                        ? Results.NoContent()
                        : Results.NotFound();
                })
            .WithSummary("Byt namn på lag")
            .WithTags("Spelare & Lag");

        app.MapDelete("/api/games/{id:guid}/teams/{teamId:guid}", async (Guid id, Guid teamId, GamePlayerService svc, CancellationToken ct) =>
            {

                return await svc.RemoveTeam(id, teamId, ct)
                    ? Results.NoContent()
                    : Results.NotFound();
            })
            .WithSummary("Ta bort lag")
            .WithTags("Spelare & Lag");

        app.MapPost("/api/games/{id:guid}/players", async (Guid id, AddPlayerToGameDto dto, GamePlayerService svc, CancellationToken ct) =>
            {

                var gp = new GamePlayer
                { GameId = id, PlayerId = dto.PlayerId, TeamId = dto.TeamId, JoinedAt = DateTime.UtcNow };
                var result = await svc.AddPlayerToGame(gp, ct);
                return result is null
                    ? Results.Conflict("Player already in game")
                    : Results.Created($"/games/{id}", new { gp.GameId, gp.PlayerId, gp.TeamId });
            })
            .WithSummary("Lägg till befintlig spelare")
            .WithTags("Spelare & Lag");

        app.MapPost("/api/games/{id:guid}/players/new",
                async (Guid id, CreatePlayerForGameDto dto, GamePlayerService svc, CancellationToken ct) =>
                {

                    if (string.IsNullOrWhiteSpace(dto.UserName))
                        return Results.BadRequest("UserName krävs");

                    var player = new Player
                    { Id = Guid.NewGuid(), UserName = dto.UserName.Trim(), CreatedAt = DateTime.UtcNow };
                    await svc.CreatePlayer(player, ct);

                    var gp = new GamePlayer
                    { GameId = id, PlayerId = player.Id, TeamId = dto.TeamId, JoinedAt = DateTime.UtcNow };
                    await svc.AddPlayerToGame(gp, ct);

                    return Results.Created($"/games/{id}", new { player.Id, player.UserName, gp.GameId, gp.TeamId });
                })
            .WithSummary("Skapa och lägg till ny spelare")
            .WithTags("Spelare & Lag");

        app.MapPut("/api/games/{id:guid}/players/team",
                async (Guid id, AssignPlayerToTeamDto dto, GamePlayerService svc, CancellationToken ct) =>
                {

                    var result = await svc.AssignPlayerToTeam(id, dto.PlayerId, dto.TeamId, ct);
                    return result is null
                        ? Results.NotFound()
                        : Results.Ok(new { result.PlayerId, result.TeamId });
                })
            .WithSummary("Tilldela spelare till lag")
            .WithTags("Spelare & Lag");

        app.MapPut("/api/games/{id:guid}/players/{playerId:guid}/rename",
                async (Guid id, Guid playerId, RenameDto dto, GamePlayerService svc) =>
                {
                    if (string.IsNullOrWhiteSpace(dto.Name))
                        return Results.BadRequest("Namn saknas");
                    return await svc.RenamePlayer(id, playerId, dto.Name)
                        ? Results.NoContent()
                        : Results.NotFound();
                })
            .WithSummary("Byt namn på spelare")
            .WithTags("Spelare & Lag");

        app.MapDelete("/api/games/{id:guid}/players/{playerId:guid}",
                async (Guid id, Guid playerId, GamePlayerService svc, CancellationToken ct) =>
                {
                    return await svc.RemovePlayerFromGame(id, playerId, ct)
                        ? Results.NoContent()
                        : Results.NotFound();
                })
            .WithSummary("Ta bort spelare från match")
            .WithTags("Spelare & Lag");

        return app;
    }
}
