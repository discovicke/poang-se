using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Score> Scores { get; set; }
}