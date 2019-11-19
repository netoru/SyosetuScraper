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
        public List<Chapter> Chapters { get; } = new List<Chapter>();

        public Volume(int getId, int getNumber, string getName, string getLink) => (Id, Number, Name, _link) = (getId, getNumber, getName, getLink);

        public void GetVolume(List<HtmlNode> list)
        {
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

                Chapters.Add(new Chapter(chapterId, i + 1, chapterNode.InnerText, _link + chapterId + "/"));
            }

            if (Settings.Default.GetOnlyNovelInfo)
                return;

            foreach (var chapter in Chapters)
            {
                chapter.CheckValidity();

                if (chapter.Valid)
                    chapter.GetChapter();
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

        public void Save(string path)
        {
            if (Settings.Default.VolumeEqFolder)
                if (!string.IsNullOrEmpty(Name))
                {
                    var volumePath = Settings.Default.VolumeFolderNameFormat;
                    volumePath = volumePath.Replace("{Id}", Id.ToString());
                    volumePath = volumePath.Replace("{Number}", Number.ToString());
                    volumePath = volumePath.Replace("{Name}", Name);
                    volumePath = volumePath.Replace("{Chapters}", Chapters.Count.ToString());

                    path += "\\" + Novel.CheckChars(volumePath);
                    Directory.CreateDirectory(path);
                }

            foreach (var chapter in Chapters)
                chapter.Save(path);
        }
    }
}