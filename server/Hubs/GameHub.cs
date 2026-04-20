using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using server.Service;

namespace server.Hubs;

/// <summary>
/// SignalR-hub för realtidskommunikation under ett spel.
/// Hanterar anslutning med claim-logik (spelare/åskådare/creator),
/// gruppbaserade notifieringar per match-id samt automatisk unclaim vid frånkoppling.
/// </summary>
public class GameHub(AppDbContext db, GamePlayerService playerSvc) : Hub
{
    /// <summary>
    /// Körs när en klient ansluter. Förväntar sig query-parametrarna
    /// <c>gameId</c> (obligatorisk), <c>playerId</c> (valfri) och <c>creatorSecret</c> (valfri).
    /// Dirigerar anslutningen till <see cref="JoinAsSpectator"/> eller <see cref="JoinAsPlayer"/>.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        if (http is null)
        {
            await Clients.Caller.SendAsync("Error", "No HTTP context.");
            Context.Abort();
            return;
        }

        var gameIdStr = http.Request.Query["gameId"].ToString();
        if (!Guid.TryParse(gameIdStr, out var gameId))
        {
            await Clients.Caller.SendAsync("Error", "Missing or invalid gameId.");
            Context.Abort();
            return;
        }

        var playerIdStr = http.Request.Query["playerId"].ToString();
        if (string.IsNullOrEmpty(playerIdStr))
        {
            // Explicit spectator-val -> skicka ClaimAccepted, annars vänta på att användaren väljer
            var explicitSpectator = http.Request.Query["spectator"].ToString() == "true";
            if (explicitSpectator)
                await JoinAsSpectator(gameId, gameIdStr);
            else
                await JoinAsPending(gameIdStr);
            await base.OnConnectedAsync();
            return;
        }

        if (!Guid.TryParse(playerIdStr, out var playerId))
        {
            await Clients.Caller.SendAsync("Error", "Invalid playerId.");
            Context.Abort();
            return;
        }

        await JoinAsPlayer(gameId, gameIdStr, playerId, http);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Friger en spelares claim. Anropas av klienten när användaren väljer att byta spelare.
    /// </summary>
    public async Task UnclaimPlayer(string gameIdStr, string playerIdStr)
    {
        if (Guid.TryParse(gameIdStr, out var gameId) && Guid.TryParse(playerIdStr, out var playerId))
            await playerSvc.UnclaimPlayer(gameId, playerId, Context.ConnectionId);
    }

    /// <summary>Tar bort klientens SignalR-anslutning från spelets grupp.</summary>
    public Task LeaveGame(string gameId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);

    /// <summary>
    /// Körs automatiskt vid frånkoppling. Friger alla claims kopplade till denna anslutning.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await playerSvc.UnclaimByConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Lägger till anslutningen i spelets grupp utan att tilldela en roll.
    /// Klienten förväntas visa claim-väljaren och sedan återansluta med ett val.
    /// </summary>
    private async Task JoinAsPending(string gameIdStr)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameIdStr);
        await Clients.Caller.SendAsync("ClaimPending");
    }

    /// <summary>
    /// Lägger till anslutningen i spelets SignalR-grupp och skickar <c>ClaimAccepted</c>
    /// med rollen "spectator".
    /// </summary>
    private async Task JoinAsSpectator(Guid gameId, string gameIdStr)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameIdStr);
        await Clients.Caller.SendAsync("ClaimAccepted", new
        {
            GameId = gameId,
            PlayerId = (Guid?)null,
            PlayerName = (string?)null,
            Role = "spectator"
        });
    }

    /// <summary>
    /// Validerar att spelaren finns i spelet, försöker claima platsen och skickar
    /// antingen <c>ClaimAccepted</c> (med korrekt roll) eller <c>ClaimRejected</c>.
    /// </summary>
    private async Task JoinAsPlayer(Guid gameId, string gameIdStr, Guid playerId, HttpContext http)
    {
        var gamePlayer = await db.GamePlayers
            .Include(gp => gp.Player)
            .FirstOrDefaultAsync(gp => gp.GameId == gameId && gp.PlayerId == playerId);

        if (gamePlayer is null)
        {
            await Clients.Caller.SendAsync("Error", "Player not found in this game.");
            Context.Abort();
            return;
        }

        var claimed = await playerSvc.ClaimPlayer(gameId, playerId, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, gameIdStr);

        if (claimed is null)
        {
            await Clients.Caller.SendAsync("ClaimRejected", "Player already claimed by another user.");
            return;
        }

        var role = await ResolveRole(gameId, http);
        await Clients.Caller.SendAsync("ClaimAccepted", new
        {
            GameId = gameId,
            PlayerId = playerId,
            PlayerName = gamePlayer.Player.UserName,
            Role = role
        });
    }

    /// <summary>
    /// Avgör om anslutningen är creator genom att jämföra <c>creatorSecret</c>-query-parametern
    /// mot spelets lagrade hemlighet.
    /// </summary>
    private async Task<string> ResolveRole(Guid gameId, HttpContext http)
    {
        var secretStr = http.Request.Query["creatorSecret"].ToString();
        if (!Guid.TryParse(secretStr, out var creatorSecret))
            return "player";

        var game = await db.Games.FindAsync(gameId);
        return game?.CreatorSecret == creatorSecret
            ? "creator"
            : "player";
    }
}
