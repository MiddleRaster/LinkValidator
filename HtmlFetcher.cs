using System.Net;
using Microsoft.Playwright;

public static class HtmlFetcher
{
    private static readonly HttpClient client = new HttpClient();

    static HtmlFetcher()
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/120.0.0.0 Safari/537.36");

        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
        client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");
    }

    public static async Task<string?> FetchHtmlAsync(string url)
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) // TODO: this isn't right, because I will have incremental URLs, too.
        {
            Console.WriteLine($"Invalid URL: {url}");
            return null;
        }

        try
        {
            using var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden       ||
                response.StatusCode == HttpStatusCode.TooManyRequests ||
                response.StatusCode == HttpStatusCode.Unauthorized    ){
                return await FetchWithPlaywrightAsync(url); // If the server is blocking bots, escalate to Playwright
            }

            Console.WriteLine($"Failed to fetch {url}: {response.StatusCode}");
            return null;
        }
        catch (Exception /*ex*/)
        {
            return await FetchWithPlaywrightAsync(url);
        }
    }

    private static async Task<string?> FetchWithPlaywrightAsync(string url)
    {
        try
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
                });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                            "AppleWebKit/537.36 (KHTML, like Gecko) " +
                            "Chrome/120.0.0.0 Safari/537.36",
                Locale = "en-US"
            });

            var page = await context.NewPageAsync();
            page.SetDefaultNavigationTimeout(30000);

            var response = await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            if (response != null && response.Status >= 400)
            {
                Console.WriteLine($"Playwright navigation returned {(int)response.Status} for {url}");
                return null;
            }

            return await page.ContentAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Playwright fetch failed for {url}: {ex.Message}");
            return null;
        }
    }
}