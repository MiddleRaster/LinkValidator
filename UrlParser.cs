using System.Web;

namespace LinkValidator
{
    public static class UrlParser
    {
        public static string Normalize(string link, string cwd, string siteRoot)
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
                link.StartsWith("mailto:",     StringComparison.OrdinalIgnoreCase) ||
                link.StartsWith("tel:",        StringComparison.OrdinalIgnoreCase))
                return "";

            // Absolute URL?
            if (link.StartsWith("http://",  StringComparison.OrdinalIgnoreCase) ||
                link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return link;
            }

            // Protocol-relative: "//example.com/page"
            if (link.StartsWith("//"))
            {   // Infer protocol from siteRoot
                string protocol = siteRoot.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? "https:" : "http:";
                return protocol + link;
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

        public static string GetDirectory(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return url;

            // If the URL ends with a slash, it's already a directory
            if (uri.AbsolutePath.EndsWith("/"))
                return uri.GetLeftPart(UriPartial.Path);

            // If the path has no dot, treat it as a folder missing a slash
            var lastSegment = uri.Segments[^1];
            if (!lastSegment.Contains('.'))
                return uri.GetLeftPart(UriPartial.Path) + "/";

            // Otherwise it's a file; strip the file name
            var directory = uri.AbsoluteUri.Substring(0,
                uri.AbsoluteUri.Length - lastSegment.Length);

            return directory;
        }

        public static string GetDomain(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return url;

            return $"{uri.Scheme}://{uri.Host}";
        }

        public static string NormalizeQueryParameters(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            url = System.Net.WebUtility.HtmlDecode(url); // Decode HTML entities like &amp; into &

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return url;

            if (string.IsNullOrEmpty(uri.Query) || uri.Query == "?")
                return uri.ToString();  // Short-circuit: no query parameters

            var parsed = HttpUtility.ParseQueryString(uri.Query);
            var dict   = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in parsed.AllKeys.Where(k => k != null))
            {
                var values = parsed.GetValues(key);
                dict[key!] = values?.LastOrDefault() ?? string.Empty; // last value wins
            }
            var normalizedQuery = string.Join("&", dict.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase).Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var builder = new UriBuilder(uri)
            {
                Query = normalizedQuery
            };

            return builder.Uri.ToString().Replace("\"", "%22"); // that last bit is to escale https://www.reddit.com/r/kanban/?f=flair_name:"Discussion"
        }
    }
}