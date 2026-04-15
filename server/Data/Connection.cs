using Npgsql;
using DotNetEnv;

/// <summary>
/// Läser databasanslutningsinställningar från miljövariabler och bygger en Npgsql-connection string.
/// Förväntar sig: <c>DB_HOST</c>, <c>DB_PORT</c>, <c>DB_NAME</c>, <c>DB_USER</c>, <c>DB_PASSWORD</c>.
/// </summary>
public class Connection
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Database { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    /// <summary>
    /// Läser samtliga inställningar från miljövariabler via DotNetEnv och loggar host/port/db/user till konsolen.
    /// </summary>
    public Connection()
    {
        Host = Environment.GetEnvironmentVariable("DB_HOST") ?? "";
        Port = int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var p) ? p : 5432;
        Database = Environment.GetEnvironmentVariable("DB_NAME") ?? "";
        Username = Environment.GetEnvironmentVariable("DB_USER") ?? "";
        Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
        Console.WriteLine($"Loaded connection settings: Host={Host}, Port={Port}, Database={Database}, Username={Username}");
    }

    /// <summary>
    /// Returnerar en färdig Npgsql-connection string baserad på de inlästa inställningarna.
    /// </summary>
    public override string ToString()
    {
        return new NpgsqlConnectionStringBuilder
        {
            Host = Host,
            Port = Port,
            Database = Database,
            Username = Username,
            Password = Password
        }.ToString();
    }
}
