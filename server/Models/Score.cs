using server.Models;

/// <summary>
/// Representerar en poängregistrering för en spelare (och valfritt lag) i ett spel.
/// </summary>
public class Score
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid? TeamId { get; set; }
    public int? Round { get; set; }
    /// <summary>
    /// Poängen för denna specifika runda eller registrering.
    /// </summary>
    public double Value { get; set; }
    /// <summary>
    /// Spelarens ackumulerade totalsumma i spelet fram till och med denna registrering.
    /// Beräknas i backend vid varje ny Score och sparas här för snabb avläsning utan omberäkning.
    /// Exempel: Value = 3, tidigare rundor = 7, CumulativeValue = 10.
    /// </summary>
    public double? CumulativeValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Game Game { get; set; } = null!;
    public Player Player { get; set; } = null!;
    public Team? Team { get; set; }
}
