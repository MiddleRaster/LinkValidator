
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

                string cwd = GetDirectory(url);

                string? html = await HtmlFetcher.FetchHtmlAsync(url);
                if (html != null)
                {
                    // only recurse if in our domain
                    if (!url.StartsWith(baseURL))
                        continue;

                    List<string> links = HrefExtractor.ExtractAllHrefs(html); // now extract all the URLs from the html string
                    foreach (string link in links)
                    {
                        string normalized = Normalize(link, cwd, baseURL);

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

        private static string Normalize(string link, string cwd, string siteRoot)
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
                string root = siteRoot.EndsWith("/")
                    ? siteRoot.TrimEnd('/')
                    : siteRoot;

                return root + link;
            }

            // Relative path: "page.html", "foo/bar", "../stuff"
            string prefix = cwd.EndsWith("/") ? cwd : cwd + "/";

            return prefix + link;
        }


        private static string GetDirectory(string url)
        {
            int lastSlash = url.LastIndexOf('/');
            if (lastSlash < 8)  // https://blah.com without a trailing slash needs to work, too
                return url + "/";

            return url.Substring(0, lastSlash + 1);
        }
    }
}
