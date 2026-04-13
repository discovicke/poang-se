using server.Dtos;

namespace server.Service;

public class PlayerServices(AppDbContext db)
{
    public async Task CreatePlayer(Player player)
    {
        db.Players.Add(player);
        await db.SaveChangesAsync();
    }
}
