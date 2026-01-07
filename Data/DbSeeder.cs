using System.Text.Json;
using PriceParser.Web.Models;

namespace PriceParser.Web.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        var selectors = new[]
        {
            "meta[itemprop='price']",
            "meta[property='product:price:amount']",
            "meta[property='og:price:amount']",
            "meta[property='og:price']",
            "meta[name='price']",
            "[itemprop='price']",
            "[data-testid='price']",
            "[data-qa='price']",
            "[data-qa='product-price']",
            ".product-price__current",
            ".product-price",
            ".product__price",
            ".price",
            ".price__current",
            ".current-price",
            ".card-price",
            ".new-price",
            ".sale-price"
        };

        var selectorsJson = JsonSerializer.Serialize(selectors);

        var shop = db.Shops.FirstOrDefault(s => s.Code == "santehnika-online");

        if (shop == null)
        {
            db.Shops.Add(new Shop
            {
                Code = "santehnika-online",
                Name = "santehnika-online.ru",
                BaseUrl = "https://santehnika-online.ru",
                PriceSelectors = selectorsJson
            });
        }
        else
        {
            shop.Name = "santehnika-online.ru";
            shop.BaseUrl = "https://santehnika-online.ru";
            shop.PriceSelectors = selectorsJson;
        }

        db.SaveChanges();
    }
}
