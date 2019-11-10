using System;
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

        private void OnFormClose(object sender, EventArgs e)
        {
            if (Settings.Default.RememberSettings)
            {
                Settings.Default.Size = Size;
                Settings.Default.Location = Location;

                Settings.Default.Save();
            }
        }
    }
}