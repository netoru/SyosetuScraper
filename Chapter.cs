using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Web;
using Google.Cloud.Translation.V2;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SyosetuScraper
{
    public class Chapter
    {
        public int Id { get; }
        public int Number { get; }
        public string Name { get; }
        public string tlName { get; set; }
        public string Link { get; }
        public NestDictionary<int, string, string> Pages { get; private set; } = new NestDictionary<int, string, string>();
        public int Valid { get; private set; } = -1;
        public NestDictionary<int, string, Image> Images { get; private set; } = new NestDictionary<int, string, Image>();

        private HtmlDocument _doc;
        private HtmlNodeCollection _header;
        private HtmlNodeCollection _footnotes;
        private readonly string _chapterPath;
        private bool _emptyPreviousLine = false;

        public Chapter(int getId, int getNumber, string getName, string getLink, string getPath)
        {
            Id = getId;
            Number = getNumber;
            Name = getName;
            Link = getLink;
            _chapterPath = getPath;

            if (Settings.Default.GoogleAPI && Settings.Default.TL_ChapterTitle)
            {
                TranslationResult result = Main.gClient.TranslateText(Name, LanguageCodes.English, LanguageCodes.Japanese);
                tlName = result.TranslatedText;
            }
            if (!string.IsNullOrEmpty(tlName) && Settings.Default.GoogleAPI && !Settings.Default.TL_KeepOriginalAsWell)
                Name = tlName;
        }

        public void CheckValidity()
        {
            if (Settings.Default.NoChapterAlreadyDL)
                if (ChapterExists())
                {
                    Valid = 5;
                    _doc = null;
                    return;
                }

            _doc = Helpers.GetPage(Link);

            var cNameNode = _doc.DocumentNode.SelectSingleNode("//p[@class='novel_subtitle']");
            var chapterName = (cNameNode == null) ? string.Empty : Helpers.RemoveCensorship(HttpUtility.HtmlDecode(cNameNode.InnerText.TrimStart().TrimEnd()));
            var cIdNode = _doc.DocumentNode.SelectSingleNode("//div[@id='novel_no']");
            var chapterId = (cIdNode == null) ? string.Empty : cIdNode.InnerText.TrimStart().TrimEnd();

            if (string.IsNullOrEmpty(chapterName))
            {
                Valid = 1;
                _doc = null;
                return;
            }

            if (string.IsNullOrEmpty(chapterId))
            {
                Valid = 2;
                _doc = null;
                return;
            }

            if (chapterName != Name)
            {
                Valid = 3;
                _doc = null;
                return;
            }

            chapterId = chapterId.Substring(0, chapterId.IndexOf("/"));

            if (Convert.ToInt32(chapterId) != Id)
            {
                Valid = 4;
                _doc = null;
                return;
            }

            Valid = 0;
        }

        public void GetChapter(HtmlNode chapterNode = null)
        {
            if (chapterNode != null)
                _doc = new HtmlDocument();

            var chk = 0;
            var pageIndex = 0;

            _footnotes = new HtmlNodeCollection(_doc.DocumentNode);

            if (Settings.Default.IncludeChapterTitle)
            {
                _header = new HtmlNodeCollection(_doc.DocumentNode);
                var hNode1 = HtmlNode.CreateNode($"<p id=\"Lh0\">{Name}</p>");
                var hNode2 = HtmlNode.CreateNode("================================");
                _header.Insert(0, hNode1);
                _header.Insert(1, hNode2);
                DivideInPages(_header, ref chk, ref pageIndex);
                _header = null;
            }

            if (chapterNode == null)
                chapterNode = _doc.DocumentNode.SelectSingleNode("//div[@id='novel_honbun']");

            var lineNodes = chapterNode?.SelectNodes("./p[starts-with(@id, 'L')]");

            if (lineNodes == null)
                return;

            DivideInPages(lineNodes, ref chk, ref pageIndex);

            if (Settings.Default.IncludeAuthorNote)
            {
                var anoteNode = _doc.DocumentNode.SelectSingleNode("//div[@id='novel_a']");
                var aLineNodes = anoteNode?.SelectNodes("./p[starts-with(@id, 'La')]");

                if (aLineNodes != null)
                {
                    var aNode = HtmlNode.CreateNode("<p id=\"La0\">================Author Note================</p>");
                    aLineNodes.Insert(0, aNode);

                    DivideInPages(aLineNodes, ref chk, ref pageIndex);
                }
            }

            _doc = null;

            if (Settings.Default.IncludeFootnotes)
                if (_footnotes.Count > 0)
                    DivideInPages(_footnotes, ref chk, ref pageIndex);

            _footnotes = null;

            Save();
        }

        private Image GetImage(HtmlNode node)
        {
            if (!Settings.Default.DLImages)
                return null;

            var imgNode = node.SelectSingleNode(".//img[@src]");
            if (imgNode == null) return null;

            var nodeHtml = imgNode.OuterHtml;
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
                        line = "================404 - Image Not Found================";
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

                line = HttpUtility.HtmlDecode(line);
                line = line.Replace("　", "");

                if (Settings.Default.RC_ChapterContent)
                    line = Helpers.RemoveCensorship(line);

                if (_emptyPreviousLine && string.IsNullOrEmpty(line))
                {
                    continue;
                }
                else
                {
                    _emptyPreviousLine = string.IsNullOrEmpty(line);

                    //had to do this cause GT counts crlf too but str.Length doesn't
                    var len = string.IsNullOrEmpty(line) ? 1 : line.Length;
                    chk += len;

                    if (chk > Settings.Default.PageMaxLength)
                    {
                        chk = len;
                        pageIndex++;

                        if (!Pages.ContainsKey(pageIndex))
                            Pages[pageIndex] = Pages.New();
                        if (!Images.ContainsKey(pageIndex))
                            Images[pageIndex] = Images.New();
                    }

                    if (Settings.Default.GoogleAPI && Settings.Default.TL_ChapterContent && !string.IsNullOrEmpty(line))
                    {
                        try
                        {
                            TranslationResult result = Main.gClient.TranslateText(line, LanguageCodes.English, LanguageCodes.Japanese);
                            Pages[pageIndex][node.Id] = result.TranslatedText;
                        }
                        catch (Google.GoogleApiException ex)
                        {
                            Pages[pageIndex][node.Id] = line;
                            ChapterError(ex.Message);
                        }
                    }
                    else
                        Pages[pageIndex][node.Id] = line;
                }
            }
        }

        public string ToString(int page)
        {
            if (!Pages.ContainsKey(page))
                return "";

            var txt = new StringBuilder();

            foreach (var line in Pages[page])
                txt.AppendLine(line.Value);

            return txt.ToString();
        }

        public void Save()
        {
            foreach (var page in Pages)
            {
                var chapterFileName = Settings.Default.ChapterFileNameFormat;
                chapterFileName = chapterFileName.Replace("{Page}", page.Key.ToString());
                chapterFileName = Helpers.GenerateFileName(chapterFileName, this);

                if (string.IsNullOrEmpty(chapterFileName))
                    chapterFileName = Name;

                chapterFileName = $"{_chapterPath}\\{Helpers.CheckChars(chapterFileName)}.txt";

                if (!File.Exists(chapterFileName))
                {
                    TextWriter tw = new StreamWriter(chapterFileName);
                    tw.WriteLine(ToString(page.Key));
                    tw.Close();
                }
                else if (File.Exists(chapterFileName))
                    using (var tw = new StreamWriter(chapterFileName, false))
                        tw.WriteLine(ToString(page.Key));
            }

            Pages = null;

            foreach (var page in Images)
            {
                foreach (var image in page.Value)
                {
                    var imagePath = Settings.Default.ImageFileNameFormat;
                    imagePath = imagePath.Replace("{Page}", page.Key.ToString());
                    imagePath = imagePath.Replace("{Id_Image}", image.Key);
                    imagePath = Helpers.GenerateFileName(imagePath, this);

                    if (string.IsNullOrEmpty(imagePath))
                        imagePath = Name;

                    imagePath = $"{_chapterPath}\\{Helpers.CheckChars(imagePath)}.png";
                    image.Value.Save(imagePath, ImageFormat.Png);
                }
            }

            Images = null;
        }

        private bool ChapterExists()
        {
            var res = false;

            var pattern = Settings.Default.ChapterFileNameFormat;
            pattern = pattern.Replace("{Page}", "@Page@");
            pattern = Helpers.GenerateFileName(pattern, this);
            pattern = Helpers.CheckChars(pattern);
            pattern = pattern.Replace("@Page@", @"\d+?") + @"\.txt";
            pattern = Regex.Escape(pattern);

            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentNullException();

            var files = Directory.GetFiles(_chapterPath, "*.txt").Select(Path.GetFileName);

            foreach (var file in files)
            {
                res = Regex.IsMatch(file, pattern, RegexOptions.ExplicitCapture);
                
                if (res)
                    return res;
            }

            return res;
        }

        private void ChapterError(string errorMsg)
        {
            var path = _chapterPath + "\\ChapterError.txt";
            using var tw = new StreamWriter(path, File.Exists(path));
            if (File.Exists(path)) tw.WriteLine();
            tw.WriteLine($"Chapter: {Number} - {Name}");
            tw.WriteLine("Link: " + Link);
            tw.WriteLine("Error message:");
            tw.WriteLine(errorMsg);
        }
    }
}