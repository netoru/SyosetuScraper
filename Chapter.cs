using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public Dictionary<string, Image> Images { get; } = new Dictionary<string, Image>();

        private HtmlDocument _doc;

        private HtmlNodeCollection _footnotes;


        //public List<string> Text { get; private set; }
        //public List<string> AuthorNote { get; private set; }



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

            var node = new HtmlNode(HtmlNodeType.Element, _doc, 0) { Id = "La0", Name = "p",
                InnerHtml = "================Author Note================" };
            aLineNodes.Insert(0, node);

            DivideInPages(aLineNodes, ref chk, ref pageIndex);
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
                var fNode = new HtmlNode(HtmlNodeType.Element, _doc, 0) { Id = "Lf0", Name = "p",
                    InnerHtml = "================Footnotes================" };

                _footnotes.Add(fNode);
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

            //would it then be better to add them as footnotes?
            //as in, append [x], {x}, etc. or nothing at the end of the line
            //「お、おいっ。綾っ！聞こえてるだろ、そこにいる男も、お、おい――!?」{1}
            //then after the author note add a footnotes section
            //================Footnotes================
            //{1} そこにいる男: ・・・・・・
            //or
            //================Footnotes================
            //Line: 「お、おいっ。綾っ！聞こえてるだろ、そこにいる男も、お、おい――!?」
            //Furigana: そこにいる男(・・・・・・)
            //or
            //================Footnotes================
            //「お、おいっ。綾っ！聞こえてるだろ、そこにいる男も、お、おい――!?」
            //そこにいる男
            //・・・・・・
            //third mode seems better, just have to make sure 
            //to put some space between each footnote
            //like, add two Env.NewLine in-between each

            return node.InnerHtml.Replace(sub, kanji);
        }

        private void DivideInPages(HtmlNodeCollection nodeCollection, ref int chk, ref int pageIndex)
        {
            if (!Pages.ContainsKey(pageIndex))
                Pages[pageIndex] = Pages.New();

            foreach (var node in nodeCollection)
            {
                var line = "";

                if (node.InnerHtml.Contains("<img"))
                {
                    Image img = null; // GetImage(node);

                    if (img == null)
                        line = "================Unable to download image================";
                    else
                    {
                        line = $"================Image {Id}-{node.Id}================";
                        Images.Add(node.Id, img);
                    }
                }
                else if (node.InnerHtml.Contains("<ruby>"))
                {
                    line = Furigana(node);
                }
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
                }

                Pages[pageIndex][node.Id] = line;
            }
        }

        public override string ToString()
        {
            var txt = new StringBuilder();

            txt.AppendLine(Id + ". " + Name);
            txt.AppendLine();
            txt.AppendLine();
            //txt.AppendLine(Text);

            return txt.ToString();
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
