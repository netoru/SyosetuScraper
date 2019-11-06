using System.Windows.Forms;

namespace SyosetuScraper
{
    public partial class Main : Form
    {
        public Main()
        {
            //InitializeComponent();
            var end = Scraping.CrawlAsync().Result;
            var msg = end ? "Download Complete" : "Operation Failed";
            MessageBox.Show(msg);
            Settings.Default.Save();
        }
    }
}