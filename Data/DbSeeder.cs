using PriceParser.Web.Models;

namespace PriceParser.Web.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Shops.Any()) return;

        db.Shops.AddRange(
            new Shop { Code = "gipermarketdom", Name = "gipermarketdom.ru", BaseUrl = "https://gipermarketdom.ru", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 },
            new Shop { Code = "vodoparad", Name = "vodoparad.ru", BaseUrl = "https://www.vodoparad.ru", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 },
            new Shop { Code = "neptun66", Name = "neptun66.ru", BaseUrl = "https://neptun66.ru", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 },
            new Shop { Code = "rusklimat-ekb", Name = "rusklimat.ru (Екатеринбург)", BaseUrl = "https://www.rusklimat.ru/ekaterinburg", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 },
            new Shop { Code = "grohe-shop", Name = "shop.grohe.ru", BaseUrl = "https://shop.grohe.ru", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 },
            new Shop { Code = "domotex", Name = "domotex.ru", BaseUrl = "https://www.domotex.ru", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 },
            new Shop { Code = "smesitel96", Name = "smesitel96.ru", BaseUrl = "https://smesitel96.ru", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 },
            new Shop { Code = "fixsen", Name = "fixsen.su", BaseUrl = "https://fixsen.su", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 },
            new Shop { Code = "roca-shop", Name = "shop.roca.ru", BaseUrl = "https://shop.roca.ru", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 },
            new Shop { Code = "cersanit-3d", Name = "cersanit.ru (3d-be)", BaseUrl = "https://cersanit.ru/catalog/3d-be", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 },
            new Shop { Code = "santehnika-online", Name = "santehnika-online.ru", BaseUrl = "https://santehnika-online.ru", PriceSelectors = "[\"meta[itemprop='price']\",\"meta[property='product:price:amount']\",\"meta[property='og:price:amount']\",\"[itemprop='price']\",\".product-price__current\",\".product-price\",\".product__price\",\".price\",\".price__current\",\".current-price\",\".card-price\",\".new-price\",\".sale-price\"]",
 }
        );

        db.SaveChanges();
    }
}
