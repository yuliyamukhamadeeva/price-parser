using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PriceParser.Web.Data;

namespace PriceParser.Web.Pages.Products;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<Row> Items { get; set; } = new();

    public record Row(
        int Id,
        string Name,
        string? Sku,
        int LinksCount,
        long? MinPriceKopeks,
        string? MinShopName
    );

    public async Task OnGetAsync()
    {
        var q = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(Query))
            q = q.Where(p => p.Name.Contains(Query));

        Items = await q
            .OrderByDescending(p => p.Id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Sku,
                LinksCount = p.Links.Count,

                // Берем самый "выгодный" лог: минимальная цена, а если одинаковая — самый свежий
                BestLog = _db.PriceLogs
                    .Where(l => l.ProductId == p.Id)
                    .OrderBy(l => l.PriceKopeks)
                    .ThenByDescending(l => l.ParsedAt)
                    .Select(l => new { l.PriceKopeks, ShopName = l.Shop.Name })
                    .FirstOrDefault()
            })
            .Select(x => new Row(
                x.Id,
                x.Name,
                x.Sku,
                x.LinksCount,
                x.BestLog != null ? x.BestLog.PriceKopeks : null,
                x.BestLog != null ? x.BestLog.ShopName : null
            ))
            .ToListAsync();
    }
}
