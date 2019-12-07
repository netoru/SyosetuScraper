﻿using HtmlAgilityPack;
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
        public string Status { get; private set; }
        public DateTime? PublicationDate { get; private set; }
        public DateTime? LatestUpdate { get; private set; }
        public HtmlDocument InfoTopDoc { get; private set; }
        public List<Volume> Volumes { get; private set; }
        public HashSet<string> Tags { get; private set; }
        public HtmlDocument NovelDoc { get; private set; }
        public bool IsValid => (Name != "エラー") ? true : false;
        public string TableOfContents => GetToC();

        private string _novelSavePath = "";

        public Novel(string getNick, string getLink, HtmlDocument getDoc) => (Nickname, Link, NovelDoc) = (getNick, getLink, getDoc);

        public void Setup()
        {
            Volumes = new List<Volume>();

            Name = SearchNovelDoc("//p[@class='novel_title']");
            if (Name == "エラー") return;

            Series = SearchNovelDoc("//p[@class='series_title']");
            
            var auth = SearchNovelDoc("//div[@class='novel_writername']/a", true);
            
            if (auth == "エラー")
            {
                Author = SearchNovelDoc("//div[@class='novel_writername']").Replace("作者：", "");
            }
            else
            {
                var regGroups = Regex.Match(auth, "<a href=\"(?<link>.*)\">(?<author>.*)</a>").Groups;

                if (regGroups.ContainsKey("link"))
                    if (!string.IsNullOrEmpty(regGroups["link"].Value))
                        AuthorLink = regGroups["link"].Value;
                    else
                        AuthorLink = "エラー";

                if (regGroups.ContainsKey("author"))
                    if (!string.IsNullOrEmpty(regGroups["author"].Value))
                        Author = regGroups["author"].Value;
                    else
                        Author = "エラー";
            }

            Description = SearchNovelDoc("//div[@id='novel_ex']");

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

            if (Settings.Default.ScrapeAdditionalNovelInfo || Settings.Default.ScrapeTags)
            {
                var infoTopLink = Link.Replace(Id, "novelview/infotop/ncode/" + Id);
                InfoTopDoc = Scraping.GetPage(infoTopLink);

                if (Settings.Default.ScrapeAdditionalNovelInfo)
                    GetMoreInfo();

                if (Settings.Default.ScrapeTags)
                    GetTags();
            }

            CreateNovelFolder();

            GetNovel();

            CreateIndex();
        }

        private string SearchNovelDoc(string xpath, bool getOut = false)
        {
            var resNode = NovelDoc.DocumentNode.SelectSingleNode(xpath);
            if (resNode == null) return "エラー";
            
            var result = getOut ? resNode.OuterHtml : resNode.InnerText;
            return result.TrimStart().TrimEnd();
        }

        private HtmlNode SearchInfoTopDoc(string searchInnerText, string nodeCollection = "//tr", string returnNode = "td")
        {
            var trNodes = InfoTopDoc.DocumentNode.SelectNodes(nodeCollection);

            foreach (var trNode in trNodes)
                foreach (var item in trNode.ChildNodes)
                    if (item.InnerText == searchInnerText)
                        return trNode.SelectSingleNode(returnNode);

            return HtmlNode.CreateNode("");
        }

        private void GetNovel()
        {
            var indexNode = NovelDoc.DocumentNode.SelectNodes("//div[@class='index_box']");

            if (indexNode == null)
            {
                if (Status == "one-shot")
                    GetOneShot();
                return;
            }

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
                Volumes.Add(new Volume(volIndex, i, volName, Link, _novelSavePath));
                i++;
            }

            if (Volumes.Count == 0)
                Volumes.Add(new Volume(-1, -1, string.Empty, Link, _novelSavePath));

            foreach (var item in Volumes)
            {
                var current = Volumes.IndexOf(item);
                var isLast = current == Volumes.Count() - 1;
                var indexFrom = item.Id + 1;
                var indexTo = isLast ? nodes.Count() - indexFrom : Volumes[current + 1].Id - indexFrom;
                item.GetVolume(nodes.GetRange(indexFrom, indexTo));
                item.Forget();
            }
        }

        private void GetOneShot()
        {
            Volumes.Add(new Volume(-1, 1, Name, Link, _novelSavePath));
            Volumes[0].GetVolume(NovelDoc.DocumentNode.SelectSingleNode("//div[@id='novel_honbun']"));
            Volumes[0].Forget();
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
            var input = SearchInfoTopDoc("キーワード").InnerText;

            if (string.IsNullOrEmpty(input))
                return;

            //Normalize characters like: Ｓｙｏｓｅｔｕ
            //into: Syosetu
            input = input.Normalize(NormalizationForm.FormKC).ToUpper();

            //annoying garbage
            var replaceables = new List<string>() { "\n", "&NBSP;", "　", "・", ".", "/", "(", ")", "\t" };

            foreach (var item in replaceables)
                input = input.Replace(item, " ");

            while (input.Contains("  "))
                input = input.Replace("  ", " ");
            
            var splitter = ' ';

            var originalWords = input.Split(splitter);
            Tags = new HashSet<string>();

            for (var i = 0; i < originalWords.Length; i++)
            {
                if (string.IsNullOrEmpty(originalWords[i]))
                    continue;

                switch ((Settings.Default.ReplaceKnownTags, Scraping.KnownTags.ContainsKey(originalWords[i])))
                {
                    case (true, true):
                        Tags.Add(Scraping.KnownTags[originalWords[i]]);
                        break;
                    case (true, false):
                        Tags.Add(originalWords[i]);
                        Scraping.UnknownTags.Add(originalWords[i]);
                        break;
                    default:
                        Tags.Add(originalWords[i]);
                        break;
                }
            }
        }

        private void GetMoreInfo()
        {
            if (Description == "エラー")
            {
                var tmp = SearchInfoTopDoc("あらすじ");

                var desc = (tmp != null) ? tmp.InnerText : string.Empty;

                if (!string.IsNullOrEmpty(desc))
                    Description = desc;
            }

            var statNode = InfoTopDoc.DocumentNode.SelectSingleNode("//span[@id='noveltype']");

            if (statNode == null)
                statNode = InfoTopDoc.DocumentNode.SelectSingleNode("//span[@id='noveltype_notend']");

            if (statNode != null)
                Status = GetStatus(statNode);

            var chk = SearchInfoTopDoc("掲載日");

            var pDate = (chk != null) ? chk.InnerText : string.Empty;

            if (!string.IsNullOrEmpty(pDate))
                PublicationDate = ConvertJPDate(pDate);

            if (Status == "one-shot")
            {
                LatestUpdate = PublicationDate;
            }
            else
            {
                chk = SearchInfoTopDoc("最新部分掲載日");

                if (chk == null)
                    chk = SearchInfoTopDoc("最終部分掲載日");

                var lUpdate = (chk != null) ? chk.InnerText : string.Empty;

                if (!string.IsNullOrEmpty(lUpdate))
                    LatestUpdate = ConvertJPDate(lUpdate);
            }

            if (!Status.Contains("ongoing"))
                return;

            if (LatestUpdate.HasValue)
                if ((DateTime.Now - LatestUpdate.Value).TotalDays <= Settings.Default.OngoingStatusLength)
                    return;

            Status = Status.Replace("ongoing", "hiatus");

            if ((DateTime.Now - LatestUpdate.Value).TotalDays <= Settings.Default.HiatusStatusLength)
                return;

            Status = Status.Replace("hiatus", "dropped");
        }

        private string GetStatus(HtmlNode node)
        {
            string res;
            switch (node.InnerText)
            {
                case "完結済":
                    res = "completed";
                    break;
                case "連載中":
                    res = "ongoing";
                    break;
                case "短編":
                    res = "one-shot";
                    break;
                default:
                    return string.Empty;
            }

            var chapNode = node.NextSibling;

            if (chapNode == null)
                return res;

            var count = Regex.Match(chapNode.InnerText, "全(\\d+)部分").Groups;

            if (count.Count < 2)
                return res;

            if (string.IsNullOrEmpty(count[1].Value))
                return res;

            return res + $", {count[1].Value} chapters";
        }

        private DateTime ConvertJPDate(string jpDate)
        {
            var pattern = @"(?<Year>\d{4})年.*(?<Month>\d{2})月.*(?<Day>\d{2})日.*(?<Hours>\d{2})時.*(?<Minutes>\d{2})分";
            var res = Regex.Match(jpDate, pattern).Groups;

            return new DateTime(Convert.ToInt32(res["Year"].Value), Convert.ToInt32(res["Month"].Value), 
                Convert.ToInt32(res["Day"].Value), Convert.ToInt32(res["Hours"].Value), Convert.ToInt32(res["Minutes"].Value), 00);
        }

        public override string ToString()
        {
            var txt = new StringBuilder();

            txt.AppendLine("Name: " + Name);
            if (Series != "エラー") txt.AppendLine("Series: " + Series);
            if (Author != "エラー") txt.AppendLine("Author: " + Author);
            txt.AppendLine("Link: " + Link);

            if (!string.IsNullOrEmpty(AuthorLink))
                txt.AppendLine("Author's page: " + AuthorLink);

            if (!string.IsNullOrEmpty(Status)) 
                txt.AppendLine("Status: " + Status);

            if (PublicationDate.HasValue) 
                txt.AppendLine("Publication Date: " + PublicationDate.Value.ToString(Settings.Default.DateTimeFormat));

            if (LatestUpdate.HasValue) 
                txt.AppendLine("Latest Update: " + LatestUpdate.Value.ToString(Settings.Default.DateTimeFormat));

            if (Description != "エラー")
            {
                txt.AppendLine();
                txt.AppendLine("Description:");
                txt.AppendLine(Description);
            }
                
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

        private void CreateNovelFolder()
        {
            _novelSavePath = Settings.Default.SavePath;

            if (!Settings.Default.GetOnlyNovelInfo)
            {
                if (Settings.Default.TypeEqFolder) _novelSavePath += CheckChars(Type) + "\\";
                if (Settings.Default.SeriesEqFolder) _novelSavePath += CheckChars(Series) + "\\";
                if (Settings.Default.AuthorEqFolder) _novelSavePath += CheckChars(Author) + "\\";

                var novelFolderName = Settings.Default.NovelFolderNameFormat;
                novelFolderName = novelFolderName.Replace("{Id}", Id.ToString());
                novelFolderName = novelFolderName.Replace("{Name}", Name);
                novelFolderName = novelFolderName.Replace("{Author}", Author);
                novelFolderName = novelFolderName.Replace("{Type}", Type);
                novelFolderName = novelFolderName.Replace("{Series}", Series);

                if (!string.IsNullOrEmpty(Nickname))
                    novelFolderName = novelFolderName.Replace("{Nickname}", Nickname);
                else
                    novelFolderName = novelFolderName.Replace("{Nickname}", "");

                _novelSavePath += CheckChars(novelFolderName);
                Directory.CreateDirectory(_novelSavePath);
            }
        }

        private void CreateIndex()
        {
            if (!Settings.Default.CreateIndex)
                return;

            var indexFileName = Settings.Default.IndexFileNameFormat;
            indexFileName = indexFileName.Replace("{Id}", Id.ToString());
            indexFileName = indexFileName.Replace("{Name}", Name);
            indexFileName = indexFileName.Replace("{Author}", Author);
            indexFileName = indexFileName.Replace("{Type}", Type);
            indexFileName = indexFileName.Replace("{Series}", Series);

            var path = Settings.Default.SavePath;

            if (Settings.Default.KeepIndexInsideNovelFolder)
                path = _novelSavePath + "\\";

            path += CheckChars(indexFileName);

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