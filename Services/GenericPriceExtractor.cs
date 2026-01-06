using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;

namespace PriceParser.Web.Services;

public class GenericPriceExtractor
{
    private readonly HtmlFetcher _fetcher;

    public GenericPriceExtractor(HtmlFetcher fetcher)
    {
        _fetcher = fetcher;
    }

    private static readonly Regex NumRx =
        new(@"(\d[\d\s\u00A0]*([.,]\d{1,2})?)", RegexOptions.Compiled);

    private static readonly Regex JsonPriceRx =
        new("\"price\"\\s*:\\s*\"?(\\d[\\d\\s\\u00A0]*([.,]\\d{1,2})?)\"?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async Task<decimal?> TryExtractBySelectorsAsync(
        string url,
        IEnumerable<string> selectors,
        HttpClient http,
        CancellationToken ct)
    {
  
        var html = await _fetcher.GetHtmlAsync(url, ct);

        html ??= await DownloadHtmlAsync(url, http, ct);

        if (string.IsNullOrWhiteSpace(html))
            return null;

        var rawHit = TryExtractFromRawHtml(html);
        if (rawHit.HasValue && rawHit.Value > 0) return rawHit.Value;

        var ctx = BrowsingContext.New(Configuration.Default);
        var doc = await ctx.OpenAsync(r => r.Content(html), ct);


        foreach (var sel in selectors ?? Array.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(sel)) continue;

            var node = doc.QuerySelector(sel);
            if (node is null) continue;

            var raw = node is IElement el && el.TagName.Equals("META", StringComparison.OrdinalIgnoreCase)
                ? el.GetAttribute("content")
                : node.TextContent;

            var p = ParsePrice(raw ?? "");
            if (p.HasValue && p.Value > 0) return p.Value;
        }


        var jsonLd = TryExtractFromJsonLd(doc);
        if (jsonLd.HasValue && jsonLd.Value > 0) return jsonLd.Value;

    
        var anyText = FindAnyPriceLikeText(doc);
        var anyParsed = ParsePrice(anyText ?? "");
        if (anyParsed.HasValue && anyParsed.Value > 0) return anyParsed.Value;

        return null;
    }

    public async Task<decimal?> TryExtractAsync(string url, HttpClient http, CancellationToken ct)
    {
        var selectors = new[]
        {
            "meta[itemprop='price']",
            "meta[property='product:price:amount']",
            "meta[property='og:price:amount']",
            "[itemprop='price']",
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

        return await TryExtractBySelectorsAsync(url, selectors, http, ct);
    }

    private static decimal? TryExtractFromRawHtml(string html)
    {
        var m = JsonPriceRx.Match(html);
        if (!m.Success) return null;

        var p = ParsePrice(m.Groups[1].Value);
        return p is > 0 ? p : null;
    }

    private static decimal? TryExtractFromJsonLd(IDocument doc)
    {
        var scripts = doc.QuerySelectorAll("script[type='application/ld+json']");
        foreach (var s in scripts)
        {
            var json = s.TextContent;
            if (string.IsNullOrWhiteSpace(json)) continue;

            var p = TryFindPriceInJson(json);
            if (p.HasValue && p.Value > 0) return p.Value;
        }

        return null;
    }

    private static decimal? TryFindPriceInJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);

            decimal? Find(JsonElement el)
            {
                if (el.ValueKind == JsonValueKind.Object)
                {
                    if (el.TryGetProperty("offers", out var offers))
                    {
                        var v = Find(offers);
                        if (v.HasValue) return v;
                    }

                    if (el.TryGetProperty("price", out var price))
                    {
                        if (price.ValueKind == JsonValueKind.Number && price.TryGetDecimal(out var d)) return d;

                        if (price.ValueKind == JsonValueKind.String)
                        {
                            var parsed = ParsePrice(price.GetString() ?? "");
                            if (parsed.HasValue) return parsed.Value;
                        }
                    }

                    foreach (var p in el.EnumerateObject())
                    {
                        var v = Find(p.Value);
                        if (v.HasValue) return v;
                    }
                }

                if (el.ValueKind == JsonValueKind.Array)
                {
                    foreach (var x in el.EnumerateArray())
                    {
                        var v = Find(x);
                        if (v.HasValue) return v;
                    }
                }

                return null;
            }

            return Find(doc.RootElement);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string?> DownloadHtmlAsync(string url, HttpClient http, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X) AppleWebKit/537.36 (KHTML, like Gecko) Chrome Safari");
        req.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        req.Headers.TryAddWithoutValidation("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

        using var res = await http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) return null;

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        Stream decoded = stream;

        var enc = res.Content.Headers.ContentEncoding.Select(x => x.ToLowerInvariant()).ToArray();
        if (enc.Contains("br"))
            decoded = new BrotliStream(stream, CompressionMode.Decompress, leaveOpen: false);
        else if (enc.Contains("gzip"))
            decoded = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: false);
        else if (enc.Contains("deflate"))
            decoded = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: false);

        using var reader = new StreamReader(decoded, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync();
    }

    private static string? FindAnyPriceLikeText(IDocument doc)
    {
        var nodes = doc.All.Where(n =>
            n is IElement &&
            n.TextContent is { Length: > 0 } t &&
            (t.Contains("₽") || t.Contains("руб") || t.Contains("RUB", StringComparison.OrdinalIgnoreCase)) &&
            t.Length < 120
        ).Take(50);

        foreach (var n in nodes)
            return n.TextContent;

        return null;
    }

    private static decimal? ParsePrice(string s)
    {
        var m = NumRx.Match(s);
        if (!m.Success) return null;

        var num = m.Groups[1].Value
            .Replace(" ", "")
            .Replace("\u00A0", "")
            .Replace(",", ".");

        if (decimal.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
            return v;

        return null;
    }
}
