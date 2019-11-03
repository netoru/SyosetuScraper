using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SyosetuScraper
{
    class Scraping
    {
        public readonly static string SavePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Syosetu Novels\";
        //private readonly List<Novel> _novels = new List<Novel>();
        private static readonly Dictionary<string, string> _cookieIndx = new Dictionary<string, string>();
        public static CookieContainer SyousetsuCookie { get; } = new CookieContainer();

        public static void Crawl()
        {
            string[] urlCollection = File.ReadAllLines(SavePath + "URLs.txt");

            GenerateCookies();

            foreach (var url in urlCollection)
            {
                var novel = new Novel(url, GetPage(url, SyousetsuCookie));
                novel.Setup();
                novel.Save();
            }
        }

        private static void GenerateCookies()
        {
            /*I thought all cookies were needed but 
             *now it doesn't give me an error anymore
             *for not having all of them...
             *tho having them too doesn't hurt.
            */
            _cookieIndx.Add("fix_menu_bar", "1");
            _cookieIndx.Add("fontsize", "0");
            _cookieIndx.Add("ks2", "4vbep391u5mu");
            _cookieIndx.Add("lineheight", "0");
            _cookieIndx.Add("novellayout", "0");
            _cookieIndx.Add("sasieno", "0");
            _cookieIndx.Add("over18", "yes");

            foreach (var item in _cookieIndx)
                SyousetsuCookie.Add(new Cookie(item.Key, item.Value, "/", ".syosetu.com"));
        }

        public static HtmlDocument GetPage(string link, CookieContainer cookies)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(link);
                request.Method = "GET";
                request.CookieContainer = cookies;
                //useragent is needed else the getresponse returns 403 forbidden
                request.UserAgent = "definitely-not-a-screen-scraper";
                var response = (HttpWebResponse)request.GetResponse();
                var stream = response.GetResponseStream();

                var doc = new HtmlDocument();
                using (var reader = new StreamReader(stream))
                {
                    string html = reader.ReadToEnd();
                    doc.LoadHtml(html);
                }

                return doc;
            }
            catch (WebException)
            {
                var html = new StringBuilder();
                html.Append("<html>");
                html.Append("<head>");
                html.Append("<title>エラー</title>");
                html.Append("</head>");
                html.Append("<body>");
                html.Append("</body>");
                html.Append("</html>");

                var doc = new HtmlDocument();
                doc.LoadHtml(html.ToString());
                return doc;
            }
        }

    }
}