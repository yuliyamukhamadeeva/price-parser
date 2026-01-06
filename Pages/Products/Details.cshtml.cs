using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PriceParser.Web.Data;
using PriceParser.Web.Models;

namespace PriceParser.Web.Pages.Products;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;

    public DetailsModel(AppDbContext db) => _db = db;

    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string ProductSku { get; set; } = "";
    public List<SelectListItem> ShopOptions { get; set; } = new();
    public List<LinkRow> Links { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public record LinkRow(string ShopName, string Url, bool IsActive);

    public class InputModel
    {
        [Range(1, int.MaxValue)]
        public int ShopId { get; set; }

        [Required]
        [Url]
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

        Input = new InputModel { IsActive = true };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        ProductId = id;

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        if (!ModelState.IsValid)
        {
            ProductName = product.Name;
            ProductSku = string.IsNullOrWhiteSpace(product.Sku) ? "SKU не указан" : $"SKU: {product.Sku}";
            await LoadShopsAsync();
            await LoadLinksAsync();
            return Page();
        }

        var url = Input.Url.Trim();

        var link = new ProductLink
        {
            ProductId = id,
            ShopId = Input.ShopId,
            Url = url,
            IsActive = Input.IsActive
        };

        _db.ProductLinks.Add(link);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            TempData["Error"] = "Такая ссылка уже добавлена";
            return RedirectToPage(new { id });
        }

        TempData["Success"] = "Ссылка добавлена";
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
            .Select(l => new LinkRow(l.Shop.Name, l.Url, l.IsActive))
            .ToListAsync();
    }
}
