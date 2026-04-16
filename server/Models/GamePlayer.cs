namespace server.Models;

/// <summary>
/// Join-tabell mellan Game och Player. Registrerar vilka spelare som deltar i ett specifikt spel.
/// </summary>
public class GamePlayer
{
    public Guid GameId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid? TeamId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>SignalR connection-id för den som claimat denna spelplats. Null = ej claimad.</summary>
    public string? ClaimedByConnectionId { get; set; }

    public Game Game { get; set; } = null!;
    public Player Player { get; set; } = null!;
    public Team? Team { get; set; }
}
