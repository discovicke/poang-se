using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using server.Models;

/// <summary>
/// EF Core-databaskontext för applikationen.
/// Exponerar DbSet:ar för samtliga entiteter och konfigurerar relationer i <see cref="OnModelCreating"/>.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>Alla spel.</summary>
    public DbSet<Game> Games => Set<Game>();
    /// <summary>Alla registrerade spelare.</summary>
    public DbSet<Player> Players => Set<Player>();
    /// <summary>Kopplingstabell mellan spel och spelare.</summary>
    public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();
    /// <summary>Lag som tillhör spel.</summary>
    public DbSet<Team> Teams => Set<Team>();
    /// <summary>Alla poängregistreringar.</summary>
    public DbSet<Score> Scores => Set<Score>();

    /// <summary>
    /// Konfigurerar relationer, composite keys och delete-beteenden för <see cref="GamePlayer"/>,
    /// <see cref="Team"/> och <see cref="Score"/>.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder mb)
    {
        // GamePlayer är en join-tabell med composite key
        mb.Entity<GamePlayer>(e =>
        {
            e.HasKey(gp => new { gp.GameId, gp.PlayerId });

            e.HasOne(gp => gp.Game)
             .WithMany(g => g.GamePlayers)
             .HasForeignKey(gp => gp.GameId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(gp => gp.Player)
             .WithMany(p => p.GamePlayers)
             .HasForeignKey(gp => gp.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(gp => gp.Team)
             .WithMany(t => t.GamePlayers)
             .HasForeignKey(gp => gp.TeamId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Team tillhör ett Game
        mb.Entity<Team>(e =>
        {
            e.HasKey(t => t.Id);

            e.HasOne(t => t.Game)
             .WithMany(g => g.Teams)
             .HasForeignKey(t => t.GameId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Score kopplar Game + Player + (valfritt) Team
        mb.Entity<Score>(e =>
        {
            e.HasKey(s => s.Id);

            e.HasOne(s => s.Game)
             .WithMany(g => g.Scores)
             .HasForeignKey(s => s.GameId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.Player)
             .WithMany(p => p.Scores)
             .HasForeignKey(s => s.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.Team)
             .WithMany(t => t.Scores)
             .HasForeignKey(s => s.TeamId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
