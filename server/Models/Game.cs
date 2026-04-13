namespace server.Models;
/// <summary>
/// Representerar ett spel/match. Huvudentitet som äger Player, Team och Scores.
/// </summary>
public class Game
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public GameStatus Status { get; set; } = GameStatus.Waiting;
    public bool LowerIsBetter { get; set; } = false;
    public int? MaxRounds { get; set; } = 1;
    public Guid? WinnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }

    public List<GamePlayer> GamePlayers { get; set; } = new();
    public List<Team> Teams { get; set; } = new();
    public List<Score> Scores { get; set; } = new();
}
