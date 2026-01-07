using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PriceParser.Web.Data;
using PriceParser.Web.Models;
using PriceParser.Web.Services;

namespace PriceParser.Web.Pages.Products;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly PriceParserService _parser;

    public DetailsModel(AppDbContext db, PriceParserService parser)
    {
        _db = db;
        _parser = parser;
    }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string ProductSku { get; set; } = "";

    public decimal? MinPrice { get; set; }
    public string MinShopName { get; set; } = "";

    public List<SelectListItem> ShopOptions { get; set; } = new();
    public List<LinkRow> Links { get; set; } = new();
    public List<LogRow> LastLogs { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public record LinkRow(int Id, string ShopName, string Url, bool IsActive);
    public record LogRow(string ParsedAtLocal, string ShopName, decimal PriceRub);

    public class InputModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Выбери магазин")]
        public int ShopId { get; set; }

        [Required(ErrorMessage = "Вставь ссылку")]
        [Url(ErrorMessage = "Нужна корректная ссылка")]
        [StringLength(2000)]
        public string Url { get; set; } = "";

        public bool IsActive { get; set; } = true;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ProductId = id;

        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        ProductName = product.Name;
        ProductSku = string.IsNullOrWhiteSpace(product.Sku) ? "SKU не указан" : $"SKU: {product.Sku}";

        await LoadShopsAsync();
        await LoadLinksAsync();
        await LoadMinAndLogsAsync();

        Input = new InputModel { IsActive = true };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        ProductId = id;

        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        var hasShops = await _db.Shops.AsNoTracking().AnyAsync();
        if (!hasShops)
        {
            TempData["Error"] = "Сначала добавь магазины (DbSeeder), потом добавляй ссылки.";
            return RedirectToPage(new { id });
        }

        if (!ModelState.IsValid)
        {
            ProductName = product.Name;
            ProductSku = string.IsNullOrWhiteSpace(product.Sku) ? "SKU не указан" : $"SKU: {product.Sku}";
            await LoadShopsAsync();
            await LoadLinksAsync();
            await LoadMinAndLogsAsync();
            return Page();
        }

        var url = (Input.Url ?? "").Trim();

        var shop = await _db.Shops.AsNoTracking().FirstOrDefaultAsync(s => s.Id == Input.ShopId);
        if (shop is null)
        {
            TempData["Error"] = "Выбранный магазин не найден.";
            return RedirectToPage(new { id });
        }

        if (!string.IsNullOrWhiteSpace(shop.BaseUrl))
        {
            var baseUrl = shop.BaseUrl.Trim();
            if (!url.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = $"Ссылка не похожа на магазин \"{shop.Name}\". Ожидается: {baseUrl}";
                return RedirectToPage(new { id });
            }
        }

        var exists = await _db.ProductLinks.AsNoTracking().AnyAsync(l =>
            l.ProductId == id &&
            l.ShopId == Input.ShopId &&
            l.Url == url);

        if (exists)
        {
            TempData["Error"] = "Такая ссылка уже добавлена";
            return RedirectToPage(new { id });
        }

        _db.ProductLinks.Add(new ProductLink
        {
            ProductId = id,
            ShopId = Input.ShopId,
            Url = url,
            IsActive = Input.IsActive
        });

        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Ссылка добавлена";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Не получилось сохранить ссылку (возможно, дубль).";
        }

        return RedirectToPage(new { id });
    }

  
    public async Task<IActionResult> OnPostParseAsync(int id)
    {
        var exists = await _db.Products.AsNoTracking().AnyAsync(p => p.Id == id);
        if (!exists) return NotFound();

        var saved = await _parser.RunAsync(id, HttpContext.RequestAborted); 

        TempData["Success"] = saved > 0
            ? $"Парсинг выполнен, записей: {saved}"
            : "Парсинг выполнен, но цена не найдена";

        return RedirectToPage(new { id });
    }

    private async Task LoadShopsAsync()
    {
        ShopOptions = await _db.Shops.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
            .ToListAsync();

        if (!ShopOptions.Any())
            ShopOptions.Add(new SelectListItem("Нет магазинов", "0"));
    }

    private async Task LoadLinksAsync()
    {
        Links = await _db.ProductLinks.AsNoTracking()
            .Where(l => l.ProductId == ProductId)
            .Include(l => l.Shop)
            .OrderByDescending(l => l.IsActive)
            .ThenBy(l => l.Shop.Name)
            .Select(l => new LinkRow(l.Id, l.Shop.Name, l.Url, l.IsActive))
            .ToListAsync();
    }

    private async Task LoadMinAndLogsAsync()
    {
        var min = await _db.PriceLogs.AsNoTracking()
            .Where(x => x.ProductId == ProductId)
            .Include(x => x.Shop)
            .OrderBy(x => x.PriceKopeks)
            .Select(x => new { x.PriceKopeks, ShopName = x.Shop.Name })
            .FirstOrDefaultAsync();

        if (min is not null)
        {
            MinPrice = min.PriceKopeks / 100m;
            MinShopName = min.ShopName;
        }

        var logs = await _db.PriceLogs.AsNoTracking()
            .Where(x => x.ProductId == ProductId)
            .Include(x => x.Shop)
            .OrderByDescending(x => x.ParsedAt)
            .Take(30)
            .Select(x => new { x.ParsedAt, ShopName = x.Shop.Name, x.PriceKopeks })
            .ToListAsync();

LastLogs = logs
    .Select(x => new LogRow(
        x.ParsedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss"),
        x.ShopName,
(x.PriceKopeks ?? 0) / 100m

    ))
    .ToList();

    }
}
