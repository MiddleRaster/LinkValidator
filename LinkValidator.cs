namespace LinkValidator
{
    public class LinkValidator
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine("Please provide a URL to validate.\nUsage:  LinkValidator.exe URL-to-validate");
                return -1;
            }

            return 0;
        }
    }
}
