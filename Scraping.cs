using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SyosetuScraper
{
    class Scraping
    {
        public static Dictionary<string, string> KnownTags { get; } = new Dictionary<string, string>();
        public static CookieContainer SyousetsuCookie { get; } = new CookieContainer();

        private readonly static string _defaultSavePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Syosetu Novels\";
        private static readonly Dictionary<string, string> _cookieIndx = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _urls = new Dictionary<string, string>();
        private static readonly List<Novel> _novels = new List<Novel>();

        public static async Task<bool> CrawlAsync()
        {
            if (string.IsNullOrEmpty(Settings.Default.SavePath) || !Directory.Exists(Settings.Default.SavePath))
                Settings.Default.SavePath = _defaultSavePath;

            if (string.IsNullOrEmpty(Settings.Default.SourceFileNameFormat))
                Settings.Default.SourceFileNameFormat = "URLs";

            var source = Settings.Default.SavePath + Settings.Default.SourceFileNameFormat + ".txt";

            if (!File.Exists(source))            
                return false;            

            GenerateCookies();

            if (Settings.Default.ScrapeTags)
                GetTagsDictionary();

            var lines = File.ReadAllLines(source);

            foreach (var line in lines)
            {
                var x = line.Split(";");

                if (x.Length < 1) continue;
                if (_urls.ContainsKey(x[0])) continue;

                var nick = (x.Length > 1) ? x[1] : string.Empty;
                
                _urls.Add(x[0], nick);
            }

            foreach (var url in _urls)
                _novels.Add(new Novel(url.Value, url.Key, GetPage(url.Key)));

            var tasks = new Task[_novels.Count];

            var i = 0;
            foreach (var novel in _novels)
            {
                if (i >= _novels.Count)
                    break;

                tasks[i] = Task.Run(() => novel.Setup());
                i++;
            }

            Task.WaitAll(tasks);

            foreach (var novel in _novels)
                novel.Save();

            return true;
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

        public static HtmlDocument GetPage(string link)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(link);
                request.Method = "GET";
                request.CookieContainer = SyousetsuCookie;
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

        private static void GetTagsDictionary()
        {
            var path = Settings.Default.SavePath + Settings.Default.KnownTagsFileName + ".txt";

            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                var x = line.Split(";");

                if (x.Length < 2) continue;
                if (KnownTags.ContainsKey(x[0])) continue;

                KnownTags.Add(x[0], x[1]);
            }
        }
    }
}