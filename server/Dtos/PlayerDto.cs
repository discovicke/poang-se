namespace server.Dtos;

public class PlayerDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string UserName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
}
