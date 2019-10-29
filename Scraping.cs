using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SyosetuScraper
{
    class Scraping
    {
        private readonly string _savePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Syosetu Novels\";
        private readonly List<Novel> _novels = new List<Novel>();
        private static readonly Dictionary<string, string> _cookieIndx = new Dictionary<string, string>();
        public static CookieContainer SyousetsuCookie { get; } = new CookieContainer();

        public void Crawl(string[] urlCollection)
        {
            GenerateCookies();

            //Had I been smarter, 
            //I would've been able to make the two following foreach loops async
            /*foreach (var url in urlCollection)
            {
                var toc = GetPage(url, SyousetsuCookie);
                _novels.Add(new Novel(url, toc));
            }*/
            _novels.Add(new Novel(urlCollection[4], GetPage(urlCollection[4], SyousetsuCookie)));

            foreach (var volume in _novels[0].Volumes)
            {
                foreach (var chapter in volume.Chapters)
                {
                    chapter.CheckValidity();

                    if (chapter.Valid)
                        chapter.GetChapter();
                }
            }
            /*
            _novels[0].Volumes[1].Chapters[2].CheckValidity();

            if (_novels[0].Volumes[1].Chapters[2].Valid)
                _novels[0].Volumes[1].Chapters[2].GetChapter();
            */
            Save(_novels[0], false);

            /*foreach (var novel in _novels)            
                Save(novel, false);*/

            Console.WriteLine("Download complete.");
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

        //instead of being here, consider moving save to the Novel class
        private void Save(Novel novel, bool CreateFoldersForEachVolume = true)
        {
            //add handling to save somewhere else
            string path = _savePath + novel.Type + "\\" + CheckChars(novel.Name);
            Directory.CreateDirectory(path);

            var indexPath = path + "\\_Index.txt";
            if (!File.Exists(indexPath))
            {
                TextWriter tw = new StreamWriter(indexPath);
                tw.WriteLine(novel.ToString());
                tw.Close();
            }
            else if (File.Exists(indexPath))
                using (var tw = new StreamWriter(indexPath, false))
                    tw.WriteLine(novel.ToString());

            foreach (var volume in novel.Volumes)
            {
                var volPath = path;

                if (CreateFoldersForEachVolume)
                    if (!string.IsNullOrEmpty(volume.Name))
                    {
                        volPath += $"\\{volume.Number} - {CheckChars(volume.Name)}";
                        Directory.CreateDirectory(volPath);
                    }

                foreach (var chapter in volume.Chapters)
                {
                    foreach (var page in chapter.Pages)
                    {
                        var chapterPath = volPath + $"\\{chapter.Id}-{page.Key} - {CheckChars(chapter.Name)}.txt";

                        if (!File.Exists(chapterPath))
                        {
                            TextWriter tw = new StreamWriter(chapterPath);
                            tw.WriteLine(chapter.ToString(page.Key));
                            tw.Close();
                        }
                        else if (File.Exists(chapterPath))
                            using (var tw = new StreamWriter(chapterPath, false))
                                tw.WriteLine(chapter.ToString());
                    }
                }
            }
        }

        //Check for illegal characters
        private string CheckChars(string input)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(input, "□");
        }
    }
}
