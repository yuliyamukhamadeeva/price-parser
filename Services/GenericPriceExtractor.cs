using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;

namespace PriceParser.Web.Services;

public class GenericPriceExtractor
{
    private static readonly Regex NumRx = new(@"(\d[\d\s]*([.,]\d{1,2})?)", RegexOptions.Compiled);

    public async Task<decimal?> TryExtractAsync(string url, HttpClient http, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X) AppleWebKit/537.36 (KHTML, like Gecko) Chrome Safari");

        using var res = await http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) return null;

        var html = await res.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(html)) return null;

        var ctx = BrowsingContext.New(Configuration.Default);
        var doc = await ctx.OpenAsync(r => r.Content(html), ct);

        var candidates = new List<string?>();

        candidates.Add(doc.QuerySelector("meta[property='product:price:amount']")?.GetAttribute("content"));
        candidates.Add(doc.QuerySelector("meta[property='og:price:amount']")?.GetAttribute("content"));
        candidates.Add(doc.QuerySelector("meta[itemprop='price']")?.GetAttribute("content"));
        candidates.Add(doc.QuerySelector("[itemprop='price']")?.TextContent);
        candidates.Add(doc.QuerySelector("[data-price]")?.GetAttribute("data-price"));

        foreach (var sel in new[]
        {
            ".price", ".product-price", ".product__price", ".product-price__current",
            ".price__current", ".current-price", ".card-price", ".new-price", ".sale-price"
        })
        {
            candidates.Add(doc.QuerySelector(sel)?.TextContent);
        }

        candidates.Add(FindAnyPriceLikeText(doc));

        foreach (var raw in candidates.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var p = ParsePrice(raw!);
            if (p.HasValue && p.Value > 0) return p.Value;
        }

        return null;
    }

    private static string? FindAnyPriceLikeText(IDocument doc)
    {
        var nodes = doc.All.Where(n =>
            n is IElement e &&
            e.TextContent is { Length: > 0 } t &&
            (t.Contains("₽") || t.Contains("руб") || t.Contains("RUB", StringComparison.OrdinalIgnoreCase)) &&
            t.Length < 80
        ).Take(20);

        foreach (var n in nodes)
            return n.TextContent;

        return null;
    }

    private static decimal? ParsePrice(string s)
    {
        var m = NumRx.Match(s);
        if (!m.Success) return null;

        var num = m.Groups[1].Value.Replace(" ", "").Replace("\u00A0", "");
        num = num.Replace(",", ".");

        if (decimal.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
            return v;

        return null;
    }
}
