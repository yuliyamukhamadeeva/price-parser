using Microsoft.Playwright;

namespace PriceParser.Web.Services;

public sealed class HtmlFetcher : IAsyncDisposable
{
    private readonly ILogger<HtmlFetcher> _logger;

    private IPlaywright? _pw;
    private IBrowser? _browser;

    public HtmlFetcher(ILogger<HtmlFetcher> logger)
    {
        _logger = logger;
    }

    private async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser != null) return _browser;

        _pw ??= await Playwright.CreateAsync();
        _browser = await _pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        return _browser;
    }

    public async Task<string?> GetHtmlAsync(string url, CancellationToken ct)
    {
        var browser = await GetBrowserAsync();

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X) AppleWebKit/537.36 (KHTML, like Gecko) Chrome Safari",
            Locale = "ru-RU"
        });

        var page = await context.NewPageAsync();

        try
        {
            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 45000
            });

       
            await page.WaitForTimeoutAsync(1500);

            ct.ThrowIfCancellationRequested();

            var html = await page.ContentAsync();

            _logger.LogInformation("HTML fetched: {Len} chars | {Url}", html?.Length ?? 0, url);

            if (IsBlocked(html))
                _logger.LogWarning("ANTI-BOT page detected for {Url}", url);

            return html;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Playwright fetch failed: {Url}", url);
            return null;
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
        }
    }

    private static bool IsBlocked(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return true;

        return html.Contains("servicepipe.ru", StringComparison.OrdinalIgnoreCase)
               || html.Contains("id_captcha_frame_div", StringComparison.OrdinalIgnoreCase)
               || html.Contains("/exhkqyad", StringComparison.OrdinalIgnoreCase);
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null) await _browser.DisposeAsync();
        _pw?.Dispose();
    }
}
