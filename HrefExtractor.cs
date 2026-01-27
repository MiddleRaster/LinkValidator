namespace LinkValidator
{
    public static class HrefExtractor
    {
        public static List<string> ExtractAllHrefs(string html)
        {
            html = System.Net.WebUtility.HtmlDecode(html);

            var results = new List<string>();
            int pos = 0;

            while (true)
            {
                // Find the next <a>, <script>, and <style> after 'pos'
                int nextA = html.IndexOf("<a", pos, StringComparison.OrdinalIgnoreCase);
                int nextScript = html.IndexOf("<script", pos, StringComparison.OrdinalIgnoreCase);
                int nextStyle = html.IndexOf("<style", pos, StringComparison.OrdinalIgnoreCase);

                // If there's no more <a>, we're done
                if (nextA < 0)
                    break;

                // Determine the nearest tag of interest
                int  nextTag  = nextA;
                bool isScript = false;
                bool isStyle  = false;

                if (nextScript >= 0 && nextScript < nextTag)
                    (nextTag, isScript, isStyle) = (nextScript, true, false);

                if (nextStyle >= 0 && nextStyle < nextTag)
                    (nextTag, isScript, isStyle) = (nextStyle, false, true);

                // If the nearest thing is a <script>, skip its block and continue
                if (isScript)
                {
                    int scriptEnd = html.IndexOf("</script>", nextScript, StringComparison.OrdinalIgnoreCase);
                    if (scriptEnd < 0)
                        break; // malformed HTML, bail
                    pos = scriptEnd + "</script>".Length;
                    continue;
                }

                // If the nearest thing is a <style>, skip its block and continue
                if (isStyle)
                {
                    int styleEnd = html.IndexOf("</style>", nextStyle, StringComparison.OrdinalIgnoreCase);
                    if (styleEnd < 0)
                        break;
                    pos = styleEnd + "</style>".Length;
                    continue;
                }

                // If we get here, the nearest tag is a real <a>
                int aPos = nextA;

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
