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
    }
}