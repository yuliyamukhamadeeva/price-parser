namespace PriceParser.Web.Models;

public class ProductLink
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = default!;

    public string Url { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
