using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PriceParser.Web.Data;
using PriceParser.Web.Services;

namespace PriceParser.Web.Pages.Parsing;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly PriceParserService _service;

    public IndexModel(AppDbContext db, PriceParserService service)
    {
        _db = db;
        _service = service;
    }

    public List<LogRow> LastLogs { get; set; } = new();
    public List<SummaryRow> Summary { get; set; } = new();

  public record LogRow(string Time, string ProductName, string ShopName, string Price);
public record SummaryRow(string ProductName, string Price, string ShopName);


    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var saved = await _service.RunAsync(HttpContext.RequestAborted);
        TempData["Success"] = $"Парсинг выполнен, записей: {saved}";
        return RedirectToPage("Index");
    }

    private async Task LoadAsync()
    {
        LastLogs = await _db.PriceLogs
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.Shop)
            .OrderByDescending(x => x.ParsedAt)
            .Take(50)
            .Select(x => new LogRow(
                x.ParsedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss"),
                x.Product.Name,
                x.Shop.Name,
                FormatRub(x.PriceKopeks)
            ))
            .ToListAsync();

        var recent = await _db.PriceLogs
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.Shop)
            .OrderByDescending(x => x.ParsedAt)
            .Take(2000)
            .ToListAsync();

        Summary = recent
            .GroupBy(x => x.ProductId)
            .Select(g =>
            {
var best = g.OrderBy(x => x.PriceKopeks).ThenByDescending(x => x.ParsedAt).First();

return new SummaryRow(best.Product.Name, FormatRub(best.PriceKopeks), best.Shop.Name);


            })
            .OrderBy(x => x.ProductName)
            .ToList();
    }
    private static string FormatRub(long? kopeks)
{
    if (!kopeks.HasValue) return "—";
    var rub = kopeks.Value / 100m;
    return rub.ToString("0.00");
}

}
