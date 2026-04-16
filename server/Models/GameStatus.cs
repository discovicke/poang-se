namespace server.Models;

/// <summary>
/// Livscykeltillstånd för ett spel.
/// </summary>
public enum GameStatus
{
    /// <summary>Lobby – spelet har skapats men inte startats ännu.</summary>
    Waiting,
    /// <summary>Pågående – poäng kan registreras och rundor kan avanceras.</summary>
    Active,
    /// <summary>Avslutat – ingen ytterligare redigering tillåts.</summary>
    Finished
}
