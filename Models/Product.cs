namespace PriceParser.Web.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Sku { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ProductLink> Links { get; set; } = new();
}
