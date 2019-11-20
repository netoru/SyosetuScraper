using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SyosetuScraper
{
    class Novel
    {
        public string Id { get; private set; }
        public string Series { get; private set; }
        public string Name { get; private set; }
        public string Nickname { get; private set; }
        public string Author { get; private set; }
        public string Description { get; private set; }
        public string Type { get; private set; }
        public string Link { get; private set; }
        public string AuthorLink { get; private set; }
        public string InfotopLink { get; private set; }
        public List<Volume> Volumes { get; private set; }
        public HashSet<string> Tags { get; private set; }
        public HtmlDocument _doc { get; private set; }
        public bool IsValid => (Name != "エラー") ? true : false;
        public string TableOfContents => GetToC();

        public Novel(string getNick, string getLink, HtmlDocument getDoc) => (Nickname, Link, _doc) = (getNick, getLink, getDoc);

        public void Setup()
        {
            Volumes = new List<Volume>();

            Name = SearchDoc("//p[@class='novel_title']");
            Series = SearchDoc("//p[@class='series_title']");
            Author = SearchDoc("//div[@class='novel_writername']", true);
            Description = SearchDoc("//div[@id='novel_ex']");

            var groups = Regex.Match(Link, @".+\/(\w+)\.syosetu\.com\/(\w+)\/").Groups;

            try
            {
                Type = groups[1].Value;
                Id = groups[2].Value;
            }
            catch (IndexOutOfRangeException)
            {
                throw;
            }

            if (Settings.Default.ScrapeTags)            
                GetTags();            

            GetNovel();
        }

        private string SearchDoc(string xpath, bool repStr = false, string oldStr = "作者：", string newStr = "")
        {
            var resNode = _doc.DocumentNode.SelectSingleNode(xpath);
            var result = (resNode == null) ? "エラー" : resNode.InnerText.TrimStart().TrimEnd();
            result = repStr ? result.Replace(oldStr, newStr) : result;
            return result;
        }

        private void GetNovel()
        {
            var indexNode = _doc.DocumentNode.SelectNodes("//div[@class='index_box']");

            if (indexNode == null)
                return;

            var nodes = indexNode.First().ChildNodes
                .Where(n => n.Name == "div" || n.Name == "dl").ToList();
            
            var i = 1;
            foreach (var node in nodes)
            {
                if (node == null)
                    continue;
                if (node.Name == "dl")
                    continue;
                var volName = node.InnerText.TrimStart().TrimEnd();
                var volIndex = nodes.IndexOf(node);
                Volumes.Add(new Volume(volIndex, i, volName, Link));
                i++;
            }

            if (Volumes.Count == 0)
                Volumes.Add(new Volume(-1, -1, string.Empty, Link));

            foreach (var item in Volumes)
            {
                var current = Volumes.IndexOf(item);
                var isLast = current == Volumes.Count() - 1;
                var indexFrom = item.Id + 1;
                var indexTo = isLast ? nodes.Count() - indexFrom : Volumes[current + 1].Id - indexFrom;
                item.GetVolume(nodes.GetRange(indexFrom, indexTo));
            }
        }

        private string GetToC()
        {
            var toc = new StringBuilder();

            foreach (var volume in Volumes)
                toc.AppendLine(volume.ToString());

            return toc.ToString();
        }

        private void GetTags()
        {
            var infoTopLink = Link.Replace(Id, "novelview/infotop/ncode/" + Id);
            var infoTopDoc = Scraping.GetPage(infoTopLink);

            var trNodes = infoTopDoc.DocumentNode.SelectNodes("//tr");
            HtmlNode tdNode = HtmlNode.CreateNode("");

            foreach (var trNode in trNodes)
                foreach (var item in trNode.ChildNodes)
                    if (item.InnerText == "キーワード")
                    {
                        tdNode = trNode.SelectSingleNode("td");
                        break;
                    }

            var input = tdNode.InnerText;

            if (string.IsNullOrEmpty(input))
                return;

            input = input.Replace("\n", " ");
            input = input.Replace("&nbsp;", " ");
            input = input.Replace("　", " ");

            //・
            //.
            // /


            while (input.Contains("  "))
                input = input.Replace("  ", " ");
            
            var splitter = ' ';

            var originalWords = input.Split(splitter);
            Tags = new HashSet<string>();

            for (var i = 0; i < originalWords.Length; i++)
            {
                if (string.IsNullOrEmpty(originalWords[i]))
                    continue;

                if (Settings.Default.ReplaceKnownTags && Scraping.KnownTags.ContainsKey(originalWords[i]))
                    Tags.Add(Scraping.KnownTags[originalWords[i]]);
                else
                    Tags.Add(originalWords[i]);
            }
        }

        public override string ToString()
        {
            var txt = new StringBuilder();

            txt.AppendLine("Name: " + Name);
            if (Series != "エラー") txt.AppendLine("Series: " + Series);
            txt.AppendLine("Author: " + Author);
            txt.AppendLine("Link: " + Link);
            txt.AppendLine();
            txt.AppendLine("Description:");
            txt.AppendLine(Description);
            txt.AppendLine();

            if (Settings.Default.ScrapeTags)
            {
                var tagsLine = "Tags: ";

                for (int i = 0; i < Tags.Count; i++)
                {
                    tagsLine += Tags.ElementAt(i);

                    if (i < Tags.Count - 1)
                        tagsLine += ", ";
                }

                txt.AppendLine(tagsLine);
                txt.AppendLine();
            }

            txt.AppendLine("Table of Contents: ");
            txt.AppendLine(TableOfContents);

            return txt.ToString();
        }

        public void Save()
        {
            string path = Settings.Default.SavePath;
            
            if (!Settings.Default.GetOnlyNovelInfo)
            {
                if (Settings.Default.TypeEqFolder) path += CheckChars(Type) + "\\";
                if (Settings.Default.SeriesEqFolder) path += CheckChars(Series) + "\\";
                if (Settings.Default.AuthorEqFolder) path += CheckChars(Author) + "\\";

                var novelPath = Settings.Default.NovelFolderNameFormat;
                novelPath = novelPath.Replace("{Id}", Id.ToString());
                novelPath = novelPath.Replace("{Name}", Name);
                novelPath = novelPath.Replace("{Author}", Author);
                novelPath = novelPath.Replace("{Type}", Type);
                novelPath = novelPath.Replace("{Series}", Series);

                if (!string.IsNullOrEmpty(Nickname))
                    novelPath = novelPath.Replace("{Nickname}", Nickname);
                else
                    novelPath = novelPath.Replace("{Nickname}", "");

                path += CheckChars(novelPath);
                Directory.CreateDirectory(path);

                foreach (var volume in Volumes)
                    volume.Save(path);
            }

            if (!Settings.Default.CreateIndex)
                return;

            var indexPath = Settings.Default.IndexFileNameFormat;
            indexPath = indexPath.Replace("{Id}", Id.ToString());
            indexPath = indexPath.Replace("{Name}", Name);
            indexPath = indexPath.Replace("{Author}", Author);
            indexPath = indexPath.Replace("{Type}", Type);
            indexPath = indexPath.Replace("{Series}", Series);

            if (!Settings.Default.KeepIndexInsideNovelFolder)
                path = Settings.Default.SavePath;
            else
                path += "\\";

            path += CheckChars(indexPath);

            if (!File.Exists(path))
            {
                TextWriter tw = new StreamWriter(path);
                tw.WriteLine(ToString());
                tw.Close();
            }
            else if (File.Exists(path))
                using (var tw = new StreamWriter(path, false))
                    tw.WriteLine(ToString());
        }

        public static string CheckChars(string input)
        {
            //Check for illegal characters
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(input, "□");
        }
    }
}