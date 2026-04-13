using Npgsql;
using DotNetEnv;

public class Connection
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Database { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

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