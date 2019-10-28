using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyosetuScraper
{
    public partial class Form1 : Form
    {
        private const string _URL1 = "https://novel18.syosetu.com/n8641dj/";
        private const string _URL2 = "https://novel18.syosetu.com/n1034fj/";
        private const string _URL3 = "https://novel18.syosetu.com/n1596cj/";
        private const string _URL4 = "https://novel18.syosetu.com/n0880fl/";
        private const string _URL5 = "https://novel18.syosetu.com/n8977fo/";

        public Form1()
        {
            InitializeComponent();
            new Scraping().Crawl(new string[] { _URL1, _URL2, _URL3, _URL4, _URL5 });
        }
    }
}
