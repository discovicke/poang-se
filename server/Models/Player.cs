public class Player
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public List<Score>? Scores { get; set; }
}