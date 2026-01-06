using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PriceParser.Web.Data;
using PriceParser.Web.Models;

namespace PriceParser.Web.Pages.Products;

public class CreateModel : PageModel
{
    private readonly AppDbContext _db;

    public CreateModel(AppDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = "";

        [StringLength(100)]
        public string? Sku { get; set; }
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var product = new Product
        {
            Name = Input.Name.Trim(),
            Sku = string.IsNullOrWhiteSpace(Input.Sku) ? null : Input.Sku.Trim()
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Товар добавлен";
        return RedirectToPage("Details", new { id = product.Id });
    }
}
