using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SyosetuScraper
{
    class Volume
    {
        public int Id { get; }
        public int Number { get; }
        public string Name { get; }
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

                Chapters.Add(new Chapter(chapterId, i + 1, chapterNode.InnerText, _link + chapterId + "/", _volumePath));
            }

            if (Settings.Default.GetOnlyNovelInfo)
                return;

            Directory.CreateDirectory(_volumePath);

            foreach (var chapter in Chapters)
            {
                chapter.CheckValidity();

                if (chapter.Valid)
                    chapter.GetChapter();

                chapter.Forget();
            }
        }

        public override string ToString()
        {
            var txt = new StringBuilder();
            var indent = "";

            if (!string.IsNullOrEmpty(Name))
            {
                txt.AppendLine(Number + ". " + Name);
                indent = "\t";
            }

            //add option to use chapter.Number instead of Id
            //to have the # of the chapter in a volume
            foreach (var chapter in Chapters)
                txt.AppendLine(indent + chapter.Id + ". " + chapter.Name);

            return txt.ToString();
        }

        public void DivideChaptersByVolume()
        {
            if (!Settings.Default.VolumeEqFolder || string.IsNullOrEmpty(Name))
                return;

            var volumeFolderName = Settings.Default.VolumeFolderNameFormat;
            volumeFolderName = volumeFolderName.Replace("{Id}", Id.ToString());
            volumeFolderName = volumeFolderName.Replace("{Number}", Number.ToString());
            volumeFolderName = volumeFolderName.Replace("{Name}", Name);
            volumeFolderName = volumeFolderName.Replace("{Chapters}", Chapters.Count.ToString());

            _volumePath += "\\" + Novel.CheckChars(volumeFolderName);
        }

        public void Forget() => Chapters = new List<Chapter>();
    }
}