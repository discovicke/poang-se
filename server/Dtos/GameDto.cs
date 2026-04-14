namespace server.Dtos;

public class CreateGameDto
{
    public string Name { get; set; } = "";
    public bool LowerIsBetter { get; set; } = false;
    public int? MaxRounds { get; set; } = 1;
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

