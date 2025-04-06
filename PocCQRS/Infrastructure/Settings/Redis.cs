namespace PocCQRS.Infrastructure.Settings;

public class Redis
{
    public const string SectionName = "Cache";
    public int ExpirationMinutes { get; set; } = 15;
    public string RedisConnection { get; set; } = default!;
}