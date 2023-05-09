using Dapper;
using Dapper.Contrib.Extensions;
using HtmlAgilityPack;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SyosetuScraper
{
    public class Helpers
    {
        public enum NameRanking
        {
            Nickname = 0,
            Eng = 1,
            Romaji = 2,
            JP = 3
        }

        public static CookieContainer SyousetsuCookie { get; } = new CookieContainer();

        public static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(Settings.Default.DBConnectionString);
            try
            {
                conn.Open();
            }
            catch (SqlException)
            {
                conn = null;
            }
            catch (InvalidOperationException)
            {
                conn = null;
            }

            return conn;
        }

        public static SqlConnection GetConnection(string connection)
        {
            var conn = new SqlConnection(connection);
            conn.Open();
            return conn;
        }

        public static HtmlDocument GetPage(string link)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(link);
                request.Method = "GET";
                request.CookieContainer = SyousetsuCookie;
                //useragent is needed else the getresponse returns 403 forbidden
                request.UserAgent = "definitely-not-a-screen-scraper";
                var response = (HttpWebResponse)request.GetResponse();
                var stream = response.GetResponseStream();

                var doc = new HtmlDocument();
                using (var reader = new StreamReader(stream))
                {
                    string html = reader.ReadToEnd();
                    doc.LoadHtml(html);
                }

                return doc;
            }
            catch (WebException)
            {
                var html = new StringBuilder();
                html.Append("<html>");
                html.Append("<head>");
                html.Append("<title>エラー</title>");
                html.Append("</head>");
                html.Append("<body>");
                html.Append("</body>");
                html.Append("</html>");

                var doc = new HtmlDocument();
                doc.LoadHtml(html.ToString());
                return doc;
            }
        }

        public static string GenerateFileName(string fileNameFormat, object parent)
        {
            var res = fileNameFormat;
            foreach (Match mtch in Regex.Matches(res, "{(.*?)}"))
            {
                var propInfo = parent.GetType().GetProperty(mtch.Groups[1].Value);
                var replacement = "";

                if (propInfo is not null)
                    replacement = propInfo.GetValue(parent, null).ToString();

                if (replacement is null)
                    replacement = "";

                res = res.Replace(mtch.Value, replacement);
            }
            return res;
        }

        public static string RemoveCensorship(string input)
        {
            if (!Settings.Default.RemoveCensorship)
                return input;

            var katakana = Settings.Default.MaruToKatakanaN.Split(";");

            foreach (var item in katakana)
                if (!string.IsNullOrEmpty(item))
                    input = Regex.Replace(input, item, item.Replace('.', 'ン'));

            var hiragana = Settings.Default.MaruToHiraganaN.Split(";");

            foreach (var item in hiragana)
                if (!string.IsNullOrEmpty(item))
                    input = Regex.Replace(input, item, item.Replace('.', 'ん'));

            return input;
        }

        public static string HtmlDoc_GetText(HtmlDocument htmlDoc, string xpath, bool getOuterHtml = false)
        {
            var resNode = htmlDoc.DocumentNode.SelectSingleNode(xpath);
            if (resNode == null) return "エラー";

            var result = getOuterHtml ? resNode.OuterHtml : resNode.InnerText;
            return result.TrimStart().TrimEnd();
        }

        public static HtmlNode HtmlDoc_GetNode(HtmlDocument htmlDoc, string searchInnerText, string nodeCollection = "//tr", string returnNode = "td")
        {
            var res = HtmlNode.CreateNode("<p></p>");

            if (htmlDoc is null)
                return res;

            var trNodes = htmlDoc.DocumentNode.SelectNodes(nodeCollection);

            if (trNodes != null)
                foreach (var trNode in trNodes)
                    foreach (var item in trNode.ChildNodes)
                        if (item.InnerText == searchInnerText)
                            return trNode.SelectSingleNode(returnNode);

            return res;
        }

        public static string CheckChars(string input)
        {
            //Check for illegal characters
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(input, "□");
        }

        public static void DeadLink(object parent, string link, string name)
        {
            var deadLinkFileName = GenerateFileName(Settings.Default.DeadLinkFileNameFormat, parent);

            if (string.IsNullOrEmpty(deadLinkFileName))
                deadLinkFileName = "Dead Links";

            deadLinkFileName = Settings.Default.SavePath + CheckChars(deadLinkFileName) + ".txt";

            if (!File.Exists(deadLinkFileName))
            {
                TextWriter tw = new StreamWriter(deadLinkFileName);
                tw.WriteLine(link + ";" + name);
                tw.Close();
            }
            else if (File.Exists(deadLinkFileName))
                using (var tw = new StreamWriter(deadLinkFileName, true))
                    tw.WriteLine(link + ";" + name);
        }

        public static DateTime ConvertJPDate(string jpDate, bool getTime = true)
        {
            var pattern = @".*(?<Year>\d{4})年.*(?<Month>\d{2})月.*(?<Day>\d{2})日.*";
            if (getTime) pattern += @"(?<Hours>\d{2})時.*(?<Minutes>\d{2})分.*";
            var res = Regex.Match(jpDate, pattern).Groups;

            if (getTime)
                return new DateTime(Convert.ToInt32(res["Year"].Value), Convert.ToInt32(res["Month"].Value), Convert.ToInt32(res["Day"].Value),
                    Convert.ToInt32(res["Hours"].Value), Convert.ToInt32(res["Minutes"].Value), 00);

            return new DateTime(Convert.ToInt32(res["Year"].Value), Convert.ToInt32(res["Month"].Value), Convert.ToInt32(res["Day"].Value));
        }

        public static void DBRelationships_Name(int rank, int master_ID, string nameValue, string relTypeName, bool noInsertOnSameRank = false)
        {
            using var conn = GetConnection();

            var relType = conn.QuerySingleOrDefault<int?>(
                @"select RelType_ID from RelTypes
                where RelType_Description = @relTypeName", new { relTypeName });

            if (!relType.HasValue)
                throw new ArgumentNullException();

            var names = conn.Query(
                @"select Ranking, Name_Value from Relationships
                left join Names on Name_ID=Slave_ID
                where Type=@relType and Master_ID = @master_ID
                order by 1 desc", new { relType, master_ID })
                .ToDictionary(row => (int)row.Ranking, row => (string)row.Name_Value);

            var nameRecord = new SQL_Names
            {
                Name_Value = nameValue,
                AddedOn = DateTime.Now,
                User_ID = Environment.UserName
            };

            var relName = new SQL_Relationships
            {
                Type = relType.Value,
                Ranking = rank,
                Master_ID = master_ID,
                AddedOn = DateTime.Now,
                User_ID = Environment.UserName
            };

            var isNameAlreadySaved = false;

            if (names.Count != 0)
            {
                var max_id = names.Keys.Max();

                foreach (var pair in names)
                    if (!isNameAlreadySaved) isNameAlreadySaved = pair.Value == nameValue;

                while (names.Keys.Contains(relName.Ranking))
                {
                    if (max_id < (int)NameRanking.JP)
                        max_id = (int)NameRanking.JP;

                    max_id++;
                    relName.Ranking = max_id;
                }
            }

            if (!isNameAlreadySaved)
            {
                relName.Slave_ID = (int)conn.Insert(nameRecord);
                conn.Insert(relName);
            }
        }

        public static void DBRelationships_Name(int rank, int master_ID, int slave_ID, string relTypeName, bool noInsertOnSameRank = false)
        {
            using var conn = GetConnection();

            var relType = conn.QuerySingleOrDefault<int?>(
                @"select RelType_ID from RelTypes
                where RelType_Description = @relTypeName", new { relTypeName });

            if (!relType.HasValue)
                throw new ArgumentNullException();

            var relExists = conn.QueryFirstOrDefault<SQL_Relationships>(@"select * from relationships
                where type = @Value and master_id = @master_id and slave_id = @slave_id",
                new { relType.Value, master_ID, slave_ID});

            if (relExists is not null)
                return;

            //check if chosen ranking is already in use
            var rankIsAvailable = conn.QueryFirstOrDefault<SQL_Relationships>(
                @"select * from relationships where type = @Value 
                and master_id = @master_id and ranking = @rank",
                new { relType.Value, master_ID, rank });

            if (rankIsAvailable is not null)
            {
                var maxRank = conn.QueryFirstOrDefault<int>(@"select max(ranking)
                    from relationships where type = @Value and master_id = @master_id",
                    new { relType.Value, master_ID });

                if (maxRank < (int)NameRanking.JP)
                    maxRank = (int)NameRanking.JP;

                rank = maxRank + 1;
            }

            var relationship = new SQL_Relationships
            {
                Type = relType.Value,
                Ranking = rank,
                Master_ID = master_ID,
                Slave_ID = slave_ID,
                AddedOn = DateTime.Now,
                User_ID = Environment.UserName
            };

            conn.Insert(relationship);
        }
    }

    public class NestDictionary<TKey1, TKey2, TValue> :
        Dictionary<TKey1, Dictionary<TKey2, TValue>>
    {
    }

    public static class NestDictionaryExtensions
    {
        public static Dictionary<TKey2, TValue> New<TKey1, TKey2, TValue>(this NestDictionary<TKey1, TKey2, TValue> _) => new();
    }

    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        // Indicates whether the current thread is processing work items.
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        // The list of tasks to be executed
        private readonly LinkedList<Task> _tasks = new(); // protected by lock(_tasks)

        // The maximum concurrency level allowed by this scheduler.
        private readonly int _maxDegreeOfParallelism;

        // Indicates whether the scheduler is currently processing work items.
        private int _delegatesQueuedOrRunning = 0;

        // Creates a new instance with the specified degree of parallelism.
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public LimitedConcurrencyLevelTaskScheduler()
        {
            if (Settings.Default.Workers < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = Settings.Default.Workers;
        }

        // Queues a task to the scheduler.
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // Inform the ThreadPool that there's work to be executed for this scheduler.
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        // Attempts to execute the specified task on the current thread.
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                // Try to run the task.
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        // Attempt to remove a previously scheduled task from the scheduler.
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        // Gets the maximum concurrency level supported by this scheduler.
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        // Gets an enumerable of the tasks currently scheduled on this scheduler.
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }

    public class AppSettings
    {
        /*Booleans*/
        public bool SaveOnExit { get; set; }
        public bool AdditionalNovelInfo { get; set; }
        public bool CreateIndex { get; set; }
        public bool DLImages { get; set; }
        public bool CreateFolder { get; set; }
        public bool DLChapterIfModified { get; set; }
        public bool RC_ChapterContent { get; set; }
        public bool RC_ChapterTitle { get; set; }
        public bool RC_VolumeName { get; set; }
        public bool RC_NovelDescription { get; set; }
        public bool RC_SeriesDescription { get; set; }
        public bool RC_Tags { get; set; }
        public bool GoogleAPI { get; set; }
        public bool TL_KeepOriginalAsWell { get; set; }
        public bool TL_ChapterContent { get; set; }
        public bool TL_ChapterTitle { get; set; }
        public bool TL_VolumeName { get; set; }
        public bool TL_NovelTitle { get; set; }
        public bool TL_NovelDescription { get; set; }
        public bool TL_SeriesTitle { get; set; }
        public bool TL_SeriesDescription { get; set; }
        public bool TL_AuthorName { get; set; }
        public bool CF_Volume { get; set; }
        public bool CF_Category { get; set; }
        public bool CF_Series { get; set; }
        public bool CF_Author { get; set; }
        public bool AppendUnknownTags { get; set; }
        public bool SaveUnknownTags { get; set; }
        public bool KeepIndexInsideNovelFolder { get; set; }
        public bool DivideChapterByPages { get; set; }
        public bool IncludeAuthorNote { get; set; }
        public bool IncludeChapterTitle { get; set; }
        public bool ScrapeTags { get; set; }
        public bool IncludeFootnotes { get; set; }
        public bool OnlyNovelInfo { get; set; }
        public bool ReplaceKnownTags { get; set; }
        public bool ImplementSQL { get; set; }
        public bool SqlButUrlsFromTxt { get; set; }
        public bool RemoveCensorship { get; set; }
        public bool RC_AuthorName { get; set; }
        public bool RC_NovelTitle { get; set; }
        public bool RC_SeriesTitle { get; set; }
        public bool NoChapterAlreadyDL { get; set; }
        /*Integers*/
        public int HiatusStatusLength { get; set; }
        public int OngoingStatusLength { get; set; }
        public int PageMaxLength { get; set; }
        public int Workers { get; set; }
        /*Strings*/
        public string ChapterFileNameFormat { get; set; }
        public string DateTimeFormat { get; set; }
        public string ImageFileNameFormat { get; set; }
        public string IndexFileNameFormat { get; set; }
        public string GoogleCredentials { get; set; }
        public string KnownTagsFileName { get; set; }
        public string NovelFolderNameFormat { get; set; }
        public string SavePath { get; set; }
        public string UnknownTagsFileName { get; set; }
        public string URLsFileName { get; set; }
        public string VolumeFolderNameFormat { get; set; }
        public string DBConnectionString { get; set; }
        public string MaruToHiraganaN { get; set; }
        public string MaruToKatakanaN { get; set; }
        public string DeadLinkFileNameFormat { get; set; }
        public string InvalidChapterFileNameFormat { get; set; }
        /*Others*/
        public Size Main_Size { get; set; }
        public Point Main_Location { get; set; }
        public System.Windows.Forms.FormWindowState Main_WindowState { get; set; }
        public Size UXS_Size { get; set; }
        public Point UXS_Location { get; set; }
        public System.Windows.Forms.FormWindowState UXS_WindowState { get; set; }

        public AppSettings()
        {
            /*Booleans*/
            SaveOnExit = Settings.Default.SaveOnExit;
            CreateIndex = Settings.Default.CreateIndex;
            KeepIndexInsideNovelFolder = Settings.Default.IndexInNovelFolder;
            DLImages = Settings.Default.DLImages;
            CreateFolder = Settings.Default.CreateFolder;
            CF_Volume = Settings.Default.CF_Volume;
            CF_Series = Settings.Default.CF_Series;
            CF_Author = Settings.Default.CF_Author;
            CF_Category = Settings.Default.CF_Category;
            DivideChapterByPages = Settings.Default.DivideChapterByPages;
            IncludeAuthorNote = Settings.Default.IncludeAuthorNote;
            IncludeChapterTitle = Settings.Default.IncludeChapterTitle;
            IncludeFootnotes = Settings.Default.IncludeFootnotes;
            ScrapeTags = Settings.Default.ScrapeTags;
            ReplaceKnownTags = Settings.Default.ReplaceKnownTags;
            SaveUnknownTags = Settings.Default.SaveUnknownTags;
            AppendUnknownTags = Settings.Default.AppendUnknownTags;
            OnlyNovelInfo = Settings.Default.OnlyNovelInfo;
            AdditionalNovelInfo = Settings.Default.AdditionalNovelInfo;
            NoChapterAlreadyDL = Settings.Default.NoChapterAlreadyDL;
            DLChapterIfModified = Settings.Default.DLChapterIfModified;
            ImplementSQL = Settings.Default.ImplementSQL;
            SqlButUrlsFromTxt = Settings.Default.SqlButUrlsFromTxt;
            RemoveCensorship = Settings.Default.RemoveCensorship;
            RC_ChapterContent = Settings.Default.RC_ChapterContent;
            RC_ChapterTitle = Settings.Default.RC_ChapterTitle;
            RC_VolumeName = Settings.Default.RC_VolumeName;
            RC_NovelTitle = Settings.Default.RC_NovelTitle;
            RC_NovelDescription = Settings.Default.RC_NovelDescription;
            RC_SeriesTitle = Settings.Default.RC_SeriesTitle;
            RC_SeriesDescription = Settings.Default.RC_SeriesDescription;
            RC_AuthorName = Settings.Default.RC_AuthorName;
            RC_Tags = Settings.Default.RC_Tags;
            GoogleAPI = Settings.Default.GoogleAPI;
            TL_KeepOriginalAsWell = Settings.Default.TL_KeepOriginalAsWell;
            TL_ChapterContent = Settings.Default.TL_ChapterContent;
            TL_ChapterTitle = Settings.Default.TL_ChapterTitle;
            TL_VolumeName = Settings.Default.TL_VolumeName;
            TL_NovelTitle = Settings.Default.TL_NovelTitle;
            TL_NovelDescription = Settings.Default.TL_NovelDescription;
            TL_SeriesTitle = Settings.Default.TL_SeriesTitle;
            TL_SeriesDescription = Settings.Default.TL_SeriesDescription;
            TL_AuthorName = Settings.Default.TL_AuthorName;

            /*Integers*/
            HiatusStatusLength = Settings.Default.HiatusStatusLength;
            OngoingStatusLength = Settings.Default.OngoingStatusLength;
            PageMaxLength = Settings.Default.PageMaxLength;
            Workers = Settings.Default.Workers;

            /*Strings*/
            //Date Format
            DateTimeFormat = Settings.Default.DateTimeFormat;
            //Name Format
            ChapterFileNameFormat = Settings.Default.ChapterFileNameFormat;
            ImageFileNameFormat = Settings.Default.ImageFileNameFormat;
            IndexFileNameFormat = Settings.Default.IndexFileNameFormat;
            NovelFolderNameFormat = Settings.Default.NovelFolderNameFormat;
            VolumeFolderNameFormat = Settings.Default.VolumeFolderNameFormat;
            UnknownTagsFileName = Settings.Default.UnknownTagsFileName;
            DeadLinkFileNameFormat = Settings.Default.DeadLinkFileNameFormat;
            InvalidChapterFileNameFormat = Settings.Default.InvalidChapterFileNameFormat;
            //Path
            SavePath = Settings.Default.SavePath;
            //File 1
            GoogleCredentials = Settings.Default.GoogleCredentials;
            //File 2
            KnownTagsFileName = Settings.Default.KnownTagsFileName;
            URLsFileName = Settings.Default.URLsFileName;
            //DB connection
            DBConnectionString = Settings.Default.DBConnectionString;
            //Free
            MaruToHiraganaN = Settings.Default.MaruToHiraganaN;
            MaruToKatakanaN = Settings.Default.MaruToKatakanaN;
            /*Others*/
            Main_Size = Settings.Default.Main_Size;
            Main_Location = Settings.Default.Main_Location;
            Main_WindowState = Settings.Default.Main_WindowState;
            UXS_Size = Settings.Default.UXS_Size;
            UXS_Location = Settings.Default.UXS_Location;
            UXS_WindowState = Settings.Default.UXS_WindowState;
        }

        public void Import()
        {
            /*Booleans*/
            Settings.Default.SaveOnExit = SaveOnExit;
            Settings.Default.CreateIndex = CreateIndex;
                Settings.Default.IndexInNovelFolder = KeepIndexInsideNovelFolder;
            Settings.Default.DLImages = DLImages;
            Settings.Default.CreateFolder = CreateFolder;
                Settings.Default.CF_Volume = CF_Volume;
                Settings.Default.CF_Series = CF_Series;
                Settings.Default.CF_Author = CF_Author;
                Settings.Default.CF_Category = CF_Category;
            Settings.Default.DivideChapterByPages = DivideChapterByPages;
            Settings.Default.IncludeAuthorNote = IncludeAuthorNote;
            Settings.Default.IncludeChapterTitle = IncludeChapterTitle;
            Settings.Default.IncludeFootnotes = IncludeFootnotes;
            Settings.Default.ScrapeTags = ScrapeTags;
                Settings.Default.ReplaceKnownTags = ReplaceKnownTags;
                Settings.Default.SaveUnknownTags = SaveUnknownTags;
                    Settings.Default.AppendUnknownTags = AppendUnknownTags;
            Settings.Default.OnlyNovelInfo = OnlyNovelInfo;
            Settings.Default.AdditionalNovelInfo = AdditionalNovelInfo;
            Settings.Default.NoChapterAlreadyDL = NoChapterAlreadyDL;
                Settings.Default.DLChapterIfModified = DLChapterIfModified;
            Settings.Default.ImplementSQL = ImplementSQL;
                Settings.Default.SqlButUrlsFromTxt = SqlButUrlsFromTxt;
            Settings.Default.RemoveCensorship = RemoveCensorship;
                Settings.Default.RC_ChapterContent = RC_ChapterContent;
                Settings.Default.RC_ChapterTitle = RC_ChapterTitle;
                Settings.Default.RC_VolumeName = RC_VolumeName;
                Settings.Default.RC_NovelTitle = RC_NovelTitle;
                Settings.Default.RC_NovelDescription = RC_NovelDescription;
                Settings.Default.RC_SeriesTitle = RC_SeriesTitle;
                Settings.Default.RC_SeriesDescription = RC_SeriesDescription;
                Settings.Default.RC_AuthorName = RC_AuthorName;
                Settings.Default.RC_Tags = RC_Tags;
            Settings.Default.GoogleAPI = GoogleAPI;
            Settings.Default.TL_KeepOriginalAsWell = TL_KeepOriginalAsWell;
            Settings.Default.TL_ChapterContent = TL_ChapterContent;
            Settings.Default.TL_ChapterTitle = TL_ChapterTitle;
            Settings.Default.TL_VolumeName = TL_VolumeName;
            Settings.Default.TL_NovelTitle = TL_NovelTitle;
            Settings.Default.TL_NovelDescription = TL_NovelDescription;
            Settings.Default.TL_SeriesTitle = TL_SeriesTitle;
            Settings.Default.TL_SeriesDescription = TL_SeriesDescription;
            Settings.Default.TL_AuthorName = TL_AuthorName;

            /*Integers*/
            Settings.Default.HiatusStatusLength = HiatusStatusLength;
            Settings.Default.OngoingStatusLength = OngoingStatusLength;
            Settings.Default.PageMaxLength = PageMaxLength;
            Settings.Default.Workers = Workers;

            /*Strings*/
            //Date Format
            Settings.Default.DateTimeFormat = DateTimeFormat;
            //Name Format
            Settings.Default.ChapterFileNameFormat = ChapterFileNameFormat;
            Settings.Default.ImageFileNameFormat = ImageFileNameFormat;
            Settings.Default.IndexFileNameFormat = IndexFileNameFormat;
            Settings.Default.NovelFolderNameFormat = NovelFolderNameFormat;
            Settings.Default.VolumeFolderNameFormat = VolumeFolderNameFormat;
            Settings.Default.UnknownTagsFileName = UnknownTagsFileName;
            Settings.Default.DeadLinkFileNameFormat = DeadLinkFileNameFormat;
            Settings.Default.InvalidChapterFileNameFormat = InvalidChapterFileNameFormat;
            //Path
            Settings.Default.SavePath = SavePath;
            //File 1
            Settings.Default.GoogleCredentials = GoogleCredentials;
            //File 2
            Settings.Default.KnownTagsFileName = KnownTagsFileName;
            Settings.Default.URLsFileName = URLsFileName;
            //DB connection
            Settings.Default.DBConnectionString = DBConnectionString;
            //Free
            Settings.Default.MaruToHiraganaN = MaruToHiraganaN;
            Settings.Default.MaruToKatakanaN = MaruToKatakanaN;
            /*Others*/
            Settings.Default.Main_Size = Main_Size;
            Settings.Default.Main_Location = Main_Location;
            Settings.Default.Main_WindowState = Main_WindowState;
            Settings.Default.UXS_Size = UXS_Size;
            Settings.Default.UXS_Location = UXS_Location;
            Settings.Default.UXS_WindowState = UXS_WindowState;
        }
    }
}