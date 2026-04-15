using server.Models;

namespace server.Mappers;

/// <summary>
/// Extension-metoder för att projicera domänmodeller till anonyma API-responsobjekt.
/// Centraliserar all responsmappning och håller endpoints fria från projektionslogik.
/// </summary>
public static class GameMapper
{
    /// <summary>
    /// Fullständig speldetalj med lag, spelare och poänghistorik.
    /// Används av <c>GET /api/games/{id}</c>.
    /// </summary>
    public static object ToDetailResponse(this Game game) => new
    {
        game.Id,
        game.Name,
        Status = game.Status.ToString(),
        game.LowerIsBetter,
        game.MaxRounds,
        game.WinnerId,
        game.CreatedAt,
        game.FinishedAt,
        game.CreatorOnly,
        game.ScoreIncrement,
        game.TeamBasedWinner,
        game.CurrentRound,
        game.GameMode,
        game.GameModeValue,
        game.StartingScore,
        Teams = game.Teams.Select(t => new { t.Id, t.Name }),
        Players = game.GamePlayers.Select(gp => new
        {
            gp.PlayerId,
            PlayerName = gp.Player.UserName,
            gp.TeamId,
            TeamName = gp.Team?.Name,
            gp.JoinedAt,
            gp.ClaimedByConnectionId
        }),
        Scores = game.Scores
            .OrderBy(s => s.Round).ThenBy(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.PlayerId,
                PlayerName = s.Player.UserName,
                s.TeamId,
                s.Round,
                s.Value,
                s.CumulativeValue,
                s.CreatedAt
            })
    };

    /// <summary>
    /// Minimalt svar vid skapande av spel. Inkluderar <c>CreatorSecret</c>
    /// som <em>enbart</em> returneras vid detta tillfälle.
    /// </summary>
    public static object ToCreatedResponse(this Game game) => new
    {
        game.Id,
        game.Name,
        Status = game.Status.ToString(),
        game.LowerIsBetter,
        game.MaxRounds,
        game.CreatedAt,
        game.CreatorSecret
    };

    /// <summary>
    /// Id + statusträng. Används av start, pause och settings-endpoints.
    /// </summary>
    public static object ToStatusResponse(this Game game) => new
    {
        game.Id,
        Status = game.Status.ToString()
    };

    /// <summary>
    /// Avslutat spels utfall: id, status, vinnare och tidsstämpel.
    /// </summary>
    public static object ToFinishResponse(this Game game) => new
    {
        game.Id,
        Status = game.Status.ToString(),
        game.WinnerId,
        game.FinishedAt,
        game.TeamBasedWinner
    };

    /// <summary>
    /// Poängregistrerings-svar med id, spelare, runda, värde och kumulativt värde.
    /// </summary>
    public static object ToScoreResponse(this Score score) => new
    {
        score.Id,
        score.PlayerId,
        score.Round,
        score.Value,
        score.CumulativeValue
    };
}
