namespace PriceParser.Web.Models;

public class PriceLog
{
    public long Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = default!;

    public string Url { get; set; } = default!;


    public long? PriceKopeks { get; set; }
    public string Currency { get; set; } = "RUB";

    public DateTime ParsedAt { get; set; } = DateTime.UtcNow;


    public string Status { get; set; } = "OK";
    public string? ErrorMessage { get; set; }
    public string? RawPriceText { get; set; }
}
