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

            GetDetails(doc);
            GetNovel(doc);
        }

        private string GetName(HtmlDocument doc)
        {
            var nameNode = doc.DocumentNode.SelectSingleNode("//p[@class='novel_title']");
            return (nameNode == null) ? "エラー" : nameNode.InnerText.TrimStart().TrimEnd();
        }

        private void GetDetails(HtmlDocument doc)
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

        private void GetNovel(HtmlDocument doc)
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
    }
}
