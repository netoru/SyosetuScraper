using Dapper;
using Dapper.Contrib.Extensions;
using Google.Cloud.Translation.V2;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SyosetuScraper
{
    public class Author
    {
        #region Global Variables

        public string SyosetuId { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public List<Novel> Novels { get; set; } = new List<Novel>();
        public HtmlDocument AuthorDoc { get; private set; }
        public DateTime? latestUpdate { get; private set; }
        private string linkToNovels { get; set; }
        private bool isNovelList { get; set; }
        private string linkRoot { get; set; }
        public int SqlId { get; set; } = 0;
        public string tlName { get; set; }

        #endregion

        #region Created

        public Author(string getLink) => Link = getLink;

        public Task Setup(bool scrapeAuthor = true)
        {
            if (SyosetuId != "エラー" && Link != "エラー")
            {
                var matches = Regex.Matches(Link, @"\b\w+\b");

                isNovelList = matches.Count switch
                {
                    5 => false,
                    8 => true,
                    _ => throw new ArgumentNullException(),
                };

                linkRoot = Link.Substring(0, Link.IndexOf('/', 8));

                if (isNovelList)
                {
                    linkToNovels = Link;
                    Link = linkRoot + '/' + SyosetuId + '/';
                }

                AuthorDoc = Helpers.GetPage(Link);

                Name = HttpUtility.HtmlDecode(Helpers.HtmlDoc_GetText(AuthorDoc, "//title"));
                if (Settings.Default.RC_AuthorName) Name = Helpers.RemoveCensorship(Name);
                if (Name == "エラー")
                {
                    Helpers.DeadLink(this, Link, "");
                    return Task.CompletedTask;
                }

                if (!isNovelList)
                {
                    var naviNode = AuthorDoc.DocumentNode.SelectSingleNode("//ul[@id='mypage_navi']/li[2]");    //novel18
                    naviNode ??= AuthorDoc.DocumentNode.SelectSingleNode("//div[@class='sideNav']//li[1]");     //ncode

                    try
                    {
                        linkToNovels = linkRoot + Regex.Match(naviNode.InnerHtml, "\"(.*?)\"").Groups[1].Value;
                    }
                    catch (ArgumentNullException) { throw; }
                }
            }

            GetTranslation();

            if (Settings.Default.ImplementSQL)
                SaveAuthorToDB();

            if (!string.IsNullOrEmpty(tlName) && Settings.Default.GoogleAPI && !Settings.Default.TL_KeepOriginalAsWell)
                Name = tlName;

            if (scrapeAuthor)
                GetListOfNovels(linkToNovels);

            return Task.CompletedTask;
        }

        private void SaveAuthorToDB()
        {
            if (SyosetuId != "エラー" && Link != "エラー")
            {
                DateTime? latestBlogpost = null, latestRelease = null;
                var htmlNode = AuthorDoc.DocumentNode.SelectSingleNode("//li[1]/span[@class='date'][1]/text()[1]");

                if (htmlNode is not null)
                {
                    var chk = DateTime.TryParseExact(htmlNode.InnerText, "yyyy/MM/dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var temp);
                    if (chk) latestBlogpost = temp;
                }

                htmlNode = AuthorDoc.DocumentNode.SelectSingleNode("//li[1]/div[@class='date']/text()[3]");

                AuthorDoc = null;

                if (htmlNode is not null)
                {
                    var chk = DateTime.TryParseExact(htmlNode.InnerText.Replace("投稿日：", ""), "yyyy/MM/dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var temp);
                    if (chk) latestRelease = temp;
                }

                latestUpdate = (latestBlogpost.HasValue, latestRelease.HasValue) switch
                {
                    (false, true) => latestRelease,
                    (true, false) => latestBlogpost,
                    (true, true) => latestBlogpost > latestRelease ? latestBlogpost : latestRelease,
                    _ => null
                };
            }

            //first check if name (and tlname) already exist
            using var conn = Helpers.GetConnection();
            var author = (SQL_Authors)null;
            var nameExists = conn.QueryFirstOrDefault<SQL_Names>("select * from names where name_value = @Nvalue", new { NValue = Name });
            var tlNameExists = (SQL_Names)null;
            
            if (!string.IsNullOrEmpty(tlName))
                tlNameExists = conn.QueryFirstOrDefault<SQL_Names>("select * from names where name_value = @Nvalue", new { NValue = tlName });


            if (nameExists is not null)
            {
                author = conn.QueryFirstOrDefault<SQL_Authors>(
                    @"select * from authors where author_id in (select master_id from relationships
                    where type=3 and slave_id=@SID)", new { SID = nameExists.Name_ID });
            }

            if (author is null)
            {
                author = new SQL_Authors()
                {
                    Author_Code = SyosetuId,
                    Author_Page = Link,
                    AddedOn = DateTime.Now,
                    LastUpdate = latestUpdate,
                    LastScrape = DateTime.Now,
                    User_ID = Environment.UserName
                };

                SqlId = (int)conn.Insert(author);
            }
            else if (author.Author_Code == SyosetuId || (SyosetuId == "エラー" && Link == "エラー") || author.Author_Code.StartsWith("nopage"))
            {
                if (author.Author_Code.StartsWith("nopage"))
                {
                    if (SyosetuId != "エラー") author.Author_Code = SyosetuId;
                    if (Link != "エラー") author.Author_Page = Link;
                }
                if (latestUpdate.HasValue) author.LastUpdate = latestUpdate;
                author.LastScrape = DateTime.Now;
                author.User_ID = Environment.UserName;

                conn.Update(author);
            }
            else if (author.Author_Code != SyosetuId)
            {
                author = new SQL_Authors()
                {
                    Author_Code = SyosetuId,
                    Author_Page = Link,
                    AddedOn = DateTime.Now,
                    LastUpdate = latestUpdate,
                    LastScrape = DateTime.Now,
                    User_ID = Environment.UserName
                };

                SqlId = (int)conn.Insert(author);
            }

            if (SyosetuId == "エラー" && Link == "エラー")
            {
                author.Author_Code = "nopage" + author.Author_ID;
                author.Author_Page = "";
                conn.Update(author);
            }

            if (nameExists is null)
                Helpers.DBRelationships_Name((int)Helpers.NameRanking.JP, author.Author_ID, Name, "Author Name");
            else
                Helpers.DBRelationships_Name((int)Helpers.NameRanking.JP, author.Author_ID, nameExists.Name_ID, "Author Name");

            if (tlNameExists is null)
            {
                if (!string.IsNullOrEmpty(tlName))
                    Helpers.DBRelationships_Name((int)Helpers.NameRanking.Eng, author.Author_ID, tlName, "Author Name");
            }
            else
                Helpers.DBRelationships_Name((int)Helpers.NameRanking.Eng, author.Author_ID, tlNameExists.Name_ID, "Author Name");
        }

        private void GetListOfNovels(string baseUrl)
        {
            AuthorDoc = Helpers.GetPage(baseUrl);

            var linkNodes = AuthorDoc.DocumentNode.SelectNodes("//li[@class='title']/a");

            if (linkNodes is null)
                return;

            foreach (var linkNode in linkNodes)
            {
                try
                {
                    var novelLink = Regex.Match(linkNode.OuterHtml, "\"(.*?)\"").Groups[1].Value;
                    Novels.Add(new Novel("", novelLink));
                }
                catch (ArgumentNullException) { throw; }
            }

            var nodeCollection = AuthorDoc.DocumentNode.SelectNodes("//div[@class='pager_idou']/a");

            AuthorDoc = null;

            if (nodeCollection is null)
                return;

            string nextPage;

            var nextPageNode = nodeCollection[^1];

            if (!HttpUtility.HtmlDecode(nextPageNode.InnerText).StartsWith("Next"))
                return;

            try
            {
                nextPage = HttpUtility.HtmlDecode(Regex.Match(nextPageNode.OuterHtml, "\"(.*?)\"").Groups[1].Value);
            }
            catch (ArgumentNullException) { throw; }

            GetListOfNovels(linkToNovels + nextPage);
        }

        public void GetTranslation()
        {
            if (string.IsNullOrEmpty(tlName) && Settings.Default.GoogleAPI && Settings.Default.TL_AuthorName)
            {
                try
                {
                    TranslationResult result = Main.gClient.TranslateText(Name, LanguageCodes.English, LanguageCodes.Japanese);
                    tlName = result.TranslatedText;
                }
                catch (Google.GoogleApiException ex)
                {
                    tlName = ex.Message;
                }
            }
        }

        #endregion
    }
}