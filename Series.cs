using Dapper;
using Dapper.Contrib.Extensions;
using Google.Cloud.Translation.V2;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Web;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SyosetuScraper
{
    public class Series
    {
        #region Global Variables

        public string SyosetuId { get; set; }
        public string Name { get; set; }
        public string tlName { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string tlDescription { get; set; }
        public List<Novel> Novels { get; set; } = new List<Novel>();
        public HtmlDocument SeriesDoc { get; private set; }
        public DateTime? latestUpdate { get; set; }
        public int SqlId { get; set; } = 0;

        #endregion

        #region Created

        public Series(string getLink) => Link = getLink;

        public Task Setup(bool scrapeSeries = true)
        {
            SeriesDoc = Helpers.GetPage(Link);

            Name = HttpUtility.HtmlDecode(Helpers.HtmlDoc_GetText(SeriesDoc, "//title"));
            if (Settings.Default.RC_SeriesTitle) Name = Helpers.RemoveCensorship(Name);
            if (Name == "エラー")
            {
                Helpers.DeadLink(this, Link, "");
                return Task.CompletedTask;
            }

            Description = HttpUtility.HtmlDecode(Helpers.HtmlDoc_GetText(SeriesDoc, "//div[@class='novel_ex']"));
            if (Settings.Default.RC_SeriesDescription) Description = Helpers.RemoveCensorship(Description);
            if (Description == "エラー") Description = "";

            GetTranslation();

            if (Settings.Default.ImplementSQL)
                SaveSeriesToDB();

            if (Settings.Default.GoogleAPI && !Settings.Default.TL_KeepOriginalAsWell)
            {
                if (!string.IsNullOrEmpty(tlName))
                    Name = tlName;

                if (!string.IsNullOrEmpty(tlDescription))
                    Description = tlDescription;
            }

            if (scrapeSeries)
                GetListOfNovels();

            SeriesDoc = null;

            return Task.CompletedTask;
        }

        private void SaveSeriesToDB()
        {
            var htmlNodeCollection = SeriesDoc.DocumentNode.SelectNodes("//div[@class='novel_info'][1]");

            foreach (var htmlNode in htmlNodeCollection)
            {
                try
                {
                    var novelDate = Helpers.ConvertJPDate(htmlNode.InnerText, false);
                    latestUpdate ??= novelDate;
                    if (latestUpdate < novelDate) latestUpdate = novelDate;
                }
                catch (FormatException) { }
            }

            using var conn = Helpers.GetConnection();
            var series = (SQL_Series)null;
            var nameExists = conn.QueryFirstOrDefault<SQL_Names>("select * from names where name_value = @Nvalue", new { NValue = Name });
            var tlNameExists = (SQL_Names)null;

            if (!string.IsNullOrEmpty(tlName))
                tlNameExists = conn.QueryFirstOrDefault<SQL_Names>("select * from names where name_value = @Nvalue", new { NValue = tlName });


            if (nameExists is not null)
            {
                series = conn.QueryFirstOrDefault<SQL_Series>(
                    @"select * from series where series_id in (select master_id from relationships
                    where type=6 and slave_id=@SID)", new { SID = nameExists.Name_ID });
            }

            if (series is null)
            {
                series = new SQL_Series()
                {
                    Series_Code = SyosetuId,
                    Series_Page = Link,
                    AddedOn = DateTime.Now,
                    LastUpdate = latestUpdate,
                    LastScrape = DateTime.Now,
                    User_ID = Environment.UserName
                };

                SqlId = (int)conn.Insert(series);
            }
            else if (series.Series_Code == SyosetuId)
            {
                if (latestUpdate.HasValue) series.LastUpdate = latestUpdate;
                series.LastScrape = DateTime.Now;
                series.User_ID = Environment.UserName;

                conn.Update(series);
            }
            else if (series.Series_Code != SyosetuId)
            {
                series = new SQL_Series()
                {
                    Series_Code = SyosetuId,
                    Series_Page = Link,
                    AddedOn = DateTime.Now,
                    LastUpdate = latestUpdate,
                    LastScrape = DateTime.Now,
                    User_ID = Environment.UserName
                };

                SqlId = (int)conn.Insert(series);
            }

            if (nameExists is null)
                Helpers.DBRelationships_Name((int)Helpers.NameRanking.JP, series.Series_ID, Name, "Series Name");
            else
                Helpers.DBRelationships_Name((int)Helpers.NameRanking.JP, series.Series_ID, nameExists.Name_ID, "Series Name");

            if (tlNameExists is null)
            {
                if (!string.IsNullOrEmpty(tlName))
                    Helpers.DBRelationships_Name((int)Helpers.NameRanking.Eng, series.Series_ID, tlName, "Series Name");
            }
            else
                Helpers.DBRelationships_Name((int)Helpers.NameRanking.Eng, series.Series_ID, tlNameExists.Name_ID, "Series Name");
        }

        private void GetListOfNovels()
        {
            var linkNodes = SeriesDoc.DocumentNode.SelectNodes("//div[@class='title']/a");

            if (linkNodes is null)
                return;

            foreach (var linkNode in linkNodes)
            {
                try
                {
                    var novelId = Regex.Match(linkNode.OuterHtml, "\"/(.*?)/\"").Groups[1].Value;
                    novelId = Link.Replace(SyosetuId, novelId);
                    Novels.Add(new Novel("", novelId));
                }
                catch (ArgumentNullException) { throw; }
            }
        }

        public void GetTranslation()
        {
            if (Settings.Default.GoogleAPI)
            {
                if (Settings.Default.TL_SeriesTitle && !string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(tlName))
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

                if (Settings.Default.TL_SeriesDescription && !string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(tlDescription))
                {
                    try
                    {
                        TranslationResult result = Main.gClient.TranslateText(Description, LanguageCodes.English, LanguageCodes.Japanese);
                        tlDescription = result.TranslatedText;
                    }
                    catch (Google.GoogleApiException ex)
                    {
                        tlDescription = ex.Message;
                    }
                }
            }
        }

        #endregion
    }
}
