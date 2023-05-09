using Dapper;
using Dapper.Contrib.Extensions;
using Google.Cloud.Translation.V2;
using HtmlAgilityPack;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SyosetuScraper
{
    public class Novel
    {
        #region Global Variables

        public string SyosetuId { get; set; }
        public string Name { get; private set; }
        public string tlName { get; set; }
        public string Nickname { get; private set; }
        public Series NovelSeries { get; private set; }
        public Author NovelAuthor { get; private set; }
        public string Description { get; private set; }
        public string tlDescription { get; set; }
        public string Type { get; private set; }
        public string Link { get; private set; }
        public string Status { get; private set; }
        public DateTime? PublicationDate { get; private set; }
        public DateTime? LatestUpdate { get; private set; }
        public HtmlDocument InfoTopDoc { get; private set; }
        public List<Volume> Volumes { get; private set; } = new List<Volume>();
        public HashSet<string> Tags { get; private set; }
        public HtmlDocument NovelDoc { get; private set; }
        public bool IsValid => Name != "エラー";
        public string TableOfContents => GetToC();
        public int TotalChapters
        {
            get
            {
                var totChapters = 0;
                foreach (var volume in Volumes)
                    totChapters += volume.Chapters.Count;
                return totChapters;
            }
        }

        public string NovelSavePath { get; private set; } = "";
        public int SqlId { get; private set; } = 0;

        #endregion

        #region Created

        public Novel(string getNick, string getLink) => (Nickname, Link) = (getNick, getLink);

        public Task Setup()
        {
            Volumes = new List<Volume>();

            NovelDoc = Helpers.GetPage(Link);

            Name = HttpUtility.HtmlDecode(Helpers.HtmlDoc_GetText(NovelDoc, "//p[@class='novel_title']"));
            if (Settings.Default.RC_NovelTitle) Name = Helpers.RemoveCensorship(Name);
            if (Name == "エラー")
            {
                var _name = string.IsNullOrEmpty(Nickname) ? Name : Nickname;
                Helpers.DeadLink(this, Link, _name);

                return Task.CompletedTask;
            }

            var seriesName = HttpUtility.HtmlDecode(Helpers.HtmlDoc_GetText(NovelDoc, "//p[@class='series_title']"));

            if (seriesName != "エラー")
            {
                if (Settings.Default.RC_SeriesTitle) seriesName = Helpers.RemoveCensorship(seriesName);

                var seriesLinkNode = NovelDoc.DocumentNode.SelectSingleNode("//p[@class='series_title']/a");

                if (seriesLinkNode is not null)
                {
                    var seriesLinkMatch = Regex.Match(seriesLinkNode.OuterHtml, "\"/(.*?)/\"");

                    if (seriesLinkMatch.Groups.Count > 1)
                    {
                        var seriesId = seriesLinkMatch.Groups[1].Value;
                        NovelSeries = new Series(Link.Replace(SyosetuId, seriesId)) { SyosetuId = seriesId, Name = seriesName };
                    }
                }
            }

            var authorName = Helpers.HtmlDoc_GetText(NovelDoc, "//div[@class='novel_writername']/a", true);
            string authorId = "エラー", authorLink = "エラー";

            if (authorName == "エラー")
            {
                authorName = Helpers.HtmlDoc_GetText(NovelDoc, "//div[@class='novel_writername']").Replace("作者：", "");
                //no link author
            }
            else
            {
                var regGroups = Regex.Match(authorName, "<a href=\"(?<link>.*)\">(?<author>.*)</a>").Groups;

                if (regGroups.ContainsKey("link"))
                    if (!string.IsNullOrEmpty(regGroups["link"].Value))
                        authorLink = regGroups["link"].Value;

                if (regGroups.ContainsKey("author"))
                    if (!string.IsNullOrEmpty(regGroups["author"].Value))
                        authorName = regGroups["author"].Value;

                if (!string.IsNullOrEmpty(authorLink))
                {
                    var authorLinkMatches = Regex.Matches(authorLink, @"\b\w+\b");

                    if (authorLinkMatches.Count > 4)
                        authorId = authorLinkMatches[4].Value;
                }
            }

            if (Settings.Default.RC_AuthorName) authorName = Helpers.RemoveCensorship(authorName);

            NovelAuthor = new Author(authorLink) { SyosetuId = authorId, Name = authorName };

            Description = HttpUtility.HtmlDecode(Helpers.HtmlDoc_GetText(NovelDoc, "//div[@id='novel_ex']"));
            if (Settings.Default.RC_NovelDescription) Description = Helpers.RemoveCensorship(Description);

            var groups = Regex.Match(Link, @".+\/(\w+)\.syosetu\.com\/(\w+)\/").Groups;

            try
            {
                Type = groups[1].Value;
                SyosetuId = groups[2].Value;
            }
            catch (IndexOutOfRangeException) { throw; }

            CreateNovelFolder();

            GetNovel();

            var infoTopLink = Link.Replace(SyosetuId, "novelview/infotop/ncode/" + SyosetuId);
            InfoTopDoc = (Settings.Default.AdditionalNovelInfo || Settings.Default.ScrapeTags) ? Helpers.GetPage(infoTopLink) : null;

            if (Settings.Default.AdditionalNovelInfo)
                GetMoreInfo();

            if (Settings.Default.ImplementSQL)
                SaveNovelToDB();

            if (Settings.Default.ScrapeTags)
                GetTags();

            InfoTopDoc = null;

            CreateIndex();

            return Task.CompletedTask;
        }

        private void GetNovel()
        {
            var indexNode = NovelDoc.DocumentNode.SelectNodes("//div[@class='index_box']");

            if (indexNode == null)
            {
                GetOneShot();
                return;
            }

            NovelDoc = null;

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
                Volumes.Add(new Volume(volIndex, i, Helpers.RemoveCensorship(HttpUtility.HtmlDecode(volName)), Link, NovelSavePath));
                i++;
            }

            if (Volumes.Count == 0)
                Volumes.Add(new Volume(-1, -1, string.Empty, Link, NovelSavePath));

            foreach (var item in Volumes)
            {
                var current = Volumes.IndexOf(item);
                var isLast = current == Volumes.Count() - 1;
                var indexFrom = item.Id + 1;
                var indexTo = isLast ? nodes.Count() - indexFrom : Volumes[current + 1].Id - indexFrom;
                item.GetVolume(nodes.GetRange(indexFrom, indexTo));
            }
        }

        private void GetOneShot()
        {
            Volumes.Add(new Volume(-1, 1, Name, Link, NovelSavePath));
            Volumes[0].GetVolume(NovelDoc.DocumentNode.SelectSingleNode("//div[@id='novel_honbun']"));
            NovelDoc = null;
        }

        private string GetToC(bool tlToc = false)
        {
            var toc = new StringBuilder();

            foreach (var volume in Volumes)
                toc.AppendLine(volume.ToString(tlToc));

            return toc.ToString();
        }

        private void GetTags()
        {
            var input = Helpers.HtmlDoc_GetNode(InfoTopDoc, "キーワード").InnerText;

            if (string.IsNullOrEmpty(input))
                return;

            //To change things like &quot; into "
            input = Helpers.RemoveCensorship(HttpUtility.HtmlDecode(input));

            //Normalize characters like: Ｓｙｏｓｅｔｕ
            //into: Syosetu
            input = input.Normalize(NormalizationForm.FormKC).ToUpper();

            //annoying garbage
            var replaceables = new List<string>() { "\n", "&NBSP;", "　", "・",
                ".", "/", "(", ")", "\t", "、", "&" };

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

                Tags.Add(originalWords[i]);
            }

            if (!Settings.Default.ImplementSQL)
                return;

            var interchangeable = new List<string>();

            using var conn = Helpers.GetConnection();
            foreach (var tag in Tags)
            {
                var tag_record = new SQL_Tags
                {
                    Tag_Name = tag,
                    AddedOn = DateTime.Now,
                    FirstNovelToUse = SqlId,
                    LastUpdate = DateTime.Now,
                    User_ID = Environment.UserName
                };

                var res = conn.QueryFirstOrDefault<SQL_Tags>(@"select * from Tags 
                            where Tag_Name = @TName", new { TName = tag });

                if (res is null)
                {
                    try
                    {
                        conn.Insert(tag_record);
                        res = tag_record;
                    }
                    catch (SqlException ex) { if (ex.Number != 2601 && ex.Number != 2627) throw; }

                    //check if the above failed because of concurrent inserts
                    //if so, try retrieving the tag again
                    //if nothing is found throw
                    if (tag_record.Tag_ID < 1)
                    {
                        res = conn.QueryFirstOrDefault<SQL_Tags>(@"select * from Tags 
                            where Tag_Name = @TName", new { TName = tag });

                        if (res is null)
                            throw new ArgumentNullException();
                    }
                }

                tag_record.Tag_ID = res.Tag_ID;

                //on sql dbs katakana and hiragana are treated the same
                if (tag_record.Tag_Name != res.Tag_Name)
                    interchangeable.Add(tag_record.Tag_Name + " " + res.Tag_Name);

                if (res.FirstNovelToUse == -1)
                {
                    res.FirstNovelToUse = SqlId;
                    res.LastUpdate = DateTime.Now;
                    conn.Update(res);
                }

                var relationship = new SQL_Relationships
                {
                    Type = 2,
                    Ranking = 0,
                    Master_ID = SqlId,
                    Slave_ID = tag_record.Tag_ID,
                    AddedOn = DateTime.Now,
                    User_ID = Environment.UserName
                };

                try
                {
                    conn.Insert(relationship);
                }
                catch (SqlException ex) { if (ex.Number != 2601 && ex.Number != 2627) throw; }
            }

            foreach (var kana in interchangeable)
            {
                Tags.Remove(kana.Split(" ")[0]);
                Tags.Add(kana.Split(" ")[1]);
            }
        }

        private void GetMoreInfo()
        {
            if (InfoTopDoc is null)
                return;

            if (Description == "エラー")
            {
                var tmp = Helpers.HtmlDoc_GetNode(InfoTopDoc, "あらすじ");

                if (!string.IsNullOrEmpty(tmp.InnerText)) Description = tmp.InnerText;
                if (Settings.Default.RC_NovelDescription) Description = Helpers.RemoveCensorship(Description);
            }

            var statNode = InfoTopDoc.DocumentNode.SelectSingleNode("//span[@id='noveltype']");

            if (statNode == null)
                statNode = InfoTopDoc.DocumentNode.SelectSingleNode("//span[@id='noveltype_notend']");

            if (statNode != null)
                Status = GetStatus(statNode);

            var chk = Helpers.HtmlDoc_GetNode(InfoTopDoc, "掲載日");

            var pDate = (!string.IsNullOrEmpty(chk.InnerText)) ? Helpers.ConvertJPDate(chk.InnerText) : (DateTime?)null;

            if (pDate.HasValue)
                PublicationDate = pDate.Value;

            if (Status == "one-shot")
            {
                LatestUpdate = PublicationDate;
            }
            else
            {
                chk = Helpers.HtmlDoc_GetNode(InfoTopDoc, "最新部分掲載日");

                if (string.IsNullOrEmpty(chk.InnerText))
                    chk = Helpers.HtmlDoc_GetNode(InfoTopDoc, "最終部分掲載日");

                var lUpdate = (!string.IsNullOrEmpty(chk.InnerText)) ? Helpers.ConvertJPDate(chk.InnerText) : (DateTime?)null;

                if (lUpdate.HasValue)
                    LatestUpdate = lUpdate.Value;
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

        public override string ToString()
        {
            if (Settings.Default.GoogleAPI)
            {
                if (Settings.Default.TL_NovelTitle && Name != "エラー")
                {
                    TranslationResult result = Main.gClient.TranslateText(Name, LanguageCodes.English, LanguageCodes.Japanese);
                    tlName = result.TranslatedText;
                }

                if (Settings.Default.TL_NovelDescription && Description != "エラー")
                {
                    TranslationResult result = Main.gClient.TranslateText(Description, LanguageCodes.English, LanguageCodes.Japanese);
                    tlDescription = result.TranslatedText;
                }
            }

            var txt = new StringBuilder();

            txt.AppendLine("Name: " + Name);

            if (!string.IsNullOrEmpty(tlName) && Settings.Default.GoogleAPI && Settings.Default.TL_KeepOriginalAsWell)
                txt.AppendLine("Name (EN): " + tlName);

            if (NovelSeries is not null)
            {
                txt.AppendLine("Series: " + NovelSeries.Name);
                txt.AppendLine("Series Description: " + NovelSeries.Description);

                if (Settings.Default.GoogleAPI && Settings.Default.TL_KeepOriginalAsWell)
                {
                    NovelSeries.GetTranslation();

                    if (!string.IsNullOrEmpty(NovelSeries.tlName))
                        txt.AppendLine("Series (EN): " + NovelSeries.tlName);
                    if (!string.IsNullOrEmpty(NovelSeries.tlDescription))
                        txt.AppendLine("Series Description (EN): " + NovelSeries.tlDescription);
                }
            }

            if (NovelAuthor.Name != "エラー")
            {
                txt.AppendLine("Author: " + NovelAuthor.Name);

                if (Settings.Default.GoogleAPI && Settings.Default.TL_KeepOriginalAsWell)
                {
                    NovelAuthor.GetTranslation();

                    if (!string.IsNullOrEmpty(NovelAuthor.tlName) && Settings.Default.GoogleAPI && Settings.Default.TL_KeepOriginalAsWell)
                        txt.AppendLine("Author (EN): " + NovelAuthor.tlName);
                }
            }
            
            txt.AppendLine("Link: " + Link);

            if (!string.IsNullOrEmpty(NovelAuthor.Link))
                txt.AppendLine("Author's page: " + NovelAuthor.Link);

            if (!string.IsNullOrEmpty(Status))
                txt.AppendLine("Status: " + Status);
            else
                txt.AppendLine($"Status: unknown, {TotalChapters} chapters");

            if (PublicationDate.HasValue)
                txt.AppendLine("Publication Date: " + PublicationDate.Value.ToString(Settings.Default.DateTimeFormat));

            if (LatestUpdate.HasValue)
                txt.AppendLine("Latest Update: " + LatestUpdate.Value.ToString(Settings.Default.DateTimeFormat));

            if (Description != "エラー")
            {
                txt.AppendLine();
                txt.AppendLine("Description:");
                txt.AppendLine(Description);

                if (Settings.Default.GoogleAPI && Settings.Default.TL_KeepOriginalAsWell)
                {
                    txt.AppendLine("Description (EN):");
                    txt.AppendLine(tlDescription);
                }
            }

            txt.AppendLine();

            if (Settings.Default.ScrapeTags && Tags is not null)
            {
                var tagsLine = "Tags: ";

                for (int i = 0; i < Tags.Count; i++)
                {
                    switch ((Settings.Default.ReplaceKnownTags, Main.KnownTags.ContainsKey(Tags.ElementAt(i))))
                    {
                        case (true, true):
                            tagsLine += Main.KnownTags[Tags.ElementAt(i)];
                            break;
                        case (true, false):
                            tagsLine += Tags.ElementAt(i);
                            if (!Main.UnknownTags.ContainsKey(Tags.ElementAt(i)))
                                Main.UnknownTags.Add(Tags.ElementAt(i), SqlId);
                            break;
                        default:
                            tagsLine += Tags.ElementAt(i);
                            break;
                    }

                    if (i < Tags.Count - 1)
                        tagsLine += ", ";
                }

                txt.AppendLine(tagsLine);
                txt.AppendLine();
            }

            txt.AppendLine("Table of Contents: ");
            txt.AppendLine(TableOfContents);

            if (Settings.Default.GoogleAPI && Settings.Default.TL_KeepOriginalAsWell)
            {
                txt.AppendLine("Table of Contents (EN): ");
                txt.AppendLine(GetToC(true));
            }

            return txt.ToString();
        }

        private void CreateNovelFolder()
        {
            NovelSavePath = Settings.Default.SavePath;

            if (!Settings.Default.OnlyNovelInfo && Settings.Default.CreateFolder)
            {
                if (Settings.Default.CF_Category) NovelSavePath += Helpers.CheckChars(Type) + "\\";
                if (Settings.Default.CF_Series) NovelSavePath += Helpers.CheckChars(NovelSeries.Name) + "\\";
                if (Settings.Default.CF_Author) NovelSavePath += Helpers.CheckChars(NovelAuthor.Name) + "\\";

                var novelFolderName = Helpers.GenerateFileName(Settings.Default.NovelFolderNameFormat, this);

                if (string.IsNullOrEmpty(novelFolderName))
                    novelFolderName = Name;

                NovelSavePath += Helpers.CheckChars(novelFolderName);
                Directory.CreateDirectory(NovelSavePath);
            }
        }

        private void CreateIndex()
        {
            if (!Settings.Default.CreateIndex)
                return;

            var indexFileName = Helpers.GenerateFileName(Settings.Default.IndexFileNameFormat, this);

            var path = Settings.Default.SavePath;

            if (Settings.Default.IndexInNovelFolder)
                path = NovelSavePath + "\\";

            path += Helpers.CheckChars(indexFileName);
            if (!path.EndsWith(".txt")) path += ".txt";

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

        private void SaveNovelToDB()
        {
            //Status: completed, 58 chapters
            var currentStatus = "unknown";
            var totChapters = -1;

            if (!string.IsNullOrEmpty(Status))
            {
                currentStatus = Status.Split(",")[0];

                if (Status != "one-shot")
                    int.TryParse(Status.Split(" ")[1], out totChapters);
                else
                    totChapters = 1;
            }

            if (totChapters == -1)
                totChapters = TotalChapters;

            var novel = new SQL_Novels
            {
                Novel_Code = SyosetuId,
                Novel_Page = Link,
                Novel_Type = Type,
                Novel_Status = currentStatus,
                PublicationDate = PublicationDate,
                Chapters = totChapters,
                AddedOn = DateTime.Now,
                LastUpdate = LatestUpdate,
                LastScrape = DateTime.Now,
                Scrape = true,
                User_ID = Environment.UserName
            };

            //check if novel already exists
            using var conn = Helpers.GetConnection();
            var res = conn.QueryFirstOrDefault<SQL_Novels>(@"select * from Novels 
                where Novel_Code = @NCode", new { NCode = SyosetuId });

            if (res is not null)
            {
                //update if yes
                novel.Novel_ID = SqlId = res.Novel_ID;
                novel.AddedOn = res.AddedOn;
                novel.LastUpdate ??= res.LastUpdate;
                novel.PublicationDate ??= res.PublicationDate;
                novel.Scrape = res.Scrape;
                if (novel.Novel_Status == "unknown") novel.Novel_Status = res.Novel_Status;
                if (res.Translated_Chapters != 0) novel.Translated_Chapters = res.Translated_Chapters;
                novel.Previous_Chapters = res.Chapters;

                conn.Update(novel);
            }
            else
            {
                //insert if not
                SqlId = (int)conn.Insert(novel);
            }

            var nameExists = conn.QueryFirstOrDefault<SQL_Names>("select * from names where name_value = @Nvalue", new { NValue = Name });
            var nicknameExists = (SQL_Names)null;
            var tlNameExists = (SQL_Names)null;

            if (!string.IsNullOrEmpty(Nickname))
                nicknameExists = conn.QueryFirstOrDefault<SQL_Names>("select * from names where name_value = @Nvalue", new { NValue = Nickname });
            if (!string.IsNullOrEmpty(tlName))
                tlNameExists = conn.QueryFirstOrDefault<SQL_Names>("select * from names where name_value = @Nvalue", new { NValue = tlName });

            if (nameExists is null)
                Helpers.DBRelationships_Name((int)Helpers.NameRanking.JP, novel.Novel_ID, Name, "Novel Name");
            else
                Helpers.DBRelationships_Name((int)Helpers.NameRanking.JP, novel.Novel_ID, nameExists.Name_ID, "Novel Name");

            if (nicknameExists is null)
            {
                if (!string.IsNullOrEmpty(Nickname))
                    Helpers.DBRelationships_Name((int)Helpers.NameRanking.Nickname, novel.Novel_ID, Nickname, "Novel Name", true);
            }
            else
                Helpers.DBRelationships_Name((int)Helpers.NameRanking.Nickname, novel.Novel_ID, nicknameExists.Name_ID, "Novel Name", true);

            if (tlNameExists is null)
            {
                if (!string.IsNullOrEmpty(tlName))
                    Helpers.DBRelationships_Name((int)Helpers.NameRanking.Eng, novel.Novel_ID, tlName, "Novel Name");
            }
            else
                Helpers.DBRelationships_Name((int)Helpers.NameRanking.Eng, novel.Novel_ID, tlNameExists.Name_ID, "Novel Name");

            NovelAuthor.Setup(false);
            if (NovelSeries is not null) NovelSeries.Setup(false);
            DBRelationships_Parents();
        }

        private void DBRelationships_Parents()
        {
            using var conn = Helpers.GetConnection();

            var authRelExists = conn.QueryFirstOrDefault<SQL_Relationships>(
                @"select * from Relationships where Type = 4 and 
                Ranking = 0 and Master_ID = @MID and Slave_ID = @SID",
                new { MID = NovelAuthor.SqlId, SID = SqlId });

            if (authRelExists is null)
            {
                authRelExists = new SQL_Relationships()
                {
                    Type = 4,
                    Ranking = 0,
                    Master_ID = NovelAuthor.SqlId,
                    Slave_ID = SqlId,
                    AddedOn = DateTime.Now,
                    User_ID = Environment.UserName
                };

                conn.Insert(authRelExists);
            }

            if (NovelSeries is null)
                return;

            var seriNovRelExists = conn.QueryFirstOrDefault<SQL_Relationships>(
                @"select * from Relationships where Type = 7 and 
                Ranking = 0 and Master_ID = @MID and Slave_ID = @SID",
                new { MID = NovelSeries.SqlId, SID = SqlId });

            if (seriNovRelExists is null)
            {
                seriNovRelExists = new SQL_Relationships()
                {
                    Type = 7,
                    Ranking = 0,
                    Master_ID = NovelSeries.SqlId,
                    Slave_ID = SqlId,
                    AddedOn = DateTime.Now,
                    User_ID = Environment.UserName
                };

                conn.Insert(seriNovRelExists);
            }

            var seriAuthRelExists = conn.QueryFirstOrDefault<SQL_Relationships>(
                @"select * from Relationships where Type = 5 and 
                Ranking = 0 and Master_ID = @MID and Slave_ID = @SID",
                new { MID = NovelAuthor.SqlId, SID = NovelSeries.SqlId });

            if (seriAuthRelExists is null)
            {
                seriAuthRelExists = new SQL_Relationships()
                {
                    Type = 5,
                    Ranking = 0,
                    Master_ID = NovelAuthor.SqlId,
                    Slave_ID = NovelSeries.SqlId,
                    AddedOn = DateTime.Now,
                    User_ID = Environment.UserName
                };

                conn.Insert(seriAuthRelExists);
            }
        }

        #endregion
    }
}