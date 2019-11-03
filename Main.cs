using System.Windows.Forms;

namespace SyosetuScraper
{
    public partial class Main : Form
    {
        public Main()
        {
            //InitializeComponent();
            Scraping.Crawl();
            MessageBox.Show("Download Complete");
        }
    }
}