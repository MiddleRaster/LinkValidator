
using System;

namespace LinkValidator
{
    public class LinkValidator
    {
        public static async Task<int> Main(string[] args)
        {
            int ret = 0;
            if (args.Length == 0) {
                Console.WriteLine("Please provide a URL to validate.\nUsage:  LinkValidator.exe URL-to-validate");
                return ret;
            }

            var pending = new Queue<string>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string baseURL = args[0];

            pending.Enqueue(baseURL);
            visited.Add    (baseURL);

            while (pending.Count > 0)
            {
                string url = pending.Dequeue();
                Console.WriteLine($"dequeueing {url}");

                string? html = await HtmlFetcher.FetchHtmlAsync(url);
                if (html != null)
                {
                    // only recurse if in our domain
                    if (!url.StartsWith(baseURL))
                        continue;

                    List<string> links = UrlExtractor.ExtractAllHrefs(html); // now extract all the URLs from the html string
                    foreach (string link in links)
                    {
                        string normalized = Normalize(link, baseURL);

                        if (!visited.Contains(normalized))
                        {
                            Console.WriteLine($"queueing {normalized}");

                            pending.Enqueue(normalized);
                            visited.Add    (normalized);
                        }
                    }
                }
                else
                {
                    // Console.WriteLine($"Failed to fetch HTML from {url}"); // error already written in HtmlFetcher
                    ret = -1;
                }
            }
            return ret;
        }

        private static string Normalize(string link, string baseURL)
        {
            if (string.IsNullOrWhiteSpace(link))
                return "";

            link = link.Trim();

            // Remove fragment (#section)
            int hash = link.IndexOf('#');
            if (hash >= 0)
                link = link.Substring(0, hash);

            // Ignore javascript:, mailto:, tel:, etc.
            if (link.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
                link.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                link.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
                return "";

            // Absolute URL?
            if (link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return link;
            }

            // Root-relative: "/about"
            if (link.StartsWith("/"))
            {
                // Ensure baseURL has no trailing slash
                if (baseURL.EndsWith("/"))
                    return baseURL.TrimEnd('/') + link;
                else
                    return baseURL + link;
            }

            // Relative path: "page.html", "foo/bar", "../stuff"
            // Ensure baseURL ends with a slash
            string prefix = baseURL.EndsWith("/") ? baseURL : baseURL + "/";

            return prefix + link;
        }
    }
}
