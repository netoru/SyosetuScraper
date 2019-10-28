using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
            var i = 1;
            foreach (var item in list)
            {
                var chapterNode = item.ChildNodes
                    .Where(n => n.Name == "dd").First()
                    .ChildNodes.Where(n => n.Name == "a")
                    .First();

                var groups = Regex.Match(chapterNode.OuterHtml, "<a href=\"/.+/(\\d+)/\">").Groups;

                if (groups.Count != 2)
                    continue;

                var res = int.TryParse(groups[1].Value, out var chapterId);

                if (!res)
                    continue;

                Chapters.Add(new Chapter(chapterId, i, chapterNode.InnerText, _link + chapterId + "/"));
                i++;
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
    }
}
