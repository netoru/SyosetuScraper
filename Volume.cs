using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Web;
using Google.Cloud.Translation.V2;

namespace SyosetuScraper
{
    public class Volume
    {
        public int Id { get; }
        public int Number { get; }
        public string Name { get; }
        public string tlName { get; set; }
        private string _link { get; }
        public List<Chapter> Chapters { get; private set; } = new List<Chapter>();

        private string _volumePath;

        public Volume(int getId, int getNumber, string getName, string getLink, string getPath)
        {
            Id = getId;
            Number = getNumber;
            Name = getName;
            _link = getLink;
            _volumePath = getPath;

            if (!string.IsNullOrEmpty(Name) && Settings.Default.GoogleAPI && Settings.Default.TL_VolumeName)
            {
                TranslationResult result = Main.gClient.TranslateText(Name, LanguageCodes.English, LanguageCodes.Japanese);
                tlName = result.TranslatedText;
            }
            if (!string.IsNullOrEmpty(tlName) && Settings.Default.GoogleAPI && !Settings.Default.TL_KeepOriginalAsWell)
                Name = tlName;
        }

        public void GetVolume(List<HtmlNode> list)
        {
            DivideChaptersByVolume();

            for (int i = 0; i < list.Count; i++)
            {
                var chapterNode = list[i].ChildNodes
                    .Where(n => n.Name == "dd").First()
                    .ChildNodes.Where(n => n.Name == "a")
                    .First();

                var groups = Regex.Match(chapterNode.OuterHtml, "<a href=\"/.+/(\\d+)/\">").Groups;

                if (groups.Count != 2)
                    continue;

                var res = int.TryParse(groups[1].Value, out var chapterId);

                if (!res)
                    continue;

                Chapters.Add(new Chapter(chapterId, i + 1, Helpers.RemoveCensorship(HttpUtility.HtmlDecode(chapterNode.InnerText.Trim())), _link + chapterId + "/", _volumePath));
            }

            if (Settings.Default.OnlyNovelInfo)
                return;

            Directory.CreateDirectory(_volumePath);

            foreach (var chapter in Chapters)
            {
                chapter.CheckValidity();

                if (chapter.Valid == 0)
                    chapter.GetChapter();
                else
                    InvalidChapter(chapter.Id, chapter.Name, chapter.Link, chapter.Valid);
            }
        }

        public void GetVolume(HtmlNode node)
        {
            Chapters.Add(new Chapter(1, 0, Name, string.Empty, _volumePath));

            if (Settings.Default.OnlyNovelInfo)
                return;

            Chapters[0].GetChapter(node);
        }

        public string ToString(bool tlToc)
        {
            var txt = new StringBuilder();
            var indent = "";
            var volName = tlToc ? tlName : Name;

            if (!string.IsNullOrEmpty(volName) && Id > -1)
            {
                txt.AppendLine(Number + ". " + volName);
                indent = "\t";
            }

            //add option to use chapter.Number instead of Id
            //to have the # of the chapter in a volume
            foreach (var chapter in Chapters)
                txt.AppendLine(indent + chapter.Id + ". " + (tlToc ? chapter.tlName : chapter.Name));

            return txt.ToString();
        }

        private void DivideChaptersByVolume()
        {
            if (!(Settings.Default.CreateFolder && Settings.Default.CF_Volume) || string.IsNullOrEmpty(Name))
                return;

            var volumeFolderName = Helpers.GenerateFileName(Settings.Default.VolumeFolderNameFormat, this);

            _volumePath += "\\" + Helpers.CheckChars(volumeFolderName);
        }

        private void InvalidChapter(int chapId, string chapName, string chapLink, int errorCode)
        {
            /*
             * Legend:
             * -1 default
             * 0 everything's good
             * 1 chapter name is null
             * 2 chapter id is null
             * 3 chapter name discrepancy between page and index
             * 4 chapter id discrepancy between page and index
             * 5 chapter already downloaded
             */

            if (errorCode == 5)
                return;

            var invalidChapterFileName = Helpers.GenerateFileName(Settings.Default.InvalidChapterFileNameFormat, this);

            if (string.IsNullOrEmpty(invalidChapterFileName))
                invalidChapterFileName = "Invalid Chapter";

            invalidChapterFileName = Helpers.CheckChars(invalidChapterFileName);

            if (!_volumePath.EndsWith("\\"))
                invalidChapterFileName = "\\" + invalidChapterFileName;

            invalidChapterFileName = _volumePath + invalidChapterFileName + ".txt";

            var line = $"[EC#{errorCode}] Couldn't download chapter [{chapId} - {chapName}], link {chapLink}";

            if (!File.Exists(invalidChapterFileName))
            {
                TextWriter tw = new StreamWriter(invalidChapterFileName);
                tw.WriteLine(line);
                tw.Close();
            }
            else if (File.Exists(invalidChapterFileName))
                using (var tw = new StreamWriter(invalidChapterFileName, true))
                    tw.WriteLine(line);
        }
    }
}