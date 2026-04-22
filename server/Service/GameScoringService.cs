using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using server.Hubs;
using server.Models;

namespace server.Service;

/// <summary>
/// Hanterar poängregistrering, poänguppdatering och alla vinstvillkorsberäkningar.
/// </summary>
public class GameScoringService(AppDbContext db, IHubContext<GameHub> hub)
{
    /// <summary>Registrerar en poäng och kontrollerar "Först till X poäng".</summary>
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

        await CheckFirstToWin(score.GameId);
        return score;
    }

    /// <summary>Uppdaterar poängen för en specifik spelare och runda. Räknar om CumulativeValue.</summary>
    public async Task<Score?> UpdateScoreValue(Guid gameId, Guid playerId, int round, double newValue)
    {
        var game = await db.Games.FindAsync(gameId);
        var startingScore = game?.StartingScore ?? 0;

        var playerScores = await db.Scores
            .Where(s => s.GameId == gameId && s.PlayerId == playerId)
            .OrderBy(s => s.Round).ThenBy(s => s.CreatedAt)
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
        await hub.Clients.Group(gameId.ToString()).SendAsync("GameUpdated");

        await CheckFirstToWin(gameId);
        return target;
    }

    // ─── Vinstlogik (publika för att GameLifecycleService ska kunna anropa dem) ───

    /// <summary>Beräknar vinnare-id för ett spel. BestOf/FirstTo-rounds → rundvinster, annars totalpoäng.</summary>
    public Guid? CalculateWinnerId(Game game)
    {
        if (game.GameMode == "BestOf" ||
            (game.GameMode == "FirstTo" && game.GameModeTarget == "rounds"))
        {
            var roundWins = CountRoundWins(game);
            if (!roundWins.Any())
                return null;
            return game.LowerIsBetter
                ? roundWins.MinBy(kv => kv.Value).Key
                : roundWins.MaxBy(kv => kv.Value).Key;
        }

        if (game.TeamBasedWinner && game.Teams.Any())
        {
            var totals = game.Scores
                .Where(s => s.TeamId.HasValue)
                .GroupBy(s => s.TeamId!.Value)
                .Select(g => new { Id = g.Key, Total = g.Sum(s => s.Value) })
                .ToList();
            return game.LowerIsBetter
                ? totals.MinBy(t => t.Total)?.Id
                : totals.MaxBy(t => t.Total)?.Id;
        }
        else
        {
            var totals = game.Scores
                .GroupBy(s => s.PlayerId)
                .Select(g => new { Id = g.Key, Total = g.Sum(s => s.Value) })
                .ToList();
            return game.LowerIsBetter
                ? totals.MinBy(p => p.Total)?.Id
                : totals.MaxBy(p => p.Total)?.Id;
        }
    }

    /// <summary>Räknar rundvinster per spelare/lag. Oavgjorda rundor ger ingen poäng.</summary>
    public Dictionary<Guid, int> CountRoundWins(Game game)
    {
        var roundWins = new Dictionary<Guid, int>();
        var useTeams = game.TeamBasedWinner && game.Teams.Count != 0;

        foreach (var roundGroup in game.Scores.GroupBy(s => s.Round))
        {
            var totals = useTeams
                ? roundGroup.Where(s => s.TeamId.HasValue)
                    .GroupBy(s => s.TeamId!.Value)
                    .Select(g => new { Id = g.Key, Total = g.Sum(s => s.Value) })
                    .ToList()
                : roundGroup.GroupBy(s => s.PlayerId)
                    .Select(g => new { Id = g.Key, Total = g.Sum(s => s.Value) })
                    .ToList();

            if (!totals.Any())
                continue;

            var best = game.LowerIsBetter
                ? totals.Min(t => t.Total)
                : totals.Max(t => t.Total);
            var winners = totals.Where(t => Math.Abs(t.Total - best) < 1e-9).ToList();
            if (winners.Count != 1)
                continue;

            roundWins.TryAdd(winners[0].Id, 0);
            roundWins[winners[0].Id]++;
        }

        return roundWins;
    }

    /// <summary>Kontrollerar "Först till X poäng" vid poängändring.</summary>
    public async Task CheckFirstToWin(Guid gameId)
    {
        var game = await db.Games.Include(g => g.Scores).FirstOrDefaultAsync(g => g.Id == gameId);
        if (game is not { Status: GameStatus.Active })
            return;
        if (game.GameMode != "FirstTo" || !game.GameModeValue.HasValue)
            return;
        if (game.GameModeTarget == "rounds")
            return;

        var target = game.GameModeValue.Value;
        if (game.TeamBasedWinner)
        {
            var totals = game.Scores.Where(s => s.TeamId.HasValue)
                .GroupBy(s => s.TeamId!.Value)
                .Select(g => new { Total = g.Sum(s => s.Value) });
            if (totals.Any(t => t.Total >= target))
                await FinishGameInternal(game);
        }
        else
        {
            var totals = game.Scores.GroupBy(s => s.PlayerId)
                .Select(g => new { Total = g.Sum(s => s.Value) });
            if (totals.Any(p => p.Total >= target))
                await FinishGameInternal(game);
        }
    }

    /// <summary>Kontrollerar rundbaserade vinstvillkor (BestOf, FirstTo-rounds).</summary>
    public async Task CheckRoundBasedWin(Guid gameId)
    {
        var game = await db.Games
            .Include(g => g.Scores).Include(g => g.Teams)
            .FirstOrDefaultAsync(g => g.Id == gameId);
        if (game is not { Status: GameStatus.Active })
            return;
        if (!game.GameModeValue.HasValue)
            return;

        int roundsToWin;
        switch (game.GameMode)
        {
            case "BestOf":
                roundsToWin = (game.GameModeValue.Value / 2) + 1;
                break;
            case "FirstTo" when game.GameModeTarget == "rounds":
                roundsToWin = game.GameModeValue.Value;
                break;
            default:
                return;
        }

        var roundWins = CountRoundWins(game);
        if (roundWins.Values.Any(w => w >= roundsToWin))
            await FinishGameInternal(game);
    }

    /// <summary>Intern finish som tar ett redan laddat game-objekt.</summary>
    private async Task FinishGameInternal(Game game)
    {
        game.Status = GameStatus.Finished;
        game.FinishedAt = DateTime.UtcNow;
        game.WinnerId = CalculateWinnerId(game);
        await db.SaveChangesAsync();
        await hub.Clients.Group(game.Id.ToString()).SendAsync("GameUpdated");
    }

    //<summary>Bygger dataobjekt för poängdiagram baserat på spelets poänghistorik.</summary>
    public object BuildScoreChartData(Game game)
    {
        var playerScores = game.Scores
        .GroupBy(s => s.Player.UserName)
        .Select(g => new
        {
            PlayerName = g.Key,
            Scores = g.OrderBy(s => s.Round)
            .Where(s => s.Round != null)
            .ToDictionary(s => s.Round!.Value),
            CumulativeScores = Enumerable.Range(1, game.CurrentRound)
            .Select(round =>
            {
                var scoreEachRound = g.FirstOrDefault(s => s.Round == round);
                if (scoreEachRound != null)
                    return scoreEachRound?.CumulativeValue ?? 0;

                return g.Where(s => s.Round < round)
                .OrderByDescending(s => s.Round)
                .FirstOrDefault()?.CumulativeValue ?? 0;
            })
            .ToList()
        })
            .ToList();

        var datasets = playerScores.Select((player, index) => new
        {

            label = player.PlayerName,
            data = player.CumulativeScores,
            fill = false,
            borderColor = $"hsl({index * 360 / playerScores.Count()}, 70%, 50%)",
            tension = 0.1
        }).ToList();

        var labels = Enumerable
        .Range(1, game.CurrentRound)
        .Select(r => $"Runda {r}")
        .ToList();

        return new
        {
            labels,
            datasets,
        };
    }


}

