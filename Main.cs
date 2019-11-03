using System.Windows.Forms;

namespace SyosetuScraper
{
    public partial class Main : Form
    {
        public Main()
        {
            //InitializeComponent();
            Scraping.CrawlAsync();
            MessageBox.Show("Download Complete");
        }
    }
}