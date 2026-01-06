using Microsoft.Extensions.Options;

namespace PriceParser.Web.Services;

public class ParsingOptions
{
    public bool Enabled { get; set; } = false;
    public int IntervalMinutes { get; set; } = 60;
}

public class ParsingHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<ParsingOptions> _options;
    private readonly ILogger<ParsingHostedService> _logger;

    public ParsingHostedService(IServiceScopeFactory scopeFactory, IOptions<ParsingOptions> options, ILogger<ParsingHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Value.Enabled) return;

        var minutes = Math.Max(1, _options.Value.IntervalMinutes);
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnce(stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task RunOnce(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PriceParserService>();

        try
        {
            var saved = await service.RunAsync(ct);
            _logger.LogInformation("Парсинг завершён, записей: {Saved}", saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка фонового парсинга");
        }
    }
}
