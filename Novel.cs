using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
        public string Author { get; private set; }
        public string Description { get; private set; }
        public string Type { get; private set; }
        public string Link { get; }
        public string TableOfContents => GetToC();
        public List<Volume> Volumes { get; } = new List<Volume>();

        public bool IsValid => (Name != "エラー") ? true : false;

        public Novel(string link, HtmlDocument doc)
        {
            Link = link;
            Name = GetName(doc);

            if (!IsValid)
                return;

            var details = GetDetailsAsync(doc);
            var novel = GetNovelAsync(doc);

            details.Wait();
            novel.Wait();
        }

        private static string GetName(HtmlDocument doc)
        {
            var nameNode = doc.DocumentNode.SelectSingleNode("//p[@class='novel_title']");
            return (nameNode == null) ? "エラー" : nameNode.InnerText.TrimStart().TrimEnd();
        }

        private async Task GetDetailsAsync(HtmlDocument doc)
        {
            var seriesNode = doc.DocumentNode.SelectSingleNode("//p[@class='series_title']");
            Series = (seriesNode == null) ? string.Empty : seriesNode.InnerText.TrimStart().TrimEnd();

            var authorNode = doc.DocumentNode.SelectSingleNode("//div[@class='novel_writername']");
            Author = (authorNode == null) ? string.Empty : authorNode.InnerText.TrimStart().TrimEnd().Replace("作者：", "");

            var descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@id='novel_ex']");
            Description = (descriptionNode == null) ? string.Empty : descriptionNode.InnerText.TrimStart().TrimEnd();

            var groups = Regex.Match(Link, @".+\/(\w+)\.syosetu\.com\/(\w+)\/").Groups;

            if (groups.Count != 3)
                return;

            Type = groups[1].Value;
            Id = groups[2].Value;
        }

        private async Task GetNovelAsync(HtmlDocument doc)
        {
            var indexNode = doc.DocumentNode.SelectNodes("//div[@class='index_box']");

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

        public override string ToString()
        {
            var txt = new StringBuilder();

            txt.AppendLine("Name: " + Name);
            if (!string.IsNullOrEmpty(Series)) txt.AppendLine("Series: " + Series);
            txt.AppendLine("Author: " + Author);
            txt.AppendLine("Link: " + Link);
            txt.AppendLine();
            txt.AppendLine("Description:");
            txt.AppendLine(Description);
            txt.AppendLine();
            txt.AppendLine("Table of Contents: ");
            txt.AppendLine(TableOfContents);

            return txt.ToString();
        }

        
        //instead of being here, consider moving save to the Novel class
        private Task Save(bool CreateFoldersForEachVolume = true)
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

        private static string CheckChars(string input)
        {
            //Check for illegal characters
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(input, "□");
        }
    }
}
