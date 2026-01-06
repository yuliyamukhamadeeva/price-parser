using Microsoft.EntityFrameworkCore;
using PriceParser.Web.Data;
using PriceParser.Web.Models;

namespace PriceParser.Web.Services;

public class PriceParserService
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly GenericPriceExtractor _extractor;
    private readonly ILogger<PriceParserService> _logger;

    public PriceParserService(
        AppDbContext db,
        IHttpClientFactory httpFactory,
        GenericPriceExtractor extractor,
        ILogger<PriceParserService> logger)
    {
        _db = db;
        _httpFactory = httpFactory;
        _extractor = extractor;
        _logger = logger;
    }

    public async Task<int> RunAsync(CancellationToken ct)
    {
        var http = _httpFactory.CreateClient();

        var links = await _db.ProductLinks
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Product)
            .Include(x => x.Shop)
            .ToListAsync(ct);

        var saved = 0;

        foreach (var link in links)
        {
            if (string.IsNullOrWhiteSpace(link.Url)) continue;
            

            decimal? price = null;
            

            try
            {
                price = await _extractor.TryExtractAsync(link.Url, http, ct);
                
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка парсинга {Url}", link.Url);
            }

            if (!price.HasValue)
            {
                _logger.LogInformation("Цена не найдена: {Shop} {Url}", link.Shop.Name, link.Url);
                continue;
            }

            var log = new PriceLog
            {
                ProductId = link.ProductId,
                ShopId = link.ShopId,
                PriceKopeks = ToKopeks(price.Value),
                ParsedAt = DateTime.UtcNow
            };

_logger.LogInformation("Цена {Price} коп. | {Shop} | {Url} | {Time}",
    log.PriceKopeks, link.Shop.Name, link.Url, log.ParsedAt);
            _db.PriceLogs.Add(log);
            saved++;
            
        }

        if (saved > 0)
            await _db.SaveChangesAsync(ct);

        return saved;
    }
    private static long ToKopeks(decimal rub)
{
    return (long)Math.Round(rub * 100m, 0, MidpointRounding.AwayFromZero);
}

}
