namespace PocCQRS.Infrastructure.Settings;

public class MySqlDB
{
    public const string SectionName = "Database";
    public string ConnectionString { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}