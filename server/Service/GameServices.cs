using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Dtos;
using server.Hubs;
namespace server.Service;

public class GameServices(AppDbContext db, IHubContext<GameHub> hub)
{
    public async Task<List<Game>> GetAllGames()
    {
        return await db.Games
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<Game?> GetGameById(Guid id)
    {
        return await db.Games
            .Include(g => g.Teams)
            .Include(g => g.GamePlayers).ThenInclude(gp => gp.Player)
            .Include(g => g.GamePlayers).ThenInclude(gp => gp.Team)
            .Include(g => g.Scores).ThenInclude(s => s.Player)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Game> CreateGame(Game game)
    {
        db.Games.Add(game);
        await db.SaveChangesAsync();
        return game;
    }

    public async Task<Team> AddTeam(Team team)
    {
        db.Teams.Add(team);
        await db.SaveChangesAsync();
        await hub.Clients.Group(team.GameId.ToString())
            .SendAsync("GameUpdated");
        return team;
    }

    public async Task<GamePlayer?> AddPlayerToGame(GamePlayer gp)
    {
        var exists = await db.GamePlayers
            .AnyAsync(x => x.GameId == gp.GameId && x.PlayerId == gp.PlayerId);
        if (exists)
            return null;

        db.GamePlayers.Add(gp);
        await db.SaveChangesAsync();
        await hub.Clients.Group(gp.GameId.ToString())
            .SendAsync("GameUpdated");
        return gp;
    }

    public async Task<Score> AddScore(Score score)
    {
        var game = await db.Games.FindAsync(score.GameId);
        var startingScore = game?.StartingScore ?? 0;

        var previousSum = await db.Scores
            .Where(s => s.GameId == score.GameId && s.PlayerId == score.PlayerId)
            .SumAsync(s => s.Value);

        score.CumulativeValue = startingScore + previousSum + score.Value;

        db.Scores.Add(score);
        await db.SaveChangesAsync();

        await hub.Clients.Group(score.GameId.ToString())
            .SendAsync("ScoreAdded", new
            {
                score.Id,
                score.PlayerId,
                score.Round,
                score.Value,
                score.CumulativeValue,
                score.CreatedAt
            });

        // Kolla om "Först till X" är uppnått
        await CheckFirstToWin(score.GameId);

        return score;
    }

    public async Task<Score?> UpdateScoreValue(Guid gameId, Guid playerId, int round, double newValue)
    {
        var game = await db.Games.FindAsync(gameId);
        var startingScore = game?.StartingScore ?? 0;

        var playerScores = await db.Scores
            .Where(s => s.GameId == gameId && s.PlayerId == playerId)
            .OrderBy(s => s.Round)
            .ThenBy(s => s.CreatedAt)
            .ToListAsync();

        var target = playerScores.FirstOrDefault(s => s.Round == round);
        if (target == null)
            return null;

        target.Value = newValue;

        double running = startingScore;
        foreach (var s in playerScores)
        {
            running += s.Value;
            s.CumulativeValue = running;
        }

        await db.SaveChangesAsync();

        await hub.Clients.Group(gameId.ToString())
            .SendAsync("GameUpdated");

        // Kolla om "Först till X" är uppnått
        await CheckFirstToWin(gameId);

        return target;
    }

    public async Task<Game?> UpdateSettings(Guid gameId, UpdateGameSettingsDto dto, Guid creatorSecret)
    {
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
            game.GameMode = dto.GameMode;
        if (dto.GameModeValue.HasValue)
            game.GameModeValue = dto.GameModeValue.Value;

        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString())
            .SendAsync("GameUpdated");
        return game;
    }

    public async Task<Game?> FinishGame(Guid id)
    {
        var game = await db.Games
            .Include(g => g.Scores)
            .Include(g => g.GamePlayers)
            .Include(game => game.Teams)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null) return null;

        game.Status = GameStatus.Finished;
        game.FinishedAt = DateTime.UtcNow;

        if (game.TeamBasedWinner && game.Teams.Any())
        {
            // Lagbaserad vinnare
            var teamScores = game.Scores
                .Where(s => s.TeamId.HasValue)
                .GroupBy(s => s.TeamId!.Value)
                .Select(g => new { TeamId = g.Key, Total = g.Sum(s => s.Value) })
                .ToList();

            if (teamScores.Any())
            {
                var winningTeam = game.LowerIsBetter
                    ? teamScores.MinBy(t => t.Total)
                    : teamScores.MaxBy(t => t.Total);
                // WinnerId pekar på det vinnande lagets Id
                game.WinnerId = winningTeam?.TeamId;
            }
        }
        else
        {
            // Individuell vinnare
            var playerScores = game.Scores
                .GroupBy(s => s.PlayerId)
                .Select(g => new { PlayerId = g.Key, Total = g.Sum(s => s.Value) })
                .ToList();

            if (playerScores.Any())
            {
                game.WinnerId = game.LowerIsBetter
                    ? playerScores.MinBy(p => p.Total)?.PlayerId
                    : playerScores.MaxBy(p => p.Total)?.PlayerId;
            }
        }

        await db.SaveChangesAsync();
        await hub.Clients.Group(id.ToString())
            .SendAsync("GameUpdated");
        return game;
    }

    public async Task<Game?> StartGame(Guid id)
    {
        var game = await db.Games
            .Include(g => g.GamePlayers)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (game == null) return null;

        game.Status = GameStatus.Active;
        game.CurrentRound = 1;
        await db.SaveChangesAsync();
        await hub.Clients.Group(id.ToString())
            .SendAsync("GameUpdated");
        return game;
    }

