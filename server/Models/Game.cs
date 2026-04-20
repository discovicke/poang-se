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
    public double StartingScore { get; set; } = 0;
    public Guid? WinnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsTemporary { get; set; } = false;

    /// <summary>Hemlig nyckel som identifierar spelskaparen. Returneras bara vid skapande.</summary>
    public Guid CreatorSecret { get; set; } = Guid.NewGuid();

    /// <summary>Bara skaparen kan redigera poäng under Active.</summary>
    public bool CreatorOnly { get; set; } = false;

    /// <summary>Hur mycket +/−-knapparna ändrar per klick.</summary>
    public double ScoreIncrement { get; set; } = 1;

    /// <summary>Om true bestäms vinnaren av lagpoäng istället för individuella.</summary>
    public bool TeamBasedWinner { get; set; } = false;

    /// <summary>Vilken runda som är aktiv (1-baserad).</summary>
    public int CurrentRound { get; set; } = 1;

    /// <summary>Null = standard, "BestOf" eller "FirstTo".</summary>
    public string? GameMode { get; set; }

    /// <summary>X-värdet för "Bäst av X" eller "Först till X".</summary>
    public int? GameModeValue { get; set; }

    /// <summary>
    /// Gäller bara "FirstTo": "rounds" = först till X rundvinster, "points" = först till X poäng.
    /// För "BestOf" är målet alltid rundor.
    /// </summary>
    public string? GameModeTarget { get; set; }

    /// <summary>Om true krävs lösenord för att se spelet.</summary>
    public bool IsPrivate { get; set; } = false;

    /// <summary>SHA-256-hash av lösenordet. Null om spelet är öppet.</summary>
    public string? PasswordHash { get; set; }

    public List<GamePlayer> GamePlayers { get; set; } = new();
    public List<Team> Teams { get; set; } = new();
    public List<Score> Scores { get; set; } = new();
}
