using HtmlAgilityPack;


namespace TestTask
{
    public class HtmlParser
    {
        private string Adress { get; set; }
        private string host { get; set; }

        private HashSet<string> UrlList;
        private HashSet<string> CheckedUrl;

        public HtmlParser(string adress)
        {
            Adress = adress;
            UrlList = new HashSet<string>();
            CheckedUrl = new HashSet<string>();
            host = GetHost(Adress);
        }

        public HtmlDocument GetHtml(string adress) //retrieves html document from url
        {
            var httpclient = new HttpClient();
            var Html = httpclient.GetStringAsync(adress).Result;
            HtmlDocument HtmlDoc = new HtmlDocument();
            HtmlDoc.LoadHtml(Html);
            return HtmlDoc;
        }
        public bool CheckUrl(string adress)     //Checks if url corresponds criteria  - "contains html document"
        {
            if (adress.Contains("http") && !adress.Contains("#") && adress.Contains(host))
                return true;
            else return false;
        }
        public bool CheckIfSiteIsHtmlDoc(string adress)
        {
            var httpclient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, adress);
            using var response = httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            if (response.Content.Headers.ContentType.MediaType == "text/html")
                return true;
            else return false;
        }
        public HashSet<string> ParseUrl(string adress) //method for crawling page and finding urls on it
        {
            CheckedUrl.Add(adress);
            var html = GetHtml(adress);
            HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//a[@href]");
            foreach (var n in nodes)
            {
                string href = n.Attributes["href"].Value;
                try
                {
                    var absUrl = GetAbsoluteUrlString(Adress, href);
                    if (!UrlList.Contains(absUrl) && CheckUrl(absUrl) && CheckIfSiteIsHtmlDoc(absUrl))
                    {
                        UrlList.Add(absUrl);
                        if (!CheckedUrl.Contains(absUrl)) //check if url contains website's domain to prevent crawling all WWW
                        {
                            ParseUrl(absUrl);
                        }                     
                    }
                    else
                    {
                        CheckedUrl.Add(absUrl);

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine($"Cant't open url {GetAbsoluteUrlString(Adress, href)}");
                }
            }
            return UrlList;
        }
        private string GetAbsoluteUrlString(string baseUrl, string url) //gets absolute url if it is relative
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
                uri = new Uri(new Uri(baseUrl), uri);
            string result = uri.ToString();
            if (result.EndsWith('/'))
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }
        private string GetHost(string url) //gets domain of website.
        {
            var uri = new Uri(url);
            string host = uri.Host;
            return host;
        }
    }
}
