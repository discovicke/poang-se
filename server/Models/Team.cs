namespace server.Models;

/// <summary>
/// Representerar ett lag inom ett specifikt spel. Valfritt, används för lagbaserade sporter.
/// </summary>
public class Team
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";

    public Game Game { get; set; } = null!;
    public List<GamePlayer> GamePlayers { get; set; } = new();
    public List<Score> Scores { get; set; } = new();
}
