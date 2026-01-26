namespace LinkValidator
{
    public class LinkValidator
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine("Please provide a URL to validate.\nUsage:  LinkValidator.exe URL-to-validate");
                return -1;
            }

            string? html = await HtmlFetcher.FetchHtmlAsync(args[0]);
            if (html != null)
            {
                Console.WriteLine(html);
                return 0; // success
            }
            else
            {
                Console.WriteLine("Failed to fetch HTML.");
                return 1; // failure
            }
        }
    }
}
