using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Dtos;
using server.Hubs;
namespace server.Service;

/// <summary>
/// Affärslogik för spel. Ansvarar för CRUD, statusövergångar, poänghantering,
/// lag-/spelarhantering samt SignalR-notifieringar till anslutna klienter.
/// </summary>
public class GameServices(AppDbContext db, IHubContext<GameHub> hub)
{
    /// <summary>Returnerar alla spel sorterade med senast skapade först.</summary>
    public async Task<List<Game>> GetAllGames()
    {
        return await db.Games
            .Where(g => g.ExpiresAt == null || g.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Hämtar ett spel med fullständiga includes: lag, spelare (med spelarentitet och lag)
    /// samt poäng (med spelarentitet).
    /// </summary>
    public async Task<Game?> GetGameById(Guid id)
    {
        return await db.Games
            .Include(g => g.Teams)
            .Include(g => g.GamePlayers).ThenInclude(gp => gp.Player)
            .Include(g => g.GamePlayers).ThenInclude(gp => gp.Team)
            .Include(g => g.Scores).ThenInclude(s => s.Player)
            .FirstOrDefaultAsync(g => g.Id == id &&
            (g.ExpiresAt == null || g.ExpiresAt > DateTime.UtcNow));
    }

    /// <summary>Sparar ett nytt spel i databasen och returnerar det.</summary>
    public async Task<Game> CreateGame(Game game)
    {
        db.Games.Add(game);
        await db.SaveChangesAsync();
        return game;
    }

    /// <summary>Lägger till ett lag i ett spel och notifierar gruppen via SignalR.</summary>
    public async Task<Team> AddTeam(Team team)
    {
        db.Teams.Add(team);
        await db.SaveChangesAsync();
        await hub.Clients.Group(team.GameId.ToString())
            .SendAsync("GameUpdated");
        return team;
    }

    /// <summary>
    /// Lägger till en spelare i ett spel om spelaren inte redan är med.
    /// Returnerar <c>null</c> vid dubblett.
    /// </summary>
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

    /// <summary>
    /// Registrerar en poäng och beräknar <c>CumulativeValue</c> baserat på
    /// spelarens befintliga poäng och spelets <c>StartingScore</c>.
    /// Kontrollerar därefter om "Först till X"-villkoret är uppfyllt.
    /// </summary>
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

        // Kolla om "Först till X" är uppnått (BestOf avgörs bara vid rundavslut)
        await CheckFirstToWin(score.GameId);

        return score;
    }

    /// <summary>
    /// Uppdaterar poängen för en specifik spelare och runda, och räknar om
    /// <c>CumulativeValue</c> för alla efterföljande rundor.
    /// Kontrollerar därefter om "Först till X"-villkoret är uppfyllt.
    /// Returnerar <c>null</c> om rundan inte finns.
    /// </summary>
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

        // Kolla om "Först till X" är uppnått (BestOf avgörs bara vid rundavslut)
        await CheckFirstToWin(gameId);

        return target;
    }

    /// <summary>
    /// Uppdaterar lobbyinställningar för ett spel om anroparen är creator och
    /// spelet fortfarande är i <c>Waiting</c>-status.
    /// Returnerar <c>null</c> om spelet inte finns eller om secret är fel.
    /// </summary>
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
        if (dto.GameModeTarget != null)
            game.GameModeTarget = dto.GameModeTarget;

        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString())
            .SendAsync("GameUpdated");
        return game;
    }

    /// <summary>
    /// Avslutar spelet, sätter status till <c>Finished</c>, beräknar vinnaren
    /// via <see cref="CalculateWinnerId"/> och notifierar gruppen.
    /// </summary>
    public async Task<Game?> FinishGame(Guid id)
    {
        var game = await db.Games
            .Include(g => g.Scores)
            .Include(g => g.GamePlayers)
            .Include(g => g.Teams)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null)
            return null;

        game.Status = GameStatus.Finished;
        game.FinishedAt = DateTime.UtcNow;
        game.WinnerId = CalculateWinnerId(game);

        await db.SaveChangesAsync();
        await hub.Clients.Group(id.ToString()).SendAsync("GameUpdated");
        return game;
    }

    /// <summary>
    /// Beräknar och returnerar id:t för vinnaren av <paramref name="game"/>.
    /// För "Bäst av X" räknas rundvinster; annars summeras poäng.
    /// Stödjer både lagbaserad och individuell vinnarlogik med hänsyn till <c>LowerIsBetter</c>.
    /// </summary>
    private Guid? CalculateWinnerId(Game game)
    {
        // BestOf och FirstTo-rounds: vinnaren är den med flest rundvinster
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

    /// <summary>
    /// Räknar antalet rundvinster per spelare (eller lag om TeamBasedWinner).
    /// Returnerar en Dictionary med Id -> antal vunna rundor.
    /// Rundor utan tydlig vinnare (oavgjort) räknas inte till någon.
    /// </summary>
    private Dictionary<Guid, int> CountRoundWins(Game game)
    {
        var roundWins = new Dictionary<Guid, int>();
        var roundGroups = game.Scores.GroupBy(s => s.Round);

        foreach (var rg in roundGroups)
        {
            if (game.TeamBasedWinner && game.Teams.Any())
            {
                // Summera poäng per lag för denna runda
                var teamTotals = rg
                    .Where(s => s.TeamId.HasValue)
                    .GroupBy(s => s.TeamId!.Value)
                    .Select(g => new { Id = g.Key, Total = g.Sum(s => s.Value) })
                    .ToList();

                if (!teamTotals.Any())
                    continue;
                var maxTotal = game.LowerIsBetter
                    ? teamTotals.Min(t => t.Total)
                    : teamTotals.Max(t => t.Total);
                var winners = teamTotals.Where(t => Math.Abs(t.Total - maxTotal) < 1e-9).ToList();
                if (winners.Count != 1)
                    continue; // oavgjort, ingen får poäng
                var winner = winners[0];
                roundWins.TryAdd(winner.Id, 0);
                roundWins[winner.Id]++;
            }
            else
            {
                var playerTotals = rg
                    .GroupBy(s => s.PlayerId)
                    .Select(g => new { Id = g.Key, Total = g.Sum(s => s.Value) })
                    .ToList();

                if (!playerTotals.Any()) continue;
                var maxTotal = game.LowerIsBetter
                    ? playerTotals.Min(p => p.Total)
                    : playerTotals.Max(p => p.Total);
                var winners = playerTotals.Where(p => Math.Abs(p.Total - maxTotal) < 1e-9).ToList();
                if (winners.Count != 1)
                    continue; // oavgjort, ingen får poäng
                var winner = winners[0];
                roundWins.TryAdd(winner.Id, 0);
                roundWins[winner.Id]++;
            }
        }

        return roundWins;
    }

    /// <summary>
    /// Startar spelet (status → <c>Active</c>, <c>CurrentRound</c> = 1)
    /// och notifierar gruppen.
    /// </summary>
    public async Task<Game?> StartGame(Guid id)
    {
        var game = await db.Games
            .Include(g => g.GamePlayers)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (game == null)
            return null;

        game.Status = GameStatus.Active;
        game.CurrentRound = 1;
        await db.SaveChangesAsync();
        await hub.Clients.Group(id.ToString())
            .SendAsync("GameUpdated");
        return game;
    }

    /// <summary>
    /// Pausar ett aktivt spel (status → <c>Waiting</c>) och notifierar gruppen.
    /// Returnerar <c>null</c> om spelet inte är aktivt.
    /// </summary>
    public async Task<Game?> PauseGame(Guid id)
    {
        var game = await db.Games.FindAsync(id);
        if (game == null || game.Status != GameStatus.Active)
            return null;

        game.Status = GameStatus.Waiting;
        await db.SaveChangesAsync();
        await hub.Clients.Group(id.ToString())
            .SendAsync("GameUpdated");
        return game;
    }

    /// <summary>
    /// Ökar <c>CurrentRound</c> med ett steg om spelet är aktivt och inte redan på sista rundan.
    /// Kontrollerar därefter om "Bäst av X"-villkoret är uppfyllt.
    /// </summary>
    public async Task<Game?> AdvanceRound(Guid id)
    {
        var game = await db.Games
            .Include(g => g.Scores)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (game is not { Status: GameStatus.Active })
            return null;

        // Rundbaserade lägen: tillåt alltid fler rundor tills en vinnare finns
        bool isRoundBased = game.GameMode == "BestOf" ||
                            (game.GameMode == "FirstTo" && game.GameModeTarget == "rounds");

        if (isRoundBased && game.GameModeValue.HasValue)
        {
            var roundsToWin = game.GameMode == "BestOf"
                ? (game.GameModeValue.Value / 2) + 1
                : game.GameModeValue.Value;
            var roundWins = CountRoundWins(game);
            if (roundWins.Values.Any(w => w >= roundsToWin))
                return game; // vinnare finns redan, inga fler rundor
            // Annars tillåt alltid att gå vidare (dynamiska extrarundor vid oavgjort)
        }
        else if (game.MaxRounds.HasValue && game.CurrentRound >= game.MaxRounds.Value)
        {
            return game; // standard max-rundor
        }

        game.CurrentRound++;
        await db.SaveChangesAsync();
        await hub.Clients.Group(id.ToString())
            .SendAsync("GameUpdated");

        // Kolla rundbaserade vinstvillkor efter varje avslutad runda
        await CheckRoundBasedWin(id);

        return game;
    }

    /// <summary>Tilldelar (eller avlägsnar) en spelare från ett lag inom ett spel.</summary>
    public async Task<GamePlayer?> AssignPlayerToTeam(Guid gameId, Guid playerId, Guid? teamId)
    {
        var gp = await db.GamePlayers
            .FirstOrDefaultAsync(x => x.GameId == gameId && x.PlayerId == playerId);
        if (gp is null)
            return null;

        gp.TeamId = teamId;
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString())
            .SendAsync("GameUpdated");
        return gp;
    }

    /// <summary>
    /// Försöker claima en spelplats åt en SignalR-anslutning.
    /// Returnerar <c>null</c> om platsen redan är claimad av en annan anslutning.
    /// </summary>
    public async Task<GamePlayer?> ClaimPlayer(Guid gameId, Guid playerId, string connectionId)
    {
        var gp = await db.GamePlayers
            .FirstOrDefaultAsync(x => x.GameId == gameId && x.PlayerId == playerId);
        if (gp is null)
            return null;
        if (gp.ClaimedByConnectionId != null && gp.ClaimedByConnectionId != connectionId)
            return null; // redan claimad av någon annan

        gp.ClaimedByConnectionId = connectionId;
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString())
            .SendAsync("GameUpdated");
        return gp;
    }

    /// <summary>
    /// Friger en claimad spelplats om <paramref name="connectionId"/> matchar det lagrade värdet.
    /// </summary>
    public async Task<GamePlayer?> UnclaimPlayer(Guid gameId, Guid playerId, string connectionId)
    {
        var gp = await db.GamePlayers
            .FirstOrDefaultAsync(x => x.GameId == gameId && x.PlayerId == playerId);
        if (gp is null)
            return null;
        if (gp.ClaimedByConnectionId != connectionId)
            return null;

        gp.ClaimedByConnectionId = null;
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString())
            .SendAsync("GameUpdated");
        return gp;
    }

    /// <summary>
    /// Friger alla spelplatser som är claimade av den givna SignalR-anslutningen.
    /// Anropas automatiskt vid <c>OnDisconnected</c>.
    /// </summary>
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

        if (claimed.Count != 0)
            await db.SaveChangesAsync();
    }

    /// <summary>
    /// Kontrollerar "Först till X poäng" – avsluta spelet om en spelare/ett lag nått målet.
    /// Körs vid varje poängändring. Rundbaserad FirstTo hanteras i CheckRoundBasedWin.
    /// </summary>
    private async Task CheckFirstToWin(Guid gameId)
    {
        var game = await db.Games
            .Include(g => g.Scores)
            .FirstOrDefaultAsync(g => g.Id == gameId);
        if (game is not { Status: GameStatus.Active })
            return;
        if (game.GameMode != "FirstTo" || !game.GameModeValue.HasValue)
            return;
        // Rundbaserad FirstTo avgörs vid rundavslut, inte per poäng
        if (game.GameModeTarget == "rounds")
            return;

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
    /// Kontrollerar rundbaserade vinst-villkor vid rundavslut:
    /// "Bäst av X" (roundsToWin = X/2+1) och "Först till X rundor" (roundsToWin = X).
    /// Oavgjorda rundor räknas inte – spelet fortsätter tills en vinnare finns.
    /// </summary>
    private async Task CheckRoundBasedWin(Guid gameId)
    {
        var game = await db.Games
            .Include(g => g.Scores)
            .Include(g => g.Teams)
            .FirstOrDefaultAsync(g => g.Id == gameId);
        if (game is not { Status: GameStatus.Active })
            return;
        if (!game.GameModeValue.HasValue)
            return;

        int roundsToWin;
        if (game.GameMode == "BestOf")
            roundsToWin = (game.GameModeValue.Value / 2) + 1;
        else if (game.GameMode == "FirstTo" && game.GameModeTarget == "rounds")
            roundsToWin = game.GameModeValue.Value;
        else
            return;

        var roundWins = CountRoundWins(game);
        if (roundWins.Values.Any(w => w >= roundsToWin))
            await FinishGame(gameId);
    }

    public async Task<int> RemoveExpiredGames()
    {
        var now = DateTime.UtcNow;
        var expired = await db.Games
            .Where(g => g.ExpiresAt != null && g.ExpiresAt <= now)
            .ToListAsync();

        db.Games.RemoveRange(expired);
        await db.SaveChangesAsync();
        return expired.Count;
    }

}
