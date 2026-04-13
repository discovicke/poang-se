using Microsoft.EntityFrameworkCore;
using server.Dtos;

namespace server.Service;

public class PlayerServices(AppDbContext db)
{
    public async Task CreatePlayer(Player player)
    {
        db.Players.Add(player);
        await db.SaveChangesAsync();
    }
    public async Task <List<Player>> GetAllPlayers()
    {
        return await db.Players.ToListAsync();
    }
}
