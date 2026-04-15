using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using server.Models;
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
        return gp;
    }

    public async Task<Score> AddScore(Score score)
    {
        // Räkna ut kumulativ (total fram till en tidpunkt) poäng för spelaren i det aktuella spelet
        var previousSum = await db.Scores
            .Where(s => s.GameId == score.GameId && s.PlayerId == score.PlayerId)
            .SumAsync(s => s.Value);

        score.CumulativeValue = previousSum + score.Value;

        db.Scores.Add(score);
        await db.SaveChangesAsync();

        await hub.Clients.Group(score.GameId.ToString())
            .SendAsync("ScoreAdded", new
            {
                score.Id,
                score.PlayerId,
                score.Value,
                score.CumulativeValue,
                score.CreatedAt
            });
        return score;
    }

    public async Task<Score?> UpdateScoreValue(Guid gameId, Guid playerId, int round, double newValue)
    {
        var playerScores = await db.Scores
            .Where(s => s.GameId == gameId && s.PlayerId == playerId)
            .OrderBy(s => s.Round)
            .ThenBy(s => s.CreatedAt)
            .ToListAsync();

        var targetRound = playerScores.FirstOrDefault(s => s.Round == round);
        if (targetRound == null)
            return null;

        targetRound.Value = newValue;

        double running = 0;
        foreach (var score in playerScores)
        {
            running += score.Value;
            score.CumulativeValue = running;
        }

        await db.SaveChangesAsync();
        return targetRound;
    }

    public async Task<Game?> FinishGame(Guid id)
    {
        var game = await db.Games
            .Include(g => g.Scores)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null)
            return null;

        game.Status = GameStatus.Finished;
        game.FinishedAt = DateTime.UtcNow;

        // Räkna ut total poäng per spelare och avgör vinnaren baserat på om LowerIsBetter
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

        await db.SaveChangesAsync();
        return game;
    }

    public async Task<Game?> StartGame(Guid id)
    {
        var game = await db.Games.FindAsync(id);
        if (game == null)
            return null;

        game.Status = GameStatus.Active;
        await db.SaveChangesAsync();
        return game;
    }
}

