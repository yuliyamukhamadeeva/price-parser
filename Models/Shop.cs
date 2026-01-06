namespace PriceParser.Web.Models;

public class Shop
{
    public string? PriceSelectors { get; set; }

    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
}
