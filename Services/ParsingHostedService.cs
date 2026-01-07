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
    private readonly IOptionsMonitor<ParsingOptions> _options;
    private readonly ILogger<ParsingHostedService> _logger;

    public ParsingHostedService(IServiceScopeFactory scopeFactory, IOptionsMonitor<ParsingOptions> options, ILogger<ParsingHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var opt = _options.CurrentValue;
            var minutes = Math.Max(1, opt.IntervalMinutes);

            if (opt.Enabled)
                await RunOnce(stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(minutes), stoppingToken);
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
