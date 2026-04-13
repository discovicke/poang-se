public class Player
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Score>? Scores { get; set; }
}