    public async Task<Game?> PauseGame(Guid id)
    {
        var game = await db.Games.FindAsync(id);
        if (game == null || game.Status != GameStatus.Active) return null;

        game.Status = GameStatus.Waiting;
        await db.SaveChangesAsync();
        await hub.Clients.Group(id.ToString())
            .SendAsync("GameUpdated");
        return game;
    }

    public async Task<Game?> AdvanceRound(Guid id)
    {
        var game = await db.Games.FindAsync(id);
        if (game == null || game.Status != GameStatus.Active) return null;

        if (game.MaxRounds.HasValue && game.CurrentRound >= game.MaxRounds.Value)
            return game; // redan på sista rundan

        game.CurrentRound++;
        await db.SaveChangesAsync();
        await hub.Clients.Group(id.ToString())
            .SendAsync("GameUpdated");

        // Kolla "Bäst av X" efter varje runda
        await CheckBestOfWin(id);

        return game;
    }

    public async Task<GamePlayer?> AssignPlayerToTeam(Guid gameId, Guid playerId, Guid? teamId)
    {
        var gp = await db.GamePlayers
            .FirstOrDefaultAsync(x => x.GameId == gameId && x.PlayerId == playerId);
        if (gp == null) return null;

        gp.TeamId = teamId;
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString())
            .SendAsync("GameUpdated");
        return gp;
    }

    public async Task<GamePlayer?> ClaimPlayer(Guid gameId, Guid playerId, string connectionId)
    {
        var gp = await db.GamePlayers
            .FirstOrDefaultAsync(x => x.GameId == gameId && x.PlayerId == playerId);
        if (gp == null) return null;
        if (gp.ClaimedByConnectionId != null && gp.ClaimedByConnectionId != connectionId)
            return null; // redan claimad av någon annan

        gp.ClaimedByConnectionId = connectionId;
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString())
            .SendAsync("GameUpdated");
        return gp;
    }

    public async Task<GamePlayer?> UnclaimPlayer(Guid gameId, Guid playerId, string connectionId)
    {
        var gp = await db.GamePlayers
            .FirstOrDefaultAsync(x => x.GameId == gameId && x.PlayerId == playerId);
        if (gp == null) return null;
        if (gp.ClaimedByConnectionId != connectionId) return null;

        gp.ClaimedByConnectionId = null;
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString())
            .SendAsync("GameUpdated");
        return gp;
    }

    public async Task UnclaimByConnection(string connectionId)
    {
        var claimed = await db.GamePlayers
            .Where(gp => gp.ClaimedByConnectionId == connectionId)
            .ToListAsync();

        foreach (var gp in claimed)
        {
            gp.ClaimedByConnectionId = null;
            await hub.Clients.Group(gp.GameId.ToString())
                .SendAsync("GameUpdated");
        }

        if (claimed.Any())
            await db.SaveChangesAsync();
    }

    /// <summary>
    /// Kontrollera "Först till X" – om någon spelare/lag nått GameModeValue avslutas spelet.
    /// </summary>
    private async Task CheckFirstToWin(Guid gameId)
    {
        var game = await db.Games
            .Include(g => g.Scores)
            .FirstOrDefaultAsync(g => g.Id == gameId);
        if (game == null || game.Status != GameStatus.Active) return;
        if (game.GameMode != "FirstTo" || !game.GameModeValue.HasValue) return;

        var target = game.GameModeValue.Value;

        if (game.TeamBasedWinner)
        {
            var teamTotals = game.Scores
                .Where(s => s.TeamId.HasValue)
                .GroupBy(s => s.TeamId!.Value)
                .Select(g => new { TeamId = g.Key, Total = g.Sum(s => s.Value) });

            if (teamTotals.Any(t => t.Total >= target))
                await FinishGame(gameId);
        }
        else
        {
            var playerTotals = game.Scores
                .GroupBy(s => s.PlayerId)
                .Select(g => new { PlayerId = g.Key, Total = g.Sum(s => s.Value) });

            if (playerTotals.Any(p => p.Total >= target))
                await FinishGame(gameId);
        }
    }

    /// <summary>
    /// Kontrollera "Bäst av X" – om någon spelat tillräckligt många rundor och vunnit majoritet.
    /// </summary>
    private async Task CheckBestOfWin(Guid gameId)
    {
        var game = await db.Games
            .Include(g => g.Scores)
            .FirstOrDefaultAsync(g => g.Id == gameId);
        if (game == null || game.Status != GameStatus.Active) return;
        if (game.GameMode != "BestOf" || !game.GameModeValue.HasValue) return;

        var bestOf = game.GameModeValue.Value;
        var roundsToWin = (bestOf / 2) + 1;

        // Räkna rundvinster per spelare (vinnaren av varje runda)
        var roundWins = new Dictionary<Guid, int>();
        var roundGroups = game.Scores.GroupBy(s => s.Round);

        foreach (var rg in roundGroups)
        {
            var playerTotals = rg.GroupBy(s => s.PlayerId)
                .Select(g => new { PlayerId = g.Key, Total = g.Sum(s => s.Value) });

            var winner = game.LowerIsBetter
                ? playerTotals.MinBy(p => p.Total)
                : playerTotals.MaxBy(p => p.Total);

            if (winner != null)
            {
                roundWins.TryAdd(winner.PlayerId, 0);
                roundWins[winner.PlayerId]++;
            }
        }

        if (roundWins.Values.Any(w => w >= roundsToWin))
            await FinishGame(gameId);
    }
}
