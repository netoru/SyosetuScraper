﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public List<Volume> Volumes { get; } = new List<Volume>();
        private HtmlDocument _doc { get; set; }
        public bool IsValid => (Name != "エラー") ? true : false;
        public string TableOfContents => GetToC();

        public Novel(string getLink, HtmlDocument getDoc) => (Link, _doc) = (getLink, getDoc);

        public async Task SetupAsync()
        {
            Trace.WriteLine("1");
            Name = await SearchDocAsync("//p[@class='novel_title']");
            Trace.WriteLine("2");
            Series = await SearchDocAsync("//p[@class='series_title']");
            Trace.WriteLine("3");
            Author = await SearchDocAsync("//div[@class='novel_writername']", "作者：");
            Trace.WriteLine("4");
            Description = await SearchDocAsync("//div[@id='novel_ex']");
            Trace.WriteLine("5");
            //await Task.Run(GetDetailsAsync); //why does this get stuck :(
            Trace.WriteLine("6");
            //await GetNovelAsync();
            Trace.WriteLine("7");
        }

        private async Task<string> SearchDocAsync(string xpath, string repl = "", string repWith = "")
        {
            var resNode = _doc.DocumentNode.SelectSingleNode(xpath);//await Task.Run(() => )
            var result = (resNode == null) ? "エラー" : resNode.InnerText.TrimStart().TrimEnd();//.Replace(repl, repWith)
            return result;
        }

        private async Task GetDetailsAsync()
        {
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
        }

        private async Task GetNovelAsync()
        {
            var indexNode = _doc.DocumentNode.SelectNodes("//div[@class='index_box']");

            if (indexNode == null)
                return;

            var nodes = indexNode.First().ChildNodes
                .Where(n => n.Name == "div" || n.Name == "dl").ToList();

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                    continue;
                if (nodes[i].Name == "dl")
                    continue;
                var volName = nodes[i].InnerText.TrimStart().TrimEnd();
                var volIndex = nodes.IndexOf(nodes[i]);
                Volumes.Add(new Volume(volIndex, i, volName, Link));
            }

            if (Volumes.Count == 0)
                Volumes.Add(new Volume(-1, -1, string.Empty, Link));

            await Task.Run(() => {
                foreach (var item in Volumes)
                {
                    var current = Volumes.IndexOf(item);
                    var isLast = current == Volumes.Count() - 1;
                    var indexFrom = item.Id + 1;
                    var indexTo = isLast ? nodes.Count() - indexFrom : Volumes[current + 1].Id - indexFrom;
                    item.GetVolume(nodes.GetRange(indexFrom, indexTo));
                }
            });
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

        
        public void Save(bool CreateFoldersForEachVolume = true)
        {
            //add handling to save somewhere else
            string path = Scraping.SavePath + Type + "\\" + CheckChars(Name);
            Directory.CreateDirectory(path);

            var indexPath = path + "\\_Index.txt";
            if (!File.Exists(indexPath))
            {
                TextWriter tw = new StreamWriter(indexPath);
                tw.WriteLine(ToString());
                tw.Close();
            }
            else if (File.Exists(indexPath))
                using (var tw = new StreamWriter(indexPath, false))
                    tw.WriteLine(ToString());

            foreach (var volume in Volumes)
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
