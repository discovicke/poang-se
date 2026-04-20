using Microsoft.Data.SqlClient;
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
        Port = int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var p) ? p : 1433;
        Database = Environment.GetEnvironmentVariable("DB_NAME") ?? "";
        Username = Environment.GetEnvironmentVariable("DB_USER") ?? "";
        Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
    }

    public override string ToString()
    {
        return new SqlConnectionStringBuilder
        {
            DataSource = Port != 1433 ? $"{Host},{Port}" : Host,
            InitialCatalog = Database,
            UserID = Username,
            Password = Password,
            TrustServerCertificate = true
        }.ToString();
    }
}
