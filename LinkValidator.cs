
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
            pending.Enqueue(args[0]);
            visited.Add    (args[0]);

            string domain  = UrlParser.GetDomain   (args[0]);
            string baseURL = UrlParser.GetDirectory(args[0]); // not necessarily the root

            while (pending.Count > 0)
            {
                string url = pending.Dequeue();
                Console.WriteLine($"dequeueing {url}");

                string cwd = UrlParser.GetDirectory(url);

                string? html = await HtmlFetcher.FetchHtmlAsync(url);
                if (html != null)
                {
                    // only recurse if in our domain
                    if (!url.StartsWith(baseURL, StringComparison.OrdinalIgnoreCase))
                        continue;

                    List<string> links = HrefExtractor.ExtractAllHrefs(html); // now extract all the URLs from the html string
                    foreach (string link in links)
                    {
                        string normalized = UrlParser.Normalize(link, cwd, domain);

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
    }
}
