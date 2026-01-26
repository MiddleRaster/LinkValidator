namespace LinkValidator
{
    public static class UrlExtractor
    {
        public static List<string> ExtractAllHrefs(string html)
        {
            var results = new List<string>();
            int pos = 0;

            while (true)
            {
                // Find next "<a"
                int aPos = html.IndexOf("<a", pos, StringComparison.OrdinalIgnoreCase);
                if (aPos < 0)
                    break;

                // Find "href" after that
                int hrefPos = html.IndexOf("href", aPos, StringComparison.OrdinalIgnoreCase);
                if (hrefPos < 0)
                {
                    pos = aPos + 2;
                    continue;
                }

                // Find '=' after href
                int eqPos = html.IndexOf('=', hrefPos);
                if (eqPos < 0)
                {
                    pos = hrefPos + 4;
                    continue;
                }

                // Skip whitespace after '='
                int valPos = eqPos + 1;
                while (valPos < html.Length && char.IsWhiteSpace(html[valPos]))
                    valPos++;

                if (valPos >= html.Length)
                    break;

                string? url = null;

                // Quoted?
                char first = html[valPos];
                if (first == '"' || first == '\'')
                {
                    int start = valPos + 1;
                    int end = html.IndexOf(first, start);
                    if (end > start)
                        url = html.Substring(start, end - start);

                    pos = end > 0 ? end + 1 : valPos + 1;
                }
                else
                {
                    // Unquoted: read until whitespace or '>'
                    int end = valPos;
                    while (end < html.Length &&
                           !char.IsWhiteSpace(html[end]) &&
                           html[end] != '>')
                        end++;

                    if (end > valPos)
                        url = html.Substring(valPos, end - valPos);

                    pos = end;
                }

                if (!string.IsNullOrWhiteSpace(url))
                    results.Add(url);
            }
            return results;
        }
    }
}
