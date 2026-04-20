using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using server.Hubs;
using server.Models;

namespace server.Service;

/// <summary>
/// Hanterar spelare, lag och claim-logik inom ett spel.
/// </summary>
public class GamePlayerService(AppDbContext db, IHubContext<GameHub> hub)
{
    /// <summary>Lägger till ett lag i ett spel.</summary>
    public async Task<Team> AddTeam(Team team)
    {
        db.Teams.Add(team);
        await db.SaveChangesAsync();
        await hub.Clients.Group(team.GameId.ToString()).SendAsync("GameUpdated");
        return team;
    }

    /// <summary>Lägger till en spelare i ett spel om spelaren inte redan är med.</summary>
    public async Task<GamePlayer?> AddPlayerToGame(GamePlayer gp)
    {
        var exists = await db.GamePlayers
            .AnyAsync(x => x.GameId == gp.GameId && x.PlayerId == gp.PlayerId);
        if (exists)
            return null;

        db.GamePlayers.Add(gp);
        await db.SaveChangesAsync();
        await hub.Clients.Group(gp.GameId.ToString()).SendAsync("GameUpdated");
        return gp;
    }

    /// <summary>Tilldelar (eller avlägsnar) en spelare från ett lag.</summary>
    public async Task<GamePlayer?> AssignPlayerToTeam(Guid gameId, Guid playerId, Guid? teamId)
    {
        var gp = await db.GamePlayers
            .FirstOrDefaultAsync(x => x.GameId == gameId && x.PlayerId == playerId);
        if (gp is null)
            return null;

        gp.TeamId = teamId;
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString()).SendAsync("GameUpdated");
        return gp;
    }

    /// <summary>Byter namn på en spelare. Kräver Waiting-status.</summary>
    public async Task<bool> RenamePlayer(Guid gameId, Guid playerId, string newName)
    {
        var game = await db.Games.FindAsync(gameId);
        if (game is null || game.Status != GameStatus.Waiting)
            return false;

        var player = await db.Players.FindAsync(playerId);
        if (player is null)
            return false;

        player.UserName = newName.Trim();
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString()).SendAsync("GameUpdated");
        return true;
    }

    /// <summary>Tar bort en spelare från ett spel. Kräver Waiting-status.</summary>
    public async Task<bool> RemovePlayerFromGame(Guid gameId, Guid playerId)
    {
        var game = await db.Games.FindAsync(gameId);
        if (game is null || game.Status != GameStatus.Waiting)
            return false;

        var gp = await db.GamePlayers
            .FirstOrDefaultAsync(x => x.GameId == gameId && x.PlayerId == playerId);
        if (gp is null)
            return false;

        db.GamePlayers.Remove(gp);
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString()).SendAsync("GameUpdated");
        return true;
    }

    /// <summary>Byter namn på ett lag. Kräver Waiting-status.</summary>
    public async Task<bool> RenameTeam(Guid gameId, Guid teamId, string newName)
    {
        var game = await db.Games.FindAsync(gameId);
        if (game is null || game.Status != GameStatus.Waiting)
            return false;

        var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.GameId == gameId);
        if (team is null)
            return false;

        team.Name = newName.Trim();
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString()).SendAsync("GameUpdated");
        return true;
    }

    /// <summary>Tar bort ett lag och friställer alla spelare som tillhörde det.</summary>
    public async Task<bool> RemoveTeam(Guid gameId, Guid teamId)
    {
        var game = await db.Games.FindAsync(gameId);
        if (game is null || game.Status != GameStatus.Waiting)
            return false;

        var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.GameId == gameId);
        if (team is null)
            return false;

        var members = await db.GamePlayers
            .Where(gp => gp.GameId == gameId && gp.TeamId == teamId)
            .ToListAsync();
        foreach (var gp in members)
            gp.TeamId = null;

        db.Teams.Remove(team);
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString()).SendAsync("GameUpdated");
        return true;
    }

    // ─── Claim-hantering ───

    /// <summary>Försöker claima en spelplats åt en SignalR-anslutning.</summary>
    public async Task<GamePlayer?> ClaimPlayer(Guid gameId, Guid playerId, string connectionId)
    {
        var gp = await db.GamePlayers
            .FirstOrDefaultAsync(x => x.GameId == gameId && x.PlayerId == playerId);
        if (gp is null)
            return null;
        if (gp.ClaimedByConnectionId != null && gp.ClaimedByConnectionId != connectionId)
            return null;

        gp.ClaimedByConnectionId = connectionId;
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString()).SendAsync("GameUpdated");
        return gp;
    }

    /// <summary>Friger en claimad spelplats.</summary>
    public async Task<GamePlayer?> UnclaimPlayer(Guid gameId, Guid playerId, string connectionId)
    {
        var gp = await db.GamePlayers
            .FirstOrDefaultAsync(x => x.GameId == gameId && x.PlayerId == playerId);
        if (gp is null) return null;
        if (gp.ClaimedByConnectionId != connectionId)
            return null;

        gp.ClaimedByConnectionId = null;
        await db.SaveChangesAsync();
        await hub.Clients.Group(gameId.ToString()).SendAsync("GameUpdated");
        return gp;
    }

    /// <summary>Friger alla claims för en given anslutning (vid disconnect).</summary>
    public async Task UnclaimByConnection(string connectionId)
    {
        var claimed = await db.GamePlayers
            .Where(gp => gp.ClaimedByConnectionId == connectionId)
            .ToListAsync();

        foreach (var gp in claimed)
        {
            gp.ClaimedByConnectionId = null;
            await hub.Clients.Group(gp.GameId.ToString()).SendAsync("GameUpdated");
        }

        if (claimed.Count != 0)
            await db.SaveChangesAsync();
    }
}

