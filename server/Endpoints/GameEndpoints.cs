using server.Dtos;
using server.Helpers;
using server.Mappers;
using server.Models;
using server.Service;

namespace server.Endpoints;

/// <summary>
/// Registrerar alla spel-relaterade Minimal API-endpoints under prefixet <c>/api/games</c>.
/// Delegerar affärslogik till <see cref="GameServices"/> och responsmappning till <see cref="GameMapper"/>.
/// </summary>
public static class GameEndpointMapper
{
    /// <summary>
    /// Kopplar endpoints till <paramref name="app"/>:
    /// <list type="bullet">
    ///   <item><c>GET    /api/games</c> – lista alla spel</item>
    ///   <item><c>GET    /api/games/{id}</c> – fullständig speldetalj</item>
    ///   <item><c>POST   /api/games</c> – skapa spel</item>
    ///   <item><c>PUT    /api/games/{id}/settings</c> – uppdatera lobbyinställningar</item>
    ///   <item><c>POST   /api/games/{id}/teams</c> – lägg till lag</item>
    ///   <item><c>POST   /api/games/{id}/players</c> – lägg till befintlig spelare</item>
    ///   <item><c>POST   /api/games/{id}/players/new</c> – skapa och lägg till ny spelare</item>
    ///   <item><c>PUT    /api/games/{id}/players/team</c> – tilldela spelare till lag</item>
    ///   <item><c>POST   /api/games/{id}/scores</c> – registrera poäng</item>
    ///   <item><c>PUT    /api/games/{id}/scores</c> – uppdatera poäng i specifik runda</item>
    ///   <item><c>PUT    /api/games/{id}/start</c> – starta spel</item>
    ///   <item><c>PUT    /api/games/{id}/pause</c> – pausa spel</item>
    ///   <item><c>PUT    /api/games/{id}/advance-round</c> – avancera till nästa runda</item>
    ///   <item><c>PUT    /api/games/{id}/finish</c> – avsluta spel och beräkna vinnare</item>
    /// </list>
    /// </summary>
    public static WebApplication GameEndpoints(this WebApplication app)
    {
        app.MapGet("/api/games", async (GameServices svc) =>
        {
            var expiredGames = await svc.RemoveExpiredGames();
            var games = await svc.GetAllGames();
            return Results.Ok(games.Select(g => g.ToListResponse()));
        });

        app.MapGet("/api/games/{id:guid}", async (Guid id, GameServices svc, HttpContext ctx) =>
        {
            var game = await svc.GetGameById(id);
            if (game is null)
                return Results.NotFound();

            if (game.IsPrivate && !IsAuthorized(ctx, id))
                return Results.Json(new { isPrivate = true, name = game.Name }, statusCode: 403);

            return Results.Ok(game.ToDetailResponse());
        });

        app.MapPost("/api/games/{id:guid}/unlock", async (Guid id, UnlockGameDto dto, GameServices svc) =>
        {
            var game = await svc.GetGameById(id);
            if (game is null)
                return Results.NotFound();
            if (!game.IsPrivate)
                return Results.BadRequest("Game is not private");
            if (game.PasswordHash != GameTokenHelper.HashPassword(dto.Password))
                return Results.Unauthorized();

            var token = GameTokenHelper.GenerateToken(id);
            return Results.Ok(new { token });
        });

        app.MapPost("/api/games", async (CreateGameDto dto, GameServices svc) =>
        {
            var game = GameFromDto(dto);
            await svc.CreateGame(game);
            return Results.Created($"/games/{game.Id}", game.ToCreatedResponse());
        });

        app.MapPut("/api/games/{id:guid}/settings", async (Guid id, UpdateGameSettingsDto dto, HttpContext ctx, GameServices svc) =>
        {
            var secretStr = ctx.Request.Headers["X-Creator-Secret"].ToString();
            if (!Guid.TryParse(secretStr, out var secret))
                return Results.Unauthorized();

            var game = await svc.UpdateSettings(id, dto, secret);
            return game is null
                ? Results.NotFound()
                : Results.Ok(game.ToStatusResponse());
        });

        app.MapPost("/api/games/{id:guid}/teams", async (Guid id, AddTeamDto dto, GameServices svc) =>
        {
            var team = new Team { Id = Guid.NewGuid(), GameId = id, Name = dto.Name };
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
            return result is null
                ? Results.Conflict("Player already in game")
                : Results.Created($"/games/{id}", new { gp.GameId, gp.PlayerId, gp.TeamId });
        });

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

        app.MapPut("/api/games/{id:guid}/players/team", async (Guid id, AssignPlayerToTeamDto dto, GameServices svc) =>
        {
            var result = await svc.AssignPlayerToTeam(id, dto.PlayerId, dto.TeamId);
            return result is null
                ? Results.NotFound()
                : Results.Ok(new { result.PlayerId, result.TeamId });
        });

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
            return Results.Created($"/games/{id}/scores/{result.Id}", result.ToScoreResponse());
        });

        app.MapPut("/api/games/{id:guid}/scores", async (Guid id, UpdateScoreValueDto dto, GameServices svc) =>
        {
            var result = await svc.UpdateScoreValue(id, dto.PlayerId, dto.Round, dto.Value);
            return result is null
                ? Results.NotFound()
                : Results.Ok(result.ToScoreResponse());
        });

        app.MapPut("/api/games/{id:guid}/start", async (Guid id, GameServices svc) =>
        {
            var game = await svc.StartGame(id);
            return game is null
                ? Results.NotFound()
                : Results.Ok(game.ToStatusResponse());
        });

        app.MapPut("/api/games/{id:guid}/pause", async (Guid id, GameServices svc) =>
        {
            var game = await svc.PauseGame(id);
            return game is null
                ? Results.NotFound()
                : Results.Ok(game.ToStatusResponse());
        });

        app.MapPut("/api/games/{id:guid}/advance-round", async (Guid id, GameServices svc) =>
        {
            var game = await svc.AdvanceRound(id);
            return game is null
                ? Results.NotFound()
                : Results.Ok(new { game.Id, game.CurrentRound });
        });

        app.MapPut("/api/games/{id:guid}/finish", async (Guid id, GameServices svc) =>
        {
            var game = await svc.FinishGame(id);
            return game is null
                ? Results.NotFound()
                : Results.Ok(game.ToFinishResponse());
        });

        app.MapPut("/api/games/{id:guid}/reset", async (Guid id, HttpContext ctx, GameServices svc) =>
        {
            var secretStr = ctx.Request.Headers["X-Creator-Secret"].ToString();
            if (!Guid.TryParse(secretStr, out var secret))
                return Results.Unauthorized();

            var game = await svc.ResetGame(id, secret);
            return game is null
                ? Results.NotFound()
                : Results.Ok(game.ToDetailResponse());
        });

        app.MapPost("/api/games/{id:guid}/rematch", async (Guid id, HttpContext ctx, GameServices svc) =>
        {
            var secretStr = ctx.Request.Headers["X-Creator-Secret"].ToString();
            if (!Guid.TryParse(secretStr, out var secret))
                return Results.Unauthorized();

            var newGame = await svc.RematchGame(id, secret);
            return newGame is null
                ? Results.NotFound()
                : Results.Created($"/games/{newGame.Id}", newGame.ToCreatedResponse());
        });

        return app;
    }

    /// <summary>
    /// Skapar en ny <see cref="Game"/>-entitet från ett <see cref="CreateGameDto"/>.
    /// Isolerar entity-konstruktion från endpoint-lambdan.
    /// </summary>
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
        var token = auth["Bearer ".Length..];
        return GameTokenHelper.ValidateToken(gameId, token);
    }
}
