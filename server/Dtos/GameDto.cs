namespace server.Dtos;

public class CreateGameDto
{
    public string Name { get; set; } = "";
    public bool LowerIsBetter { get; set; } = false;
    public int? MaxRounds { get; set; } = 1;
    public double StartingScore { get; set; } = 0;
    public bool IsPrivate { get; set; } = false;
    public string? GamePassword { get; set; }
}

public class UnlockGameDto
{
    public string Password { get; set; } = "";
}

public class AddTeamDto
{
    public string Name { get; set; } = "";
}

public class AddPlayerToGameDto
{
    public Guid PlayerId { get; set; }
    public Guid? TeamId { get; set; }
}

public class AddScoreToGameDto
{
    public Guid PlayerId { get; set; }
    public Guid? TeamId { get; set; }
    public int? Round { get; set; }
    public double Value { get; set; }
}

public class CreatePlayerForGameDto
{
    public string UserName { get; set; } = "";
    public Guid? TeamId { get; set; }
}

public class UpdateScoreValueDto
{
    public Guid PlayerId { get; set; }
    public int Round { get; set; }
    public double Value { get; set; }
}

