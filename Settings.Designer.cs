﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SyosetuScraper {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0")]
        public global::System.Drawing.Point Main_Location {
            get {
                return ((global::System.Drawing.Point)(this["Main_Location"]));
            }
            set {
                this["Main_Location"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0")]
        public global::System.Drawing.Size Main_Size {
            get {
                return ((global::System.Drawing.Size)(this["Main_Size"]));
            }
            set {
                this["Main_Size"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SaveOnExit {
            get {
                return ((bool)(this["SaveOnExit"]));
            }
            set {
                this["SaveOnExit"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CF_Volume {
            get {
                return ((bool)(this["CF_Volume"]));
            }
            set {
                this["CF_Volume"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool CF_Category {
            get {
                return ((bool)(this["CF_Category"]));
            }
            set {
                this["CF_Category"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{Nickname} - {Name}")]
        public string NovelFolderNameFormat {
            get {
                return ((string)(this["NovelFolderNameFormat"]));
            }
            set {
                this["NovelFolderNameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{Number} - {Name}")]
        public string VolumeFolderNameFormat {
            get {
                return ((string)(this["VolumeFolderNameFormat"]));
            }
            set {
                this["VolumeFolderNameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{Id}-{Page} - {Name}")]
        public string ChapterFileNameFormat {
            get {
                return ((string)(this["ChapterFileNameFormat"]));
            }
            set {
                this["ChapterFileNameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{Id}-{Page}-{Id_Image}")]
        public string ImageFileNameFormat {
            get {
                return ((string)(this["ImageFileNameFormat"]));
            }
            set {
                this["ImageFileNameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DivideChapterByPages {
            get {
                return ((bool)(this["DivideChapterByPages"]));
            }
            set {
                this["DivideChapterByPages"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5000")]
        public int PageMaxLength {
            get {
                return ((int)(this["PageMaxLength"]));
            }
            set {
                this["PageMaxLength"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IncludeChapterTitle {
            get {
                return ((bool)(this["IncludeChapterTitle"]));
            }
            set {
                this["IncludeChapterTitle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IncludeAuthorNote {
            get {
                return ((bool)(this["IncludeAuthorNote"]));
            }
            set {
                this["IncludeAuthorNote"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IncludeFootnotes {
            get {
                return ((bool)(this["IncludeFootnotes"]));
            }
            set {
                this["IncludeFootnotes"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\Syosetu Novels\\2020-10-24\\")]
        public string SavePath {
            get {
                return ((string)(this["SavePath"]));
            }
            set {
                this["SavePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DLImages {
            get {
                return ((bool)(this["DLImages"]));
            }
            set {
                this["DLImages"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool CF_Series {
            get {
                return ((bool)(this["CF_Series"]));
            }
            set {
                this["CF_Series"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CreateIndex {
            get {
                return ((bool)(this["CreateIndex"]));
            }
            set {
                this["CreateIndex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_Index.txt")]
        public string IndexFileNameFormat {
            get {
                return ((string)(this["IndexFileNameFormat"]));
            }
            set {
                this["IndexFileNameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("URLs")]
        public string URLsFileName {
            get {
                return ((string)(this["URLsFileName"]));
            }
            set {
                this["URLsFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ScrapeTags {
            get {
                return ((bool)(this["ScrapeTags"]));
            }
            set {
                this["ScrapeTags"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ReplaceKnownTags {
            get {
                return ((bool)(this["ReplaceKnownTags"]));
            }
            set {
                this["ReplaceKnownTags"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Tags")]
        public string KnownTagsFileName {
            get {
                return ((string)(this["KnownTagsFileName"]));
            }
            set {
                this["KnownTagsFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool CF_Author {
            get {
                return ((bool)(this["CF_Author"]));
            }
            set {
                this["CF_Author"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool OnlyNovelInfo {
            get {
                return ((bool)(this["OnlyNovelInfo"]));
            }
            set {
                this["OnlyNovelInfo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IndexInNovelFolder {
            get {
                return ((bool)(this["IndexInNovelFolder"]));
            }
            set {
                this["IndexInNovelFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SaveUnknownTags {
            get {
                return ((bool)(this["SaveUnknownTags"]));
            }
            set {
                this["SaveUnknownTags"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("UnknownTags.txt")]
        public string UnknownTagsFileName {
            get {
                return ((string)(this["UnknownTagsFileName"]));
            }
            set {
                this["UnknownTagsFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AppendUnknownTags {
            get {
                return ((bool)(this["AppendUnknownTags"]));
            }
            set {
                this["AppendUnknownTags"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AdditionalNovelInfo {
            get {
                return ((bool)(this["AdditionalNovelInfo"]));
            }
            set {
                this["AdditionalNovelInfo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("dd/MM/yyyy")]
        public string DateTimeFormat {
            get {
                return ((string)(this["DateTimeFormat"]));
            }
            set {
                this["DateTimeFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("60")]
        public int OngoingStatusLength {
            get {
                return ((int)(this["OngoingStatusLength"]));
            }
            set {
                this["OngoingStatusLength"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("365")]
        public int HiatusStatusLength {
            get {
                return ((int)(this["HiatusStatusLength"]));
            }
            set {
                this["HiatusStatusLength"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public int Workers {
            get {
                return ((int)(this["Workers"]));
            }
            set {
                this["Workers"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0")]
        public global::System.Drawing.Point UXS_Location {
            get {
                return ((global::System.Drawing.Point)(this["UXS_Location"]));
            }
            set {
                this["UXS_Location"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0")]
        public global::System.Drawing.Size UXS_Size {
            get {
                return ((global::System.Drawing.Size)(this["UXS_Size"]));
            }
            set {
                this["UXS_Size"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ImplementSQL {
            get {
                return ((bool)(this["ImplementSQL"]));
            }
            set {
                this["ImplementSQL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DBConnectionString {
            get {
                return ((string)(this["DBConnectionString"]));
            }
            set {
                this["DBConnectionString"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SqlButUrlsFromTxt {
            get {
                return ((bool)(this["SqlButUrlsFromTxt"]));
            }
            set {
                this["SqlButUrlsFromTxt"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("オチ〇ポ;オマ〇コ;チ〇ポ;マ〇コ;チ〇チン;オチ〇チン")]
        public string MaruToKatakanaN {
            get {
                return ((string)(this["MaruToKatakanaN"]));
            }
            set {
                this["MaruToKatakanaN"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("おち〇ぽ;おま〇こ;ち〇ぽ;ま〇こ;ち〇ちん;おち〇ちん")]
        public string MaruToHiraganaN {
            get {
                return ((string)(this["MaruToHiraganaN"]));
            }
            set {
                this["MaruToHiraganaN"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RC_ChapterContent {
            get {
                return ((bool)(this["RC_ChapterContent"]));
            }
            set {
                this["RC_ChapterContent"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RC_NovelTitle {
            get {
                return ((bool)(this["RC_NovelTitle"]));
            }
            set {
                this["RC_NovelTitle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RC_SeriesTitle {
            get {
                return ((bool)(this["RC_SeriesTitle"]));
            }
            set {
                this["RC_SeriesTitle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RC_AuthorName {
            get {
                return ((bool)(this["RC_AuthorName"]));
            }
            set {
                this["RC_AuthorName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int UXS_LastOpenedTab {
            get {
                return ((int)(this["UXS_LastOpenedTab"]));
            }
            set {
                this["UXS_LastOpenedTab"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Dead Links")]
        public string DeadLinkFileNameFormat {
            get {
                return ((string)(this["DeadLinkFileNameFormat"]));
            }
            set {
                this["DeadLinkFileNameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Invalid Chapter - {Number} - {Name}")]
        public string InvalidChapterFileNameFormat {
            get {
                return ((string)(this["InvalidChapterFileNameFormat"]));
            }
            set {
                this["InvalidChapterFileNameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NoChapterAlreadyDL {
            get {
                return ((bool)(this["NoChapterAlreadyDL"]));
            }
            set {
                this["NoChapterAlreadyDL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string GoogleCredentials {
            get {
                return ((string)(this["GoogleCredentials"]));
            }
            set {
                this["GoogleCredentials"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CreateFolder {
            get {
                return ((bool)(this["CreateFolder"]));
            }
            set {
                this["CreateFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RemoveCensorship {
            get {
                return ((bool)(this["RemoveCensorship"]));
            }
            set {
                this["RemoveCensorship"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RC_NovelDescription {
            get {
                return ((bool)(this["RC_NovelDescription"]));
            }
            set {
                this["RC_NovelDescription"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RC_SeriesDescription {
            get {
                return ((bool)(this["RC_SeriesDescription"]));
            }
            set {
                this["RC_SeriesDescription"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RC_Tags {
            get {
                return ((bool)(this["RC_Tags"]));
            }
            set {
                this["RC_Tags"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DLChapterIfModified {
            get {
                return ((bool)(this["DLChapterIfModified"]));
            }
            set {
                this["DLChapterIfModified"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool GoogleAPI {
            get {
                return ((bool)(this["GoogleAPI"]));
            }
            set {
                this["GoogleAPI"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TL_KeepOriginalAsWell {
            get {
                return ((bool)(this["TL_KeepOriginalAsWell"]));
            }
            set {
                this["TL_KeepOriginalAsWell"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TL_NovelTitle {
            get {
                return ((bool)(this["TL_NovelTitle"]));
            }
            set {
                this["TL_NovelTitle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TL_NovelDescription {
            get {
                return ((bool)(this["TL_NovelDescription"]));
            }
            set {
                this["TL_NovelDescription"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TL_SeriesTitle {
            get {
                return ((bool)(this["TL_SeriesTitle"]));
            }
            set {
                this["TL_SeriesTitle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TL_SeriesDescription {
            get {
                return ((bool)(this["TL_SeriesDescription"]));
            }
            set {
                this["TL_SeriesDescription"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TL_AuthorName {
            get {
                return ((bool)(this["TL_AuthorName"]));
            }
            set {
                this["TL_AuthorName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TL_VolumeName {
            get {
                return ((bool)(this["TL_VolumeName"]));
            }
            set {
                this["TL_VolumeName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TL_ChapterTitle {
            get {
                return ((bool)(this["TL_ChapterTitle"]));
            }
            set {
                this["TL_ChapterTitle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TL_ChapterContent {
            get {
                return ((bool)(this["TL_ChapterContent"]));
            }
            set {
                this["TL_ChapterContent"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RC_ChapterTitle {
            get {
                return ((bool)(this["RC_ChapterTitle"]));
            }
            set {
                this["RC_ChapterTitle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RC_VolumeName {
            get {
                return ((bool)(this["RC_VolumeName"]));
            }
            set {
                this["RC_VolumeName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Normal")]
        public global::System.Windows.Forms.FormWindowState Main_WindowState {
            get {
                return ((global::System.Windows.Forms.FormWindowState)(this["Main_WindowState"]));
            }
            set {
                this["Main_WindowState"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Normal")]
        public global::System.Windows.Forms.FormWindowState UXS_WindowState {
            get {
                return ((global::System.Windows.Forms.FormWindowState)(this["UXS_WindowState"]));
            }
            set {
                this["UXS_WindowState"] = value;
            }
        }
    }
}
