using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace server.Hubs
{
    public class GameHub(AppDbContext db) : Hub
    {
        private readonly AppDbContext _db = db;

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

            if (!Guid.TryParse(playerIdStr, out var playerId))
            {
                await Clients.Caller.SendAsync("Error", "Invalid playerId.");
                Context.Abort();
                return;
            }

            // Validera att spelaren faktiskt är med i den specifika matchen
            var gamePlayer = await _db.GamePlayers
                .Include(gp => gp.Player)
                .FirstOrDefaultAsync(gp => gp.GameId == gameId && gp.PlayerId == playerId);

            if (gamePlayer is null)
            {
                await Clients.Caller.SendAsync("Error", "Player not found in this game.");
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, gameIdStr);

            await Clients.Caller.SendAsync("ClaimAccepted", new
            {
                GameId = gameId,
                PlayerId = playerId,
                PlayerName = gamePlayer.Player.UserName,
                Role = "player"
            });

            await base.OnConnectedAsync();
        }

        public Task LeaveGame(string gameId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
    }
}
