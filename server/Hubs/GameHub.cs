using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using server.Service;

namespace server.Hubs
{
    public class GameHub(AppDbContext db, GameServices gameSvc) : Hub
    {
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
            var playerIdStr = http.Request.Query["playerId"].ToString();

            if (!Guid.TryParse(gameIdStr, out var gameId))
            {
                await Clients.Caller.SendAsync("Error", "Missing or invalid gameId.");
                Context.Abort();
                return;
            }

            // Åskådare – joina gruppen utan claim
            if (string.IsNullOrEmpty(playerIdStr))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, gameIdStr);
                await Clients.Caller.SendAsync("ClaimAccepted", new
                {
                    GameId = gameId,
                    PlayerId = (Guid?)null,
                    PlayerName = (string?)null,
                    Role = "spectator"
                });
                await base.OnConnectedAsync();
                return;
            }

            if (!Guid.TryParse(playerIdStr, out var playerId))
            {
                await Clients.Caller.SendAsync("Error", "Invalid playerId.");
                Context.Abort();
                return;
            }

            // Validera att spelaren finns i matchen
            var gamePlayer = await db.GamePlayers
                .Include(gp => gp.Player)
                .FirstOrDefaultAsync(gp => gp.GameId == gameId && gp.PlayerId == playerId);

            if (gamePlayer is null)
            {
                await Clients.Caller.SendAsync("Error", "Player not found in this game.");
                Context.Abort();
                return;
            }

            // Försök claima spelaren
            var claimed = await gameSvc.ClaimPlayer(gameId, playerId, Context.ConnectionId);
            if (claimed is null)
            {
                await Clients.Caller.SendAsync("ClaimRejected", "Player already claimed by another user.");
                // Fortfarande joina gruppen som åskådare
                await Groups.AddToGroupAsync(Context.ConnectionId, gameIdStr);
                await base.OnConnectedAsync();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, gameIdStr);

            // Kolla om denna spelare är creator
            var game = await db.Games.FindAsync(gameId);
            var isCreator = false;
            var creatorSecretStr = http.Request.Query["creatorSecret"].ToString();
            if (Guid.TryParse(creatorSecretStr, out var creatorSecret) && game?.CreatorSecret == creatorSecret)
                isCreator = true;

            await Clients.Caller.SendAsync("ClaimAccepted", new
            {
                GameId = gameId,
                PlayerId = playerId,
                PlayerName = gamePlayer.Player.UserName,
                Role = isCreator ? "creator" : "player"
            });

            await base.OnConnectedAsync();
        }

        public async Task UnclaimPlayer(string gameIdStr, string playerIdStr)
        {
            if (!Guid.TryParse(gameIdStr, out var gameId) || !Guid.TryParse(playerIdStr, out var playerId))
                return;

            await gameSvc.UnclaimPlayer(gameId, playerId, Context.ConnectionId);
        }

        public Task LeaveGame(string gameId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Unclaima alla spelare som denna anslutning hade
            await gameSvc.UnclaimByConnection(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
