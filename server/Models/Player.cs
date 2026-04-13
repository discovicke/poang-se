using server.Models;

/// <summary>
/// Representerar en spelare som kan delta i ett eller flera spel.
/// </summary>
public class Player
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<GamePlayer> GamePlayers { get; set; } = new();
    public List<Score> Scores { get; set; } = new();
}
