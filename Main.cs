using Dapper;
using Dapper.Contrib.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SyosetuScraper
{
    public partial class Main : Form
    {
        #region Global Variables

        private readonly static string _defaultSavePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Syosetu Novels\";
        public static Dictionary<string, string> KnownTags { get; set; } = new Dictionary<string, string>();
        private static Dictionary<string, string> _urls = new();
        public static Dictionary<string, int> UnknownTags { get; private set; } = new Dictionary<string, int>();
        public LimitedConcurrencyLevelTaskScheduler lcts = new();
        public static List<Author> Authors { get; set; } = new List<Author>();
        public static List<Series> Series { get; set; } = new List<Series>();
        public static List<Novel> Novels { get; set; } = new List<Novel>();
        public static TranslationClient gClient { get; private set; }
        private DataTable novels_DataTable;
        private DataTable tags_DataTable;

        enum UrlTypes
        {
            Invalid = 0,
            Novel = 1,
            Series = 2,
            Author = 3
        }

        #endregion

        #region Generated

        public Main() => InitializeComponent();

        private void Main_Load(object sender, EventArgs e)
        {
            Size = Settings.Default.Main_Size;
            Location = Settings.Default.Main_Location;
            WindowState = Settings.Default.Main_WindowState;

            Helpers.SyousetsuCookie.Add(new Cookie("fix_menu_bar", "1", "/", ".syosetu.com"));
            Helpers.SyousetsuCookie.Add(new Cookie("fontsize", "0", "/", ".syosetu.com"));
            Helpers.SyousetsuCookie.Add(new Cookie("ks2", "4vbep391u5mu", "/", ".syosetu.com"));
            Helpers.SyousetsuCookie.Add(new Cookie("lineheight", "0", "/", ".syosetu.com"));
            Helpers.SyousetsuCookie.Add(new Cookie("novellayout", "0", "/", ".syosetu.com"));
            Helpers.SyousetsuCookie.Add(new Cookie("sasieno", "0", "/", ".syosetu.com"));
            Helpers.SyousetsuCookie.Add(new Cookie("over18", "yes", "/", ".syosetu.com"));

            dgv_Novels.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgv_Tags.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv_Novels.ContextMenuStrip = contextMenuStrip;

            SetupDGV();
        }

        private void OnFormClose(object sender, EventArgs e)
        {
            if (Settings.Default.SaveOnExit)
            {
                Settings.Default.Main_Size = Size;
                Settings.Default.Main_Location = Location;
                Settings.Default.Main_WindowState = WindowState;

                Settings.Default.Save();
            }
        }

        private async void Tsmi_Scrape_Click(object sender, EventArgs e)
        {
            tsmiM_Settings.Enabled = false;

            if (Settings.Default.GoogleAPI && !string.IsNullOrEmpty(Settings.Default.GoogleCredentials))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Settings.Default.GoogleCredentials);
                gClient = TranslationClient.Create(GoogleCredential.GetApplicationDefault());
            }

            if (string.IsNullOrEmpty(Settings.Default.SavePath) || !Directory.Exists(Settings.Default.SavePath))
                Settings.Default.SavePath = _defaultSavePath;

            if (string.IsNullOrEmpty(Settings.Default.URLsFileName))
                Settings.Default.URLsFileName = "URLs";

            var source = Settings.Default.SavePath + Settings.Default.URLsFileName + ".txt";

            if (!Settings.Default.ImplementSQL || Settings.Default.SqlButUrlsFromTxt)
                if (!File.Exists(source))
                {
                    tsmiM_Settings.Enabled = true;
                    MessageBox.Show("Url source not found", "Novels Error");
                    return;
                }

            KnownTags = new Dictionary<string, string>();
            _urls = new Dictionary<string, string>();
            UnknownTags = new Dictionary<string, int>();
            Authors = new List<Author>();
            Series = new List<Series>();
            Novels = new List<Novel>();

            var tagTask = GetTagsDictionary();
            var urlTask = GetUrls(source);

            Task.WaitAll(tagTask, urlTask);

            var scheduler = new LimitedConcurrencyLevelTaskScheduler();
            var taskFactory = new TaskFactory(scheduler);
            var tasks = new List<Task>();

            Authors.ForEach(author => tasks.Add(taskFactory.StartNew(() => author.Setup())));
            Series.ForEach(series => tasks.Add(taskFactory.StartNew(() => series.Setup())));

            Task.WaitAll(tasks.ToArray());

            Authors.Where(author => author.Name == "エラー").ToList().ForEach(author => Authors.Remove(author));

            Authors.ForEach(author =>
                author.Novels.ForEach(authNovel =>
                {
                    if (!Novels.Exists(mainNovel => mainNovel.Link == authNovel.Link))
                        Novels.Add(authNovel);
                }));

            Authors.Clear();

            Series.Where(series => series.Name == "エラー").ToList().ForEach(series => Series.Remove(series));

            Series.ForEach(series =>
                series.Novels.ForEach(seriNovel =>
                {
                    if (!Novels.Exists(mainNovel => mainNovel.Link == seriNovel.Link))
                        Novels.Add(seriNovel);
                }));

            Series.Clear();

            if (Novels.Count == 0)
            {
                tsmiM_Settings.Enabled = true;
                MessageBox.Show("No novels to scrape found.");
                return;
            }

            tasks = new List<Task>();

            Novels.ForEach(novel => tasks.Add(taskFactory.StartNew(novel.Setup)));

            var finalTask = Task.Factory.ContinueWhenAll(tasks.ToArray(), novelCountTasks =>
            {
                int nSuccessfulTasks = 0;
                int nFailed = 0;
                foreach (var t in novelCountTasks)
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                        nSuccessfulTasks++;

                    if (t.Status == TaskStatus.Faulted)
                        nFailed++;
                }
                SaveNewTags();
                MessageBox.Show($"Scraped {nSuccessfulTasks} novels, " +
                    $"with {UnknownTags.Count} new tags and {nFailed} errors."
                    , "Download Complete");
            });

            tsmiM_Settings.Enabled = true;
        }

        private void TsmiM_Settings_Click(object sender, EventArgs e)
        {
            _ = new UX_Settings().ShowDialog();

            SetSQLElementsMode();
        }

        private void TsmiM_Export_Click(object sender, EventArgs e)
        {
            string settingsSavePath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                settingsSavePath = folderBrowserDialog.SelectedPath;
            else
                return;

            if (!settingsSavePath.EndsWith("\\"))
                settingsSavePath += "\\";

            settingsSavePath += "Settings.json";

            string json = JsonSerializer.Serialize(new AppSettings());

            if (!File.Exists(settingsSavePath))
                using (var tw = new StreamWriter(settingsSavePath))
                    tw.WriteLine(json);
            else if (File.Exists(settingsSavePath))
                using (var tw = new StreamWriter(settingsSavePath.Replace(".json", $"{DateTime.Now:-yyyy-MM-dd-HH-mm}.json"), false))
                    tw.WriteLine(json);

            MessageBox.Show("Export ended.");
        }

        private void TsmiM_Import_Click(object sender, EventArgs e)
        {
            string settingsFilePath;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                settingsFilePath = openFileDialog.FileName;
            else
                return;

            if (!File.Exists(settingsFilePath))
                return;

            var jsonString = File.ReadAllText(settingsFilePath);
            AppSettings appSettings = JsonSerializer.Deserialize<AppSettings>(jsonString);
            appSettings.Import();

            MessageBox.Show("Import ended.");
        }

        private void Btn_FilterDGVNovels_Click(object sender, EventArgs e)
        {
            novels_DataTable.DefaultView.RowFilter = $@"Novel_ID = {nud_FilterNovelID.Value}
                or (Nickname like '%{txt_FilterNickname.Text}%'
                and Japanese_Name like '%{txt_FilterJapaneseName.Text}%'
                and Novel_Type like '%{cmb_FilterNovelType.SelectedValue}%'
                and Novel_Status like '%{cmb_FilterNovelStatus.Text}%')";

            Dgv_Novels_Sorted(dgv_Novels, EventArgs.Empty);
        }

        private void Btn_UpdateDGVNovels_Click(object sender, EventArgs e) => SetupDGV();

        private void Dgv_Novels_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv_Novels.SelectedCells.Count != 1)
                return;

            var rowIndex = dgv_Novels.SelectedCells[0].RowIndex;
            var novel_id = dgv_Novels.Rows[rowIndex].Cells["Novel_ID"].Value;

            using var conn = Helpers.GetConnection();

            tags_DataTable = ToDataTable(conn.Query<SQL_Tags>(@"select * from tags where tag_id in (
                select slave_id from relationships
                where master_id = @Novel_ID and type=2)", new { novel_id }).ToList());
            dgv_Tags.DataSource = tags_DataTable.DefaultView;

            foreach (DataGridViewColumn column in dgv_Tags.Columns)
                column.Visible = false;

            dgv_Tags.Columns["Tag_Name"].Visible =
            dgv_Tags.Columns["Tag_Meaning"].Visible = true;

            dgv_Tags.Columns["Tag_Meaning"].DisplayIndex = 0;
            dgv_Tags.Columns["Tag_Name"].DisplayIndex = 1;

            dgv_Tags.Columns["Tag_Meaning"].HeaderText = "English";
            dgv_Tags.Columns["Tag_Name"].HeaderText = "Japanese";

            dgv_Tags.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void Dgv_Novels_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var columnName = dgv_Novels.Columns[e.ColumnIndex].Name;

            using var conn = Helpers.GetConnection();
            var updateNovel = conn.Get<SQL_Novels>(dgv_Novels.Rows[e.RowIndex].Cells["Novel_ID"].Value);
            var propertyInfo = updateNovel.GetType().GetProperty(columnName);

            if (propertyInfo is null)
                return;

            propertyInfo.SetValue(updateNovel, Convert.ChangeType(dgv_Novels
                .Rows[e.RowIndex].Cells[e.ColumnIndex].Value, propertyInfo.PropertyType), null);

            conn.Update(updateNovel);

            SetupDGV();
        }

        private void Dgv_Novels_Sorted(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgv_Novels.Rows)
            {
                row.Cells["Novel_Status"].Style.BackColor = row.Cells["Novel_Status"].Value switch
                {
                    "ongoing" => Color.FromArgb(82, 243, 203),
                    "one-shot" => Color.FromArgb(7, 175, 50),
                    "completed" => Color.FromArgb(7, 175, 50),
                    "hiatus" => Color.FromArgb(240, 247, 6),
                    "dropped" => Color.FromArgb(225, 19, 9),
                    "dead link" => Color.FromArgb(139, 150, 147),
                    _ => DefaultBackColor,
                };

                var daysSinceLastScrape = (DateTime.Today - (DateTime)row.Cells["LastScrape"].Value).TotalDays;

                row.Cells["LastScrape"].Style.BackColor = (row.Cells["Novel_Status"].Value, daysSinceLastScrape) switch
                {
                    ("ongoing", <= 20) => Color.FromArgb(82, 243, 203),
                    ("ongoing", > 20 and <= 40) => Color.FromArgb(139, 150, 255),
                    ("ongoing", > 40) => Color.FromArgb(11, 147, 131),
                    ("one-shot", <= 365) => Color.FromArgb(7, 175, 50),
                    ("one-shot", > 365) => Color.FromArgb(196, 240, 86),
                    ("completed", <= 365) => Color.FromArgb(7, 175, 50),
                    ("completed", > 365) => Color.FromArgb(196, 240, 86),
                    ("hiatus", <= 180) => Color.FromArgb(240, 247, 6),
                    ("hiatus", > 180) => Color.FromArgb(244, 202, 82),
                    ("dropped", <= 365) => Color.FromArgb(224, 132, 10),
                    ("dropped", > 365) => Color.FromArgb(225, 19, 9),
                    ("dead link", _) => Color.FromArgb(139, 150, 147),
                    _ => DefaultBackColor,
                };
            }
        }

        private void Tscmi_OpenPage_Click(object sender, EventArgs e)
        {
            if (dgv_Novels.SelectedRows.Count <= 0)
                return;

            using var conn = Helpers.GetConnection();

            foreach (DataGridViewRow novel in dgv_Novels.SelectedRows)
            {
                var novelID = novel.Cells["Novel_ID"].Value;

                //Novel_Page

                var novelUrl = conn.QuerySingleOrDefault<string>(@"select Novel_Page from Novels where Novel_ID = @novelID", new { novelID });

                if (novelUrl is null)
                    continue;

                Process.Start(new ProcessStartInfo { FileName = novelUrl, UseShellExecute = true });
            }
        }

        #endregion

        #region Created

        private static void SaveNewTags()
        {
            if (UnknownTags.Count < 1 || !(Settings.Default.ScrapeTags && Settings.Default.SaveUnknownTags))
                return;
            
            var path = Settings.Default.SavePath + Helpers.CheckChars(Settings.Default.UnknownTagsFileName);
            if (!path.EndsWith(".txt")) path += ".txt";

            if (!File.Exists(path))
                using (var tw = new StreamWriter(path))
                    foreach (var item in UnknownTags.Keys)
                        tw.WriteLine(item);
            else if (File.Exists(path))
                using (var tw = new StreamWriter(path, Settings.Default.AppendUnknownTags))
                    foreach (var item in UnknownTags.Keys)
                        tw.WriteLine(item);
        }

        private async static Task GetTagsDictionary()
        {
            if (!Settings.Default.ScrapeTags)
                return;

            if (Settings.Default.ImplementSQL)
            {
                using var conn = Helpers.GetConnection();
                var res = (await conn.QueryAsync("select Tag_Name, Tag_Meaning from Tags where Tag_Meaning is not null")
                    .ConfigureAwait(false)).ToDictionary(row => (string)row.Tag_Name, row => (string)row.Tag_Meaning);

                KnownTags = new Dictionary<string, string>(res);
            }
            else
            {
                var path = Settings.Default.SavePath + Settings.Default.KnownTagsFileName + ".txt";

                if (!File.Exists(path))
                {
                    MessageBox.Show("Tags source not found", "Tags Error");
                    return;
                }

                var lines = await File.ReadAllLinesAsync(path).ConfigureAwait(false);

                foreach (var line in lines)
                {
                    var x = line.Split(";");

                    if (x.Length < 2) continue;

                    var key = x[0].Normalize(NormalizationForm.FormKC).ToUpper();

                    if (KnownTags.ContainsKey(key)) continue;

                    KnownTags.Add(key, x[1]);
                }
            }
        }

        private async static Task GetUrls(string source)
        {
            if (Settings.Default.ImplementSQL && !Settings.Default.SqlButUrlsFromTxt)
            {
                using var conn = Helpers.GetConnection();
                var res = (await conn.QueryAsync(@"select Novel_Page, Name_Value
                    from Novels
                    left join Relationships on Type = 1 and Master_ID = Novel_ID and Ranking = 0
                    left join Names on Name_ID = Slave_ID
                    where Scrape=1")
                    .ConfigureAwait(false)).ToDictionary(row => (string)row.Novel_Page, row => (string)row.Name_Value);

                if (res.Count > 0)
                    foreach (var novel in res)
                        _urls.TryAdd(novel.Key, novel.Value);
            }

            if (!Settings.Default.ImplementSQL || Settings.Default.SqlButUrlsFromTxt)
            {
                var lines = await File.ReadAllLinesAsync(source).ConfigureAwait(false);

                foreach (var line in lines)
                {
                    var x = line.Split(";");

                    if (x.Length < 1) continue;
                    if (_urls.ContainsKey(x[0])) continue;

                    var nick = (x.Length > 1) ? x[1] : string.Empty;

                    _urls.Add(x[0], nick);
                }
            }

            foreach (var url in _urls)
            {
                switch (UrlType(url.Key.ToLower(), out var code))
                {
                    case UrlTypes.Novel:
                        if (!Novels.Exists(novel => novel.SyosetuId == code))
                            Novels.Add(new Novel(url.Value, url.Key) { SyosetuId = code });
                        break;
                    case UrlTypes.Series:
                        if (!Series.Exists(series => series.SyosetuId == code))
                            Series.Add(new Series(url.Key) { SyosetuId = code });
                        break;
                    case UrlTypes.Author:
                        if (!Authors.Exists(author => author.SyosetuId == code))
                            Authors.Add(new Author(url.Key) { SyosetuId = code });
                        break;
                    default:
                        break;
                }
            }
        }

        private static UrlTypes UrlType(string url, out string code)
        {
            code = null;

            if (url is null)
                return UrlTypes.Invalid;

            var matches = Regex.Matches(url, @"\b\w+\b");

            if (matches is null)
                return UrlTypes.Invalid;

            switch (matches.Count)
            {
                case 5:
                    code = matches[4].Value;

                    if (matches[1].Value.Contains("mypage"))
                        return UrlTypes.Author;

                    if (matches[4].Value[0..1] == "n")
                        return UrlTypes.Novel;

                    //changed from [0..3], as the indexes are inclusive, thus taking the first three instead of the first two
                    if (matches[4].Value[0..2].StartsWith("s") || matches[4].Value[0..2].EndsWith("s"))
                        return UrlTypes.Series;

                    return UrlTypes.Invalid;

                case 8:
                    code = matches[7].Value;
                    return UrlTypes.Author;

                default:
                    return UrlTypes.Invalid;
            }
        }

        private async void SetupDGV()
        {
            if (!Settings.Default.ImplementSQL)
            {
                SetSQLElementsMode();
                return;
            }
            
            var sortedColumn = dgv_Novels.SortedColumn;
            var sortOrder = dgv_Novels.SortOrder;
            var direction = ListSortDirection.Ascending;
            if (sortOrder != SortOrder.Ascending) direction = ListSortDirection.Descending;

            using var conn = Helpers.GetConnection();

            if (conn is null)
            {
                return;
            }

            novels_DataTable = ToDataTable(conn.Query<ViewNovels>(@"select * from viewnovels").ToList());
            dgv_Novels.DataSource = novels_DataTable.DefaultView;

            dgv_Novels.Columns["Novel_ID"].ReadOnly = true;
            dgv_Novels.Columns["Percentage"].ReadOnly = true;
            dgv_Novels.Columns["LastScrape"].ReadOnly = true;

            dgv_Novels.Columns["English_Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv_Novels.Columns["Japanese_Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

            dgv_Novels.AutoResizeColumns();

            string sortedColumnName;
            if (sortedColumn is not null)
                sortedColumnName = sortedColumn.Name;
            else
                sortedColumnName = "LastScrape";

            dgv_Novels.Sort(dgv_Novels.Columns[sortedColumnName], direction);

            var uniqueNovelTypes = novels_DataTable
                .AsEnumerable()
                .Select(row => row["Novel_Type"])
                .Distinct()
                .ToArray();

            cmb_FilterNovelType.Items.Clear();
            cmb_FilterNovelType.Items.AddRange(uniqueNovelTypes);

            var uniqueNovelStatus = novels_DataTable
                .AsEnumerable()
                .Select(row => row["Novel_Status"])
                .Distinct()
                .ToArray();

            cmb_FilterNovelStatus.Items.Clear();
            cmb_FilterNovelStatus.Items.AddRange(uniqueNovelStatus);
        }

        private static DataTable ToDataTable<T>(IList<T> data)
        {
            var properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        private void SetSQLElementsMode()
        {
            dgv_Tags.Visible =
            dgv_Novels.Visible =
            contextMenuStrip.Visible =
            nud_FilterNovelID.Visible =
            btn_FilterDGVNovels.Visible =
            btn_UpdateDGVNovels.Visible =
            cmb_FilterNovelType.Visible =
            cmb_FilterNovelStatus.Visible =
            txt_FilterNickname.Visible =
            txt_FilterEnglishName.Visible =
            txt_FilterJapaneseName.Visible =
            dgv_Tags.Enabled =
            dgv_Novels.Enabled =
            contextMenuStrip.Enabled =
            nud_FilterNovelID.Enabled =
            btn_FilterDGVNovels.Enabled =
            btn_UpdateDGVNovels.Enabled =
            cmb_FilterNovelType.Enabled =
            cmb_FilterNovelStatus.Enabled =
            txt_FilterNickname.Enabled =
            txt_FilterEnglishName.Enabled =
            txt_FilterJapaneseName.Enabled = Settings.Default.ImplementSQL;
        }

        #endregion
    }
}