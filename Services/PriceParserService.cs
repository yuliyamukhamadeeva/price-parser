using Microsoft.EntityFrameworkCore;
using PriceParser.Web.Data;
using PriceParser.Web.Models;
using System.Text.Json;

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


    public Task<int> RunAsync(CancellationToken ct)
        => RunAsync(ct, null);


    public Task<int> RunAsync(int productId, CancellationToken ct)
        => RunAsync(ct, productId);

   
    private async Task<int> RunAsync(CancellationToken ct, int? onlyProductId)
    {
        var http = _httpFactory.CreateClient();

        var q = _db.ProductLinks
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Product)
            .Include(x => x.Shop)
            .AsQueryable();

        if (onlyProductId.HasValue)
            q = q.Where(x => x.ProductId == onlyProductId.Value);

        var links = await q.ToListAsync(ct);

        var saved = 0;

        foreach (var link in links)
        {
            if (string.IsNullOrWhiteSpace(link.Url))
                continue;

            decimal? price = null;

            try
            {
            
                var selectors = Array.Empty<string>();

                if (!string.IsNullOrWhiteSpace(link.Shop.PriceSelectors))
                {
                    try
                    {
                        selectors = JsonSerializer.Deserialize<string[]>(link.Shop.PriceSelectors) ?? Array.Empty<string>();
                    }
                    catch { }
                }

                _logger.LogInformation("Селекторы: {Len} | {Shop} | {Url}", selectors.Length, link.Shop.Name, link.Url);

     
                if (selectors.Length > 0)
                    price = await _extractor.TryExtractBySelectorsAsync(link.Url, selectors, http, ct);

              
                price ??= await _extractor.TryExtractAsync(link.Url, http, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка парсинга {Url}", link.Url);
            }

            if (!price.HasValue || price.Value <= 0)
            {
                _logger.LogInformation("Цена не найдена: {Shop} {Url}", link.Shop.Name, link.Url);
                continue;
            }

            var log = new PriceLog
            {
                ProductId = link.ProductId,
                ShopId = link.ShopId,
                Url = link.Url, 
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
        => (long)Math.Round(rub * 100m, 0, MidpointRounding.AwayFromZero);
}
