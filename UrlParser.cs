using System;
using System.Collections.Generic;
using System.Text;

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


        public static string GetDirectory(string url)
        {
            int lastSlash = url.LastIndexOf('/');
            if (lastSlash < 8)  // https://blah.com without a trailing slash needs to work, too
                return url + "/";

            return url.Substring(0, lastSlash + 1);
        }
    }
}
