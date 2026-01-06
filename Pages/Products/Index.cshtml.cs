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

    public record Row(int Id, string Name, string? Sku, int LinksCount);

    public async Task OnGetAsync()
    {
        var q = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(Query))
            q = q.Where(p => p.Name.Contains(Query));

        Items = await q
            .OrderByDescending(p => p.Id)
            .Select(p => new Row(
                p.Id,
                p.Name,
                p.Sku,
                p.Links.Count
            ))
            .ToListAsync();
    }
}
