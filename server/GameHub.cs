using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace server.Hubs
{
    public class GameHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            if (httpContext is null)
            {
                // Skicka felmeddelande till den specifika klienten
                await Clients.Caller.SendAsync("Event", "No HTTP context available.");
                Context.Abort(); // Stäng anslutningen
                return;
            }

            await base.OnConnectedAsync();
        }
        public Task JoinGame(string gameId)
        => Groups.AddToGroupAsync(Context.ConnectionId, gameId);

        public Task LeaveGame(string gameId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);

    }


}