using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SyosetuScraper
{
    public partial class UX_Settings : Form
    {
        #region Global Variables

        private int _lastOpenedTab = 0;

        enum TxtVal
        {
            NameFormat = 0,
            DateFormat = 1,
            Path = 2,
            File = 3,
            DB = 4
        }

        #endregion

        #region Generated

        public UX_Settings() => InitializeComponent();

        private void UX_Settings_Load(object sender, EventArgs e)
        {
            tabControl.SelectedIndex = Settings.Default.UXS_LastOpenedTab;

            /*CheckBoxes*/
            chk_SaveOnExit.Checked = Settings.Default.SaveOnExit;
            chk_CreateIndex.Checked = Settings.Default.CreateIndex;
            chk_DLImages.Checked = Settings.Default.DLImages;
            chk_CreateFolder.Checked = Settings.Default.CreateFolder;
            chk_CF_Volume.Checked = Settings.Default.CF_Volume;
            chk_CF_Series.Checked = Settings.Default.CF_Series;
            chk_CF_Author.Checked = Settings.Default.CF_Author;
            chk_CF_Category.Checked = Settings.Default.CF_Category;
            chk_DivideChapterByPages.Checked = Settings.Default.DivideChapterByPages;
            chk_IncludeAuthorNote.Checked = Settings.Default.IncludeAuthorNote;
            chk_IncludeChapterTitle.Checked = Settings.Default.IncludeChapterTitle;
            chk_IncludeFootnotes.Checked = Settings.Default.IncludeFootnotes;
            chk_ScrapeTags.Checked = Settings.Default.ScrapeTags;
            chk_ReplaceKnownTags.Checked = Settings.Default.ReplaceKnownTags;
            chk_SaveUnknownTags.Checked = Settings.Default.SaveUnknownTags;
            chk_AppendUnknownTags.Checked = Settings.Default.AppendUnknownTags;
            chk_IndexInNovelFolder.Checked = Settings.Default.IndexInNovelFolder;
            chk_OnlyNovelInfo.Checked = Settings.Default.OnlyNovelInfo;
            chk_AdditionalNovelInfo.Checked = Settings.Default.AdditionalNovelInfo;
            chk_ImplementSQL.Checked = Settings.Default.ImplementSQL;
            chk_SqlButUrlsFromTxt.Checked = Settings.Default.SqlButUrlsFromTxt;
            chk_RemoveCensorship.Checked = Settings.Default.RemoveCensorship;
            chk_RC_ChapterContent.Checked = Settings.Default.RC_ChapterContent;
            chk_RC_ChapterTitle.Checked = Settings.Default.RC_ChapterTitle;
            chk_RC_VolumeName.Checked = Settings.Default.RC_VolumeName;
            chk_RC_NovelTitle.Checked = Settings.Default.RC_NovelTitle;
            chk_RC_NovelDescription.Checked = Settings.Default.RC_NovelDescription;
            chk_RC_SeriesTitle.Checked = Settings.Default.RC_SeriesTitle;
            chk_RC_SeriesDescription.Checked = Settings.Default.RC_SeriesDescription;
            chk_RC_AuthorName.Checked = Settings.Default.RC_AuthorName;
            chk_RC_Tags.Checked = Settings.Default.RC_Tags;
            chk_NoChapterAlreadyDL.Checked = Settings.Default.NoChapterAlreadyDL;
            chk_DLChapterIfModified.Checked = Settings.Default.DLChapterIfModified;
            chk_GoogleAPI.Checked = Settings.Default.GoogleAPI;
            chk_TL_KeepOriginalAsWell.Checked = Settings.Default.TL_KeepOriginalAsWell;
            chk_TL_NovelTitle.Checked = Settings.Default.TL_NovelTitle;
            chk_TL_NovelDescription.Checked = Settings.Default.TL_NovelDescription;
            chk_TL_SeriesTitle.Checked = Settings.Default.TL_SeriesTitle;
            chk_TL_SeriesDescription.Checked = Settings.Default.TL_SeriesDescription;
            chk_TL_AuthorName.Checked = Settings.Default.TL_AuthorName;
            chk_TL_ChapterContent.Checked = Settings.Default.TL_ChapterContent;
            chk_TL_ChapterTitle.Checked = Settings.Default.TL_ChapterTitle;
            chk_TL_VolumeName.Checked = Settings.Default.TL_VolumeName;
            /*NumericUpDown*/
            nud_HiatusStatusLength.Value = Settings.Default.HiatusStatusLength;
            nud_OngoingStatusLength.Value = Settings.Default.OngoingStatusLength;
            nud_PageMaxLength.Value = Settings.Default.PageMaxLength;
            nud_Workers.Value = Settings.Default.Workers;
            /*TextBoxes*/
            txt_ChapterFileNameFormat.Text = Settings.Default.ChapterFileNameFormat;
            txt_DateFormat.Text = Settings.Default.DateTimeFormat;
            txt_ImageFileNameFormat.Text = Settings.Default.ImageFileNameFormat;
            txt_IndexFileNameFormat.Text = Settings.Default.IndexFileNameFormat;
            txt_KnownTagsFileName.Text = Settings.Default.KnownTagsFileName;
            txt_NovelFolderNameFormat.Text = Settings.Default.NovelFolderNameFormat;
            txt_SavePath.Text = Settings.Default.SavePath;
            txt_UnknownTagsFileName.Text = Settings.Default.UnknownTagsFileName;
            txt_UrlsFileName.Text = Settings.Default.URLsFileName;
            txt_VolumeFolderNameFormat.Text = Settings.Default.VolumeFolderNameFormat;
            txt_DBConnectionString.Text = Settings.Default.DBConnectionString;
            txt_MaruToHiraganaN.Text = Settings.Default.MaruToHiraganaN;
            txt_MaruToKatakanaN.Text = Settings.Default.MaruToKatakanaN;
            txt_DeadLinkFileNameFormat.Text = Settings.Default.DeadLinkFileNameFormat;
            txt_InvalidChapterFileNameFormat.Text = Settings.Default.InvalidChapterFileNameFormat;
            txt_GoogleCredentials.Text = Settings.Default.GoogleCredentials;
            /*Form*/
            Size = Settings.Default.UXS_Size;
            Location = Settings.Default.UXS_Location;
            WindowState = Settings.Default.UXS_WindowState;

            /*Disable child options of unchecked parents*/
            foreach (TabPage tabPage in tabControl.Controls)
                foreach (Control tPControl in tabPage.Controls)
                    if (tPControl is CheckBox chkBox)
                        CheckBox_CheckedChanged(chkBox, EventArgs.Empty);
        }

        private void OnFormClose(object sender, FormClosingEventArgs e)
        {
            var msg = "Do you want to save these settings?";
            var res = DialogResult.Yes;

            if (!chk_SaveOnExit.Checked)
                res = MessageBox.Show(msg, "Save settings", MessageBoxButtons.YesNoCancel);

            switch (res)
            {
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
                case DialogResult.No:
                    break;
                case DialogResult.Yes:
                    /*CheckBoxes*/
                    Settings.Default.SaveOnExit = chk_SaveOnExit.Checked;
                    Settings.Default.CreateIndex = chk_CreateIndex.Checked;
                        Settings.Default.IndexInNovelFolder = chk_IndexInNovelFolder.Checked;
                    Settings.Default.DLImages = chk_DLImages.Checked;
                    Settings.Default.CreateFolder = chk_CreateFolder.Checked;
                        Settings.Default.CF_Volume = chk_CF_Volume.Checked;
                        Settings.Default.CF_Series = chk_CF_Series.Checked;
                        Settings.Default.CF_Author = chk_CF_Author.Checked;
                        Settings.Default.CF_Category = chk_CF_Category.Checked;
                    Settings.Default.DivideChapterByPages = chk_DivideChapterByPages.Checked;
                    Settings.Default.IncludeAuthorNote = chk_IncludeAuthorNote.Checked;
                    Settings.Default.IncludeChapterTitle = chk_IncludeChapterTitle.Checked;
                    Settings.Default.IncludeFootnotes = chk_IncludeFootnotes.Checked;
                    Settings.Default.ScrapeTags = chk_ScrapeTags.Checked;
                        Settings.Default.ReplaceKnownTags = chk_ReplaceKnownTags.Checked;
                        Settings.Default.SaveUnknownTags = chk_SaveUnknownTags.Checked;
                            Settings.Default.AppendUnknownTags = chk_AppendUnknownTags.Checked;
                    Settings.Default.OnlyNovelInfo = chk_OnlyNovelInfo.Checked;
                    Settings.Default.AdditionalNovelInfo = chk_AdditionalNovelInfo.Checked;
                    Settings.Default.NoChapterAlreadyDL = chk_NoChapterAlreadyDL.Checked;
                        Settings.Default.DLChapterIfModified = chk_DLChapterIfModified.Checked;
                    Settings.Default.ImplementSQL = chk_ImplementSQL.Checked;
                        Settings.Default.SqlButUrlsFromTxt = chk_SqlButUrlsFromTxt.Checked;
                    Settings.Default.RemoveCensorship = chk_RemoveCensorship.Checked;
                        Settings.Default.RC_ChapterContent = chk_RC_ChapterContent.Checked;
                        Settings.Default.RC_ChapterTitle = chk_RC_ChapterTitle.Checked;
                        Settings.Default.RC_VolumeName = chk_RC_VolumeName.Checked;
                        Settings.Default.RC_NovelTitle = chk_RC_NovelTitle.Checked;
                        Settings.Default.RC_NovelDescription = chk_RC_NovelDescription.Checked;
                        Settings.Default.RC_SeriesTitle = chk_RC_SeriesTitle.Checked;
                        Settings.Default.RC_SeriesDescription = chk_RC_SeriesDescription.Checked;
                        Settings.Default.RC_AuthorName = chk_RC_AuthorName.Checked;
                        Settings.Default.RC_Tags = chk_RC_Tags.Checked;
                    Settings.Default.GoogleAPI = chk_GoogleAPI.Checked;
                    Settings.Default.TL_KeepOriginalAsWell = chk_TL_KeepOriginalAsWell.Checked;
                    Settings.Default.TL_ChapterContent = chk_TL_ChapterContent.Checked;
                    Settings.Default.TL_ChapterTitle = chk_TL_ChapterTitle.Checked;
                    Settings.Default.TL_VolumeName = chk_TL_VolumeName.Checked;
                    Settings.Default.TL_NovelTitle = chk_TL_NovelTitle.Checked;
                    Settings.Default.TL_NovelDescription = chk_TL_NovelDescription.Checked;
                    Settings.Default.TL_SeriesTitle = chk_TL_SeriesTitle.Checked;
                    Settings.Default.TL_SeriesDescription = chk_TL_SeriesDescription.Checked;
                    Settings.Default.TL_AuthorName = chk_TL_AuthorName.Checked;

                    /*NumericUpDown*/
                    Settings.Default.HiatusStatusLength = (int)nud_HiatusStatusLength.Value;
                    Settings.Default.OngoingStatusLength = (int)nud_OngoingStatusLength.Value;
                    Settings.Default.PageMaxLength = (int)nud_PageMaxLength.Value;
                    Settings.Default.Workers = (int)nud_Workers.Value;
                    Settings.Default.UXS_LastOpenedTab = _lastOpenedTab;
                    
                    /*TextBoxes*/
                    //Date Format
                    if (Text_Validation(txt_DateFormat, TxtVal.DateFormat))
                        Settings.Default.DateTimeFormat = txt_DateFormat.Text;
                    //Name Format
                    if (Text_Validation(txt_ChapterFileNameFormat, TxtVal.NameFormat))
                        Settings.Default.ChapterFileNameFormat = txt_ChapterFileNameFormat.Text;
                    if (Text_Validation(txt_ImageFileNameFormat, TxtVal.NameFormat))
                        Settings.Default.ImageFileNameFormat = txt_ImageFileNameFormat.Text;
                    if (Text_Validation(txt_IndexFileNameFormat, TxtVal.NameFormat))
                        Settings.Default.IndexFileNameFormat = txt_IndexFileNameFormat.Text;
                    if (Text_Validation(txt_NovelFolderNameFormat, TxtVal.NameFormat))
                        Settings.Default.NovelFolderNameFormat = txt_NovelFolderNameFormat.Text;
                    if (Text_Validation(txt_VolumeFolderNameFormat, TxtVal.NameFormat))
                        Settings.Default.VolumeFolderNameFormat = txt_VolumeFolderNameFormat.Text;
                    if (Text_Validation(txt_UnknownTagsFileName, TxtVal.NameFormat))
                        Settings.Default.UnknownTagsFileName = txt_UnknownTagsFileName.Text;
                    if (Text_Validation(txt_DeadLinkFileNameFormat, TxtVal.NameFormat))
                        Settings.Default.DeadLinkFileNameFormat = txt_DeadLinkFileNameFormat.Text;
                    if (Text_Validation(txt_InvalidChapterFileNameFormat, TxtVal.NameFormat))
                        Settings.Default.InvalidChapterFileNameFormat = txt_InvalidChapterFileNameFormat.Text;
                    //Path
                    if (Text_Validation(txt_SavePath, TxtVal.Path))
                        Settings.Default.SavePath = txt_SavePath.Text;
                    //File 1
                    if (File.Exists(txt_GoogleCredentials.Text))
                        Settings.Default.GoogleCredentials = txt_GoogleCredentials.Text;
                    //File 2
                    if (!chk_ImplementSQL.Checked)
                        if (Text_Validation(txt_KnownTagsFileName, TxtVal.File))
                            Settings.Default.KnownTagsFileName = txt_KnownTagsFileName.Text;
                    if (!chk_ImplementSQL.Checked || (chk_ImplementSQL.Checked && chk_SqlButUrlsFromTxt.Checked))
                        if (Text_Validation(txt_UrlsFileName, TxtVal.File))
                            Settings.Default.URLsFileName = txt_UrlsFileName.Text;
                    //DB connection
                    if (chk_ImplementSQL.Checked)
                        if (Text_Validation(txt_DBConnectionString, TxtVal.DB))
                            Settings.Default.DBConnectionString = txt_DBConnectionString.Text;
                    //Free
                    Settings.Default.MaruToHiraganaN = txt_MaruToHiraganaN.Text;
                    Settings.Default.MaruToKatakanaN = txt_MaruToKatakanaN.Text;
                    
                    /*Form*/
                    Settings.Default.UXS_Size = Size;
                    Settings.Default.UXS_Location = Location;
                    Settings.Default.UXS_WindowState = WindowState;

                    /**********************/
                    Settings.Default.Save();
                    break;
            }
        }

        private void Btn_BrowseSavePath_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                txt_SavePath.Text = folderBrowserDialog1.SelectedPath;
        }

        private void Btn_TestDBConnection_Click(object sender, EventArgs e)
        {
            var check_res = true;
            var text = txt_DBConnectionString.Text.Trim();
            var msg = "Valid input string, connection to database established.";

            try
            {
                Helpers.GetConnection(text);
            }
            catch (ArgumentException ex)
            {
                check_res = false;
                msg = ex.Message;
            }
            catch (SqlException ex)
            {
                check_res = false;
                msg = ex.Message;
            }

            if (!check_res)
                MessageBox.Show(msg + $" ({txt_DBConnectionString.Name[4..]})", "Database connection validation error");
            else
                MessageBox.Show(msg, "Database connection validation successful");
        }

        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            _lastOpenedTab = (sender as TabControl).SelectedIndex;
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var chkBox = sender as CheckBox;

            foreach (TabPage tabPage in tabControl.Controls)
                foreach (Control tPControl in tabPage.Controls)
                    if (tPControl.Tag == (object)chkBox.Name)
                        tPControl.Enabled = chkBox.Checked;
        }

        private void CheckBox_EnabledChanged(object sender, EventArgs e)
        {
            var chkBox = sender as CheckBox;

            foreach (TabPage tabPage in tabControl.Controls)
                foreach (Control tPControl in tabPage.Controls)
                    if (tPControl.Tag == (object)chkBox.Name)
                        tPControl.Enabled = chkBox.Enabled && chkBox.Checked;
            /*
            checked enabled desired_result
            true    true    true
            true    false   false
            false   true    false
            false   false   false
            */
        }

        #endregion

        #region Created

        private static bool Text_Validation(TextBox sender, TxtVal val)
        {
            var text = sender.Text.Trim();
            var msg = "Empty strings are not a valid input.";

            var check_res = true;

            if (!string.IsNullOrEmpty(text))
                switch (val)
                {
                    case TxtVal.NameFormat:
                        if (text.EndsWith(".txt"))
                            text = text[0..^4];

                        //use regex to remove {.*}
                        text = new Regex("{.*?}").Replace(text, "");

                        var text2 = Helpers.CheckChars(text);
                        check_res = text == text2;

                        if (!check_res)
                            msg = "Invalid path characters where used.";
                        break;

                    case TxtVal.DateFormat:
                        try
                        {
                            var check_date = DateTime.Now.ToString(text);
                        }
                        catch (FormatException)
                        {
                            check_res = false;
                            msg = "Invalid date format.";
                        }
                        break;

                    case TxtVal.Path:
                        if (!text.EndsWith("\\"))
                            text += "\\";

                        check_res = Directory.Exists(text);

                        if (check_res)
                            sender.Text = text;
                        else
                            msg = "Directory not found.";
                        break;

                    case TxtVal.File:
                        if (text.EndsWith(".txt"))
                            text = text[0..^4];

                        sender.Text = Helpers.CheckChars(text);
                        check_res = text == sender.Text;

                        if (check_res)
                        {
                            check_res = File.Exists(Settings.Default.SavePath + text + ".txt");

                            if (!check_res)
                                msg = "File doesn't exist.";
                        }
                        else
                            msg = "Invalid path characters where used.";
                        break;

                    case TxtVal.DB:
                        try
                        {
                            Helpers.GetConnection(text);
                        }
                        catch (ArgumentException e)
                        {
                            check_res = false;
                            msg = e.Message;
                        }
                        catch (SqlException e)
                        {
                            check_res = false;
                            msg = e.Message;
                        }
                        break;

                    default:
                        check_res = false;
                        msg = "Validation method not implemented.";
                        break;
                }
            else
                check_res = false;

            if (!check_res)
                MessageBox.Show(msg + $" ({sender.Name[4..]})", "Settings validation error");

            return check_res;
        }

        #endregion
    }
}
