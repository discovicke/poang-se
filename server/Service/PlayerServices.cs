using Microsoft.EntityFrameworkCore;
using server.Dtos;

namespace server.Service;

/// <summary>
/// Affärslogik för spelare. Hanterar skapande och hämtning av <see cref="Player"/>-entiteter.
/// </summary>
public class PlayerServices(AppDbContext db)
{
    /// <summary>Sparar en ny spelare i databasen.</summary>
    public async Task CreatePlayer(Player player)
    {
        db.Players.Add(player);
        await db.SaveChangesAsync();
    }
    /// <summary>Returnerar alla spelare utan någon specifik sortering.</summary>
    public async Task <List<Player>> GetAllPlayers()
    {
        return await db.Players.ToListAsync();
    }
}
