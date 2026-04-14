using Npgsql;
using DotNetEnv;

public class Connection
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Database { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    /// <summary>
    /// Laddar databasanslutningsinställningar från miljövariabler. Används med .env från rootnivå. Förväntar sig att följande variabler är satta:
    /// <ul>
    /// <li><c>DB_HOST</c>: Databasens host-adress (t.ex. "localhost" eller en IP-adress).</li>
    /// <li><c>DB_PORT</c>: Portnumret som databasen lyssnar på (standard är 5432 för PostgreSQL).</li>
    /// <li><c>DB_NAME</c>: Namnet på databasen att ansluta till.</li>
    /// <li><c>DB_USER</c>: Användarnamnet för databasautentisering.</li>
    /// <li><c>DB_PASSWORD</c>: Lösenord för användaren.</li>
    /// </ul>
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
