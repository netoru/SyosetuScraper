using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SyosetuScraper
{
    class Chapter
    {
        public int Id { get; }
        public int Number { get; }
        public string Name { get; }
        public string Link { get; }
        public NestDictionary<int, string, string> Pages { get; } = new NestDictionary<int, string, string>();
        public bool Valid { get; private set; } = false;
        public NestDictionary<int, string, Image> Images { get; } = new NestDictionary<int, string, Image>();

        private HtmlDocument _doc;

        private HtmlNodeCollection _footnotes;

        public Chapter(int getId, int getNumber, string getName, string getLink) => (Id, Number, Name, Link) = (getId, getNumber, getName, getLink);

        public void CheckValidity()
        {
            _doc = Scraping.GetPage(Link, Scraping.SyousetsuCookie);

            var cNameNode = _doc.DocumentNode.SelectSingleNode("//p[@class='novel_subtitle']");
            var chapterName = (cNameNode == null) ? string.Empty : cNameNode.InnerText.TrimStart().TrimEnd();
            var cIdNode = _doc.DocumentNode.SelectSingleNode("//div[@id='novel_no']");
            var chapterId = (cIdNode == null) ? string.Empty : cIdNode.InnerText.TrimStart().TrimEnd();

            if (string.IsNullOrEmpty(chapterName) || string.IsNullOrEmpty(chapterId))
                return;

            if (chapterName != Name)
                return;

            chapterId = chapterId.Substring(0, chapterId.IndexOf("/"));

            if (Convert.ToInt32(chapterId) != Id)
                return;

            _footnotes = new HtmlNodeCollection(_doc.DocumentNode);

            Valid = true;
        }

        public void GetChapter()
        {
            var chapterNode = _doc.DocumentNode.SelectSingleNode("//div[@id='novel_honbun']");
            var lineNodes = chapterNode?.SelectNodes("./p[starts-with(@id, 'L')]");

            if (lineNodes == null)
                return;

            var chk = 0;
            var pageIndex = 0;
            DivideInPages(lineNodes, ref chk, ref pageIndex);

            var anoteNode = _doc.DocumentNode.SelectSingleNode("//div[@id='novel_a']");
            var aLineNodes = anoteNode?.SelectNodes("./p[starts-with(@id, 'La')]");

            if (aLineNodes == null)
                return;

            var node = HtmlNode.CreateNode("<p id=\"La0\">================Author Note================</p>");
            aLineNodes.Insert(0, node);

            DivideInPages(aLineNodes, ref chk, ref pageIndex);

            if (_footnotes == null)
                return;
            
            DivideInPages(_footnotes, ref chk, ref pageIndex);
        }

        private Image GetImage(HtmlNode node)
        {
            var nodeHtml = node.FirstChild.FirstChild.OuterHtml;
            var matches = Regex.Match(nodeHtml, "src=\"(.*?)\"").Groups;

            if (matches.Count < 2)
                return null;

            var link = matches[1].Value;

            if (link == null)
                return null;

            link = link.Replace("\\", "/");
            link = link.StartsWith("https:") || link.StartsWith("http:") ? link : "https:" + link;

            try
            {
                using var wb = new WebClient();
                wb.Headers.Add("user-agent", "definitely-not-a-screen-scraper");
                var bytes = wb.DownloadData(link);
                var ms = new MemoryStream(bytes);
                return Image.FromStream(ms);
            }
            catch (WebException)
            {
                return null;
            }
        }

        private string Furigana(HtmlNode node)
        {
            if (_footnotes.Count == 0)
            {
                var firstNode = HtmlNode.CreateNode("<p id=\"Lf0\">================Footnotes================</p>");

                _footnotes.Add(firstNode);
            }

            var sub = Regex.Match(node.InnerHtml, "<ruby>(.*)</ruby>").Value;
            var kMatches = Regex.Matches(sub, "<rb>(.*?)</rb>");
            var fMatches = Regex.Matches(sub, "<rt>(.*?)</rt>");

            var kanji = "";
            var furigana = "";

            foreach (var kMatch in kMatches.Where(kMatch => kMatch.Groups.Count > 1))            
                kanji += kMatch.Groups[1].Value;

            foreach (var fMatch in fMatches.Where(fMatch => fMatch.Groups.Count > 1))
                furigana += fMatch.Groups[1].Value;

            var line = node.InnerHtml.Replace(sub, kanji);

            _footnotes.Add(HtmlNode.CreateNode($"<p id=\"{node.Id}L\">{line}</p>"));
            _footnotes.Add(HtmlNode.CreateNode($"<p id=\"{node.Id}K\">{kanji}</p>"));
            _footnotes.Add(HtmlNode.CreateNode($"<p id=\"{node.Id}F\">{furigana}</p>"));
            _footnotes.Add(HtmlNode.CreateNode($"<p id=\"{node.Id}E\"></p>"));

            return line;
        }

        private void DivideInPages(HtmlNodeCollection nodeCollection, ref int chk, ref int pageIndex)
        {
            if (!Pages.ContainsKey(pageIndex))
                Pages[pageIndex] = Pages.New();
            if (!Images.ContainsKey(pageIndex))
                Images[pageIndex] = Images.New();

            foreach (var node in nodeCollection)
            {
                var line = "";

                if (node.InnerHtml.Contains("<img"))
                {
                    Image img = GetImage(node);

                    if (img == null)
                        line = "================Unable to download image================";
                    else
                    {
                        line = $"================Image {Id}-{node.Id}================";
                        Images[pageIndex][node.Id] = img;
                    }
                }
                else if (node.InnerHtml.Contains("<ruby>"))                
                    line = Furigana(node);                
                else                
                    line = node.InnerText;                

                line = line.Replace("　", "");
                chk += line.Length;

                if (chk > 5000)
                {
                    chk = line.Length;
                    pageIndex++;

                    if (!Pages.ContainsKey(pageIndex))
                        Pages[pageIndex] = Pages.New();
                    if (!Images.ContainsKey(pageIndex))
                        Images[pageIndex] = Images.New();
                }

                Pages[pageIndex][node.Id] = line;
            }
        }

        public string ToString(int page)
        {
            if (!Pages.ContainsKey(page))
                return "";

            var txt = new StringBuilder();

            txt.AppendLine(Name);
            txt.AppendLine();
            txt.AppendLine();

            foreach (var line in Pages[page])
                txt.AppendLine(line.Value);            

            return txt.ToString();
        }

        public void Save(string path)
        {
            foreach (var page in Pages)
            {
                var chapterPath = path + $"\\{Id}-{page.Key} - {Novel.CheckChars(Name)}.txt";

                if (!File.Exists(chapterPath))
                {
                    TextWriter tw = new StreamWriter(chapterPath);
                    tw.WriteLine(ToString(page.Key));
                    tw.Close();
                }
                else if (File.Exists(chapterPath))
                    using (var tw = new StreamWriter(chapterPath, false))
                        tw.WriteLine(ToString(page.Key));
            }


            foreach (var page in Images)
            {
                foreach (var image in page.Value)
                {
                    var imagePath = path + $"\\{Id}-{page.Key}-{image.Key}.png";
                    image.Value.Save(imagePath, ImageFormat.Png);
                }
            }
        }
    }

    public class NestDictionary<TKey1, TKey2, TValue> :
        Dictionary<TKey1, Dictionary<TKey2, TValue>>
    {
    }

    public static class NestDictionaryExtensions
    {
        public static Dictionary<TKey2, TValue> New<TKey1, TKey2, TValue>(this NestDictionary<TKey1, TKey2, TValue> _) => new Dictionary<TKey2, TValue>();
    }
}
