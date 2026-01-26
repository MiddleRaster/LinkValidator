public static class HtmlFetcher
{
    private static readonly HttpClient client = new HttpClient();

    static HtmlFetcher()
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/120.0.0.0 Safari/537.36");
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
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch {url}: {response.StatusCode}");
                return null;
            }

            string html = await response.Content.ReadAsStringAsync();
            return html;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching {url}: {ex.Message}");
            return null;
        }
    }
}