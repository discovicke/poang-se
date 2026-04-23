using server.Dtos;
using server.Helpers;
using server.Mappers;
using server.Models;
using server.Service;

namespace server.Endpoints;

/// <summary>
/// Endpoints för spelets livscykel: CRUD, status, inställningar, reset och rematch.
/// </summary>
public static class GameLifecycleEndpoints
{
    public static WebApplication MapGameLifecycleEndpoints(this WebApplication app)
    {


        app.MapGet("/api/games", async (GameLifecycleService svc, CancellationToken ct) =>
            {

                await svc.RemoveExpiredGames(ct);
                var games = await svc.GetAllGames(ct);
                return Results.Ok(games.Select(g => g.ToListResponse()));
            })
            .WithSummary("Lista alla spel")
            .WithTags("Spel");

        app.MapGet("/api/games/{id:guid}", async (Guid id, GameLifecycleService svc, HttpContext ctx, CancellationToken ct) =>
            {

                var game = await svc.GetGameById(id, ct);
                if (game is null)
                    return Results.NotFound();
                if (game.IsPrivate && !IsAuthorized(ctx, id))
                    return Results.Json(new { isPrivate = true, name = game.Name }, statusCode: 403);
                return Results.Ok(game.ToDetailResponse());
            })
            .WithSummary("Hämta speldetalj")
            .WithTags("Spel");

        app.MapPost("/api/games/{id:guid}/unlock", async (Guid id, UnlockGameDto dto, GameLifecycleService svc, CancellationToken ct) =>
            {

                var game = await svc.GetGameById(id, ct);
                if (game is null)
                    return Results.NotFound();
                if (!game.IsPrivate)
                    return Results.BadRequest("Game is not private");
                return game.PasswordHash != GameTokenHelper.HashPassword(dto.Password)
                    ? Results.Unauthorized()
                    : Results.Ok(new { token = GameTokenHelper.GenerateToken(id) });
            })
            .WithSummary("Lås upp lösenordsskyddad match")
            .WithTags("Spel");

        app.MapPost("/api/games", async (CreateGameDto dto, GameLifecycleService svc, CancellationToken ct) =>
            {

                var game = GameFromDto(dto);
                await svc.CreateGame(game, ct);
                return Results.Created($"/games/{game.Id}", game.ToCreatedResponse());
            })
            .WithSummary("Skapa nytt spel")
            .WithTags("Spel");

        app.MapPut("/api/games/{id:guid}/settings",
                async (Guid id, UpdateGameSettingsDto dto, HttpContext ctx, GameLifecycleService svc, CancellationToken ct) =>
                {
                    var secretStr = ctx.Request.Headers["X-Creator-Secret"].ToString();
                    if (!Guid.TryParse(secretStr, out var secret))
                        return Results.Unauthorized();

                    var game = await svc.UpdateSettings(id, dto, secret, ct);
                    return game is null
                        ? Results.NotFound()
                        : Results.Ok(game.ToStatusResponse());
                })
            .WithSummary("Uppdatera lobbyinställningar")
            .WithTags("Spel");

        app.MapPut("/api/games/{id:guid}/start", async (Guid id, GameLifecycleService svc, CancellationToken ct) =>
            {

                var game = await svc.StartGame(id, ct);
                return game is null
                    ? Results.NotFound()
                    : Results.Ok(game.ToStatusResponse());
            })
            .WithSummary("Starta spel")
            .WithTags("Spel");

        app.MapPut("/api/games/{id:guid}/finish", async (Guid id, GameLifecycleService svc, CancellationToken ct) =>
            {
                var game = await svc.FinishGame(id, ct);
                if (game is null)
                    return Results.Conflict(new { error = "win_condition_not_met", message = "Vinstvillkoret är ännu inte uppfyllt. Avancera fler rundor." });
                return Results.Ok(game.ToFinishResponse());
            })
            .WithSummary("Avsluta spel och beräkna vinnare")
            .WithTags("Spel");

        app.MapPut("/api/games/{id:guid}/resume", async (Guid id, GameLifecycleService svc, CancellationToken ct) =>
            {
                var game = await svc.ResumeGame(id, ct);
                return game is null
                    ? Results.NotFound()
                    : Results.Ok(game.ToStatusResponse());
            })
            .WithSummary("Återuppta pausat spel utan att återställa rundan")
            .WithTags("Spel");

        app.MapPut("/api/games/{id:guid}/pause", async (Guid id, GameLifecycleService svc, CancellationToken ct) =>
            {

                var game = await svc.PauseGame(id, ct);
                return game is null
                    ? Results.NotFound()
                    : Results.Ok(game.ToStatusResponse());
            })
            .WithSummary("Pausa spel")
            .WithTags("Spel");

        app.MapPut("/api/games/{id:guid}/advance-round", async (Guid id, GameLifecycleService svc, CancellationToken ct) =>
            {

                var game = await svc.AdvanceRound(id, ct);
                return game is null
                    ? Results.NotFound()
                    : Results.Ok(new { game.Id, game.CurrentRound });
            })
            .WithSummary("Avancera till nästa runda")
            .WithTags("Spel");

        app.MapPut("/api/games/{id:guid}/reset", async (Guid id, HttpContext ctx, GameLifecycleService svc, CancellationToken ct) =>
            {
                var secretStr = ctx.Request.Headers["X-Creator-Secret"].ToString();
                if (!Guid.TryParse(secretStr, out var secret))
                    return Results.Unauthorized();
                var game = await svc.ResetGame(id, secret, ct);
                return game is null
                    ? Results.NotFound()
                    : Results.Ok(game.ToDetailResponse());
            })
            .WithSummary("Starta om match (nollställ poäng)")
            .WithTags("Spel");

        app.MapPost("/api/games/{id:guid}/rematch", async (Guid id, HttpContext ctx, GameLifecycleService svc, CancellationToken ct) =>
            {

                var secretStr = ctx.Request.Headers["X-Creator-Secret"].ToString();
                if (!Guid.TryParse(secretStr, out var secret))
                    return Results.Unauthorized();
                var newGame = await svc.RematchGame(id, secret, ct);
                return newGame is null
                    ? Results.NotFound()
                    : Results.Created($"/games/{newGame.Id}", newGame.ToCreatedResponse());
            })
            .WithSummary("Ny match med samma spelare och inställningar")
            .WithTags("Spel");

        return app;
    }

    private static Game GameFromDto(CreateGameDto dto) => new()
    {
        Id = Guid.NewGuid(),
        Name = dto.Name,
        LowerIsBetter = dto.LowerIsBetter,
        MaxRounds = dto.MaxRounds,
        StartingScore = dto.StartingScore,
        CreatorOnly = dto.CreatorOnly,
        ScoreIncrement = dto.ScoreIncrement,
        TeamBasedWinner = dto.TeamBasedWinner,
        GameMode = dto.GameMode,
        GameModeValue = dto.GameModeValue,
        GameModeTarget = dto.GameModeTarget,
        IsPrivate = dto.IsPrivate,
        PasswordHash = dto.IsPrivate && !string.IsNullOrWhiteSpace(dto.GamePassword)
            ? GameTokenHelper.HashPassword(dto.GamePassword)
            : null,
        CreatedAt = DateTime.UtcNow,
        IsTemporary = dto.IsTemporary,
        ExpiresAt = dto.IsTemporary && dto.ExpiresAt.HasValue
            ? dto.ExpiresAt.Value.Kind == DateTimeKind.Utc
                ? dto.ExpiresAt.Value
                : dto.ExpiresAt.Value.ToUniversalTime()
            : null,
    };

    private static bool IsAuthorized(HttpContext ctx, Guid gameId)
    {
        var auth = ctx.Request.Headers.Authorization.ToString();
        if (!auth.StartsWith("Bearer "))
            return false;
        return GameTokenHelper.ValidateToken(gameId, auth["Bearer ".Length..]);
    }
}
