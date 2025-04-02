namespace PocCQRS.Infrastructure.Settings;

public sealed class AppSettings : IAppSettings, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppSettings> _logger;
    private IDisposable _changeToken;

    public RabbitMQ RabbitMQSettings { get; private set; }
    public MySqlDB DatabaseSettings { get; private set; }
    public Reddis CacheSettings { get; private set; }

    public AppSettings(
        IConfiguration configuration,
        ILogger<AppSettings> logger)
    {
        _configuration = configuration;
        _logger = logger;
        LoadAllSettings();
        SetupChangeTracking();
    }

    private void LoadAllSettings()
    {
        
        try
        {
            RabbitMQSettings = _configuration.GetSection(RabbitMQ.SectionName).Get<RabbitMQ>();
            DatabaseSettings = _configuration.GetSection(MySqlDB.SectionName).Get<MySqlDB>();
            CacheSettings = _configuration.GetSection(Reddis.SectionName).Get<Reddis>();
            
            _logger.LogInformation("Todas as configurações foram carregadas");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Falha ao carregar configurações");
            throw;
        }
    }

    private void SetupChangeTracking()
    {
        _changeToken = _configuration.GetReloadToken().RegisterChangeCallback(_ =>
        {
            _logger.LogInformation("Configurações alteradas - recarregando...");
            LoadAllSettings();
        }, null);
    }

    public Task ReloadAsync()
    {
        LoadAllSettings();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _changeToken?.Dispose();
    }
}