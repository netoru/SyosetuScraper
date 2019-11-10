using System;
using System.Collections.Generic;
using System.Text;

namespace SyosetuScraper
{
    class Series
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Novel> Novels { get; set; }
    }
}
