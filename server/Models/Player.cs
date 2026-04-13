public class Player
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = "";
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public List<Score>? Scores { get; set; }
}
