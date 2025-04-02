namespace PocCQRS.Infrastructure.Settings;

public class Reddis
{
    public const string SectionName = "Cache";
    public int ExpirationMinutes { get; set; } = 15;
    public string RedisConnection { get; set; }
}