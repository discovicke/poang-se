using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using server.Dtos;
using server.Hubs;
using server.Models;
using server.Helpers;
namespace server.Service;

/// <summary>
/// Hanterar spelets livscykel: CRUD, statusövergångar (start/paus/finish),
/// reset, rematch, avancera runda och inställningar.
/// </summary>
public class GameLifecycleService(AppDbContext db, IHubContext<GameHub> hub, GameScoringService scoring, CancellationManager.TokenLinker tokenLinker)
{
    /// <summary>Returnerar alla spel sorterade med senast skapade först.</summary>
    public async Task<List<Game>> GetAllGames(CancellationToken requestCt = default)
    {
        using var ct = tokenLinker.Link(requestCt);
        return await db.Games
            .Where(g => g.ExpiresAt == null || g.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Hämtar ett spel med fullständiga includes: lag, spelare (med spelarentitet och lag)
    /// samt poäng (med spelarentitet).
    /// </summary>
    public async Task<Game?> GetGameById(Guid id, CancellationToken requestCt = default)
    {
        using var ct = tokenLinker.Link(requestCt);
        return await db.Games
            .Include(g => g.Teams)
            .Include(g => g.GamePlayers).ThenInclude(gp => gp.Player)
            .Include(g => g.GamePlayers).ThenInclude(gp => gp.Team)
            .Include(g => g.Scores).ThenInclude(s => s.Player)
            .FirstOrDefaultAsync(g => g.Id == id &&
                (g.ExpiresAt == null || g.ExpiresAt > DateTime.UtcNow), ct);
    }

    /// <summary>Sparar ett nytt spel i databasen och returnerar det.</summary>
    public async Task<Game> CreateGame(Game game, CancellationToken requestCt = default)
    {
        using var ct = tokenLinker.Link(requestCt);
        db.Games.Add(game);
        await db.SaveChangesAsync(ct);
        return game;
    }

    /// <summary>
    /// Uppdaterar lobbyinställningar för ett spel om anroparen är creator
    /// och spelet fortfarande är i <c>Waiting</c>-status.
    /// </summary>
    public async Task<Game?> UpdateSettings(Guid gameId, UpdateGameSettingsDto dto, Guid creatorSecret, CancellationToken requestCt = default)
    {
        using var ct = tokenLinker.Link(requestCt);
        var game = await db.Games.FindAsync(gameId);
        if (game == null)
            return null;
        if (game.CreatorSecret != creatorSecret)
            return null;
        if (game.Status != GameStatus.Waiting)
            return null;

        if (dto.MaxRounds.HasValue)
            game.MaxRounds = dto.MaxRounds.Value;
        if (dto.ScoreIncrement.HasValue)
            game.ScoreIncrement = dto.ScoreIncrement.Value;
        if (dto.LowerIsBetter.HasValue)
            game.LowerIsBetter = dto.LowerIsBetter.Value;
        if (dto.CreatorOnly.HasValue)
            game.CreatorOnly = dto.CreatorOnly.Value;
        if (dto.TeamBasedWinner.HasValue)
            game.TeamBasedWinner = dto.TeamBasedWinner.Value;
        if (dto.GameMode != null)
        {
            game.GameMode = dto.GameMode;
            // BestOf/FirstTo: rundantalet växer organiskt – nolla MaxRounds
            if (dto.GameMode is "BestOf" or "FirstTo")
                game.MaxRounds = null;
        }
        if (dto.GameModeValue.HasValue)
        {
            var val = dto.GameModeValue.Value;
            // BestOf kräver udda tal: jämna avrundas upp
            if (game.GameMode == "BestOf" && val % 2 == 0)
                val++;
            game.GameModeValue = val;
        }
        if (dto.GameModeTarget != null)
            game.GameModeTarget = dto.GameModeTarget;
        if (dto.StartingScore.HasValue)
            game.StartingScore = dto.StartingScore.Value;

        await db.SaveChangesAsync(ct);
        await hub.Clients.Group(gameId.ToString()).SendAsync("GameUpdated");
        return game;
    }

    /// <summary>Startar spelet (status → Active, CurrentRound = 1).</summary>
    public async Task<Game?> StartGame(Guid id, CancellationToken requestCt = default)
    {
        using var ct = tokenLinker.Link(requestCt);
        var game = await db.Games
            .Include(g => g.GamePlayers)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
        if (game == null)
            return null;

        game.Status = GameStatus.Active;
        game.CurrentRound = 1;
        await db.SaveChangesAsync(ct);
        await hub.Clients.Group(id.ToString()).SendAsync("GameUpdated");
        return game;
    }

    /// <summary>Återupptar ett pausat spel (status → Active) utan att återställa CurrentRound.</summary>
    public async Task<Game?> ResumeGame(Guid id, CancellationToken requestCt = default)
    {
        using var ct = tokenLinker.Link(requestCt);
        var game = await db.Games.FindAsync(new object[] { id }, ct);
        if (game == null || game.Status != GameStatus.Waiting)
            return null;

        game.Status = GameStatus.Active;
        await db.SaveChangesAsync(ct);
        await hub.Clients.Group(id.ToString()).SendAsync("GameUpdated");
        return game;
    }

    /// <summary>Pausar ett aktivt spel (status → Waiting).</summary>
    public async Task<Game?> PauseGame(Guid id, CancellationToken requestCt = default)
    {
        using var ct = tokenLinker.Link(requestCt);
        var game = await db.Games.FindAsync(new object[] { id }, ct);
        if (game == null || game.Status != GameStatus.Active)
            return null;

        game.Status = GameStatus.Waiting;
        await db.SaveChangesAsync(ct);
        await hub.Clients.Group(id.ToString()).SendAsync("GameUpdated");
        return game;
    }

    /// <summary>
    /// Ökar CurrentRound med ett steg. Kontrollerar rundbaserade vinstvillkor efter avslutad runda.
    /// </summary>
    public async Task<Game?> AdvanceRound(Guid id, CancellationToken requestCt = default)
    {
        using var ct = tokenLinker.Link(requestCt);
        var game = await db.Games
            .Include(g => g.Scores)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
        if (game is not { Status: GameStatus.Active })
            return null;

        bool isRoundBased = game.GameMode == "BestOf" ||
                            game is
                            {
                                GameMode: "FirstTo",
                                GameModeTarget: "rounds"
                            };

        if (isRoundBased && game.GameModeValue.HasValue)
        {
            var roundsToWin = game.GameMode == "BestOf"
                ? (game.GameModeValue.Value / 2) + 1
                : game.GameModeValue.Value;
            var roundWins = scoring.CountRoundWins(game);
            if (roundWins.Values.Any(w => w >= roundsToWin))
            {
                // Vinstvillkoret är uppfyllt – avsluta spelet istället för att avancera runda
                await scoring.CheckRoundBasedWin(id, ct);
                return game;
            }
        }
        else if (game.MaxRounds.HasValue && game.CurrentRound >= game.MaxRounds.Value)
        {
            return game;
        }

        game.CurrentRound++;
        await db.SaveChangesAsync(ct);
        await hub.Clients.Group(id.ToString()).SendAsync("GameUpdated");

        // Kontrollera ALLA vinstvillkor när rundan avslutas
        await scoring.CheckRoundBasedWin(id, ct);
        await scoring.CheckFirstToWin(id, ct);
        return game;
    }

    /// <summary>Avslutar spelet, beräknar vinnare och notifierar gruppen.</summary>
    public async Task<Game?> FinishGame(Guid id, CancellationToken requestCt = default)
    {
        using var ct = tokenLinker.Link(requestCt);
        var game = await db.Games
            .Include(g => g.Scores)
            .Include(g => g.GamePlayers)
            .Include(g => g.Teams)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
        if (game == null)
            return null;

        // För BestOf/FirstTo-lägen: blockera manuell avslutning om vinstvillkoret inte uppnåtts
        if (game.GameModeValue.HasValue && game.GameMode is "BestOf" or "FirstTo")
        {
            bool winMet;

            if (game.GameMode == "BestOf" || game.GameModeTarget == "rounds")
            {
                var roundsToWin = game.GameMode == "BestOf"
                    ? (game.GameModeValue.Value / 2) + 1
                    : game.GameModeValue.Value;
                var wins = scoring.CountRoundWins(game);
                winMet = wins.Values.Any(w => w >= roundsToWin);
            }
            else // FirstTo points
            {
                var target = game.GameModeValue.Value;
                var totals = game.Scores
                    .GroupBy(s => game.TeamBasedWinner && s.TeamId.HasValue ? s.TeamId!.Value : s.PlayerId)
                    .Select(g => g.Sum(s => s.Value));
                winMet = totals.Any(t => t >= target);
            }

            if (!winMet)
                return null; // vinstvillkor ej uppfyllt -> endpoint returnerar 409
        }

        game.Status = GameStatus.Finished;
        game.FinishedAt = DateTime.UtcNow;
        game.WinnerId = scoring.CalculateWinnerId(game);

        await db.SaveChangesAsync(ct);
        await hub.Clients.Group(id.ToString()).SendAsync("GameUpdated");
        return game;
    }

    /// <summary>Återställer ett spel till Waiting. Alla poäng raderas.</summary>
    public async Task<Game?> ResetGame(Guid id, Guid creatorSecret, CancellationToken requestCt = default)
    {
        using var ct = tokenLinker.Link(requestCt);
        var game = await db.Games
            .Include(g => g.Scores)
            .Include(g => g.GamePlayers)
            .Include(g => g.Teams)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (game == null || game.CreatorSecret != creatorSecret)
            return null;

        db.Scores.RemoveRange(game.Scores);
        game.Status = GameStatus.Waiting;
        game.CurrentRound = 1;
        game.WinnerId = null;
        game.FinishedAt = null;

        await db.SaveChangesAsync(ct);
        await hub.Clients.Group(id.ToString()).SendAsync("GameUpdated");
        return game;
    }

    /// <summary>Skapar en ny match baserad på en befintlig: samma inställningar, lag och spelare.</summary>
    public async Task<Game?> RematchGame(Guid id, Guid creatorSecret, CancellationToken ct = default)
    {
        var original = await db.Games
            .Include(g => g.Teams)
            .Include(g => g.GamePlayers)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (original == null || original.CreatorSecret != creatorSecret)
            return null;

        var newGame = new Game
        {
            Id = Guid.NewGuid(),
            Name = original.Name,
            LowerIsBetter = original.LowerIsBetter,
            MaxRounds = original.MaxRounds,
            StartingScore = original.StartingScore,
            CreatorOnly = original.CreatorOnly,
            ScoreIncrement = original.ScoreIncrement,
            TeamBasedWinner = original.TeamBasedWinner,
            GameMode = original.GameMode,
            GameModeValue = original.GameModeValue,
            GameModeTarget = original.GameModeTarget,
            IsPrivate = original.IsPrivate,
            PasswordHash = original.PasswordHash,
            IsTemporary = original.IsTemporary,
            ExpiresAt = original.ExpiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatorSecret = Guid.NewGuid(),
        };
        db.Games.Add(newGame);

        var teamMap = new Dictionary<Guid, Guid>();
        foreach (var t in original.Teams)
        {
            var newTeamId = Guid.NewGuid();
            teamMap[t.Id] = newTeamId;
            db.Teams.Add(new Team { Id = newTeamId, GameId = newGame.Id, Name = t.Name });
        }

        foreach (var gp in original.GamePlayers)
        {
            db.GamePlayers.Add(new GamePlayer
            {
                GameId = newGame.Id,
                PlayerId = gp.PlayerId,
                TeamId = gp.TeamId.HasValue && teamMap.TryGetValue(gp.TeamId.Value, out var mapped)
                    ? mapped
                    : null,
                JoinedAt = DateTime.UtcNow,
            });
        }

        await db.SaveChangesAsync(ct);
        return newGame;
    }

    /// <summary>Tar bort alla spel vars ExpiresAt har passerat.</summary>
    public async Task<int> RemoveExpiredGames(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var expired = await db.Games
            .Where(g => g.ExpiresAt != null && g.ExpiresAt <= now)
            .ToListAsync(ct);
        db.Games.RemoveRange(expired);
        await db.SaveChangesAsync(ct);
        return expired.Count;
    }
}

