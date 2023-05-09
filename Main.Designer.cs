namespace SyosetuScraper
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.tsmi_Menu = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiM_Settings = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiM_Export = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiM_Import = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiM_About = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiM_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_Scrape = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.dgv_Novels = new System.Windows.Forms.DataGridView();
            this.dgv_Tags = new System.Windows.Forms.DataGridView();
            this.txt_FilterNickname = new System.Windows.Forms.TextBox();
            this.btn_UpdateDGVNovels = new System.Windows.Forms.Button();
            this.nud_FilterNovelID = new System.Windows.Forms.NumericUpDown();
            this.txt_FilterEnglishName = new System.Windows.Forms.TextBox();
            this.txt_FilterJapaneseName = new System.Windows.Forms.TextBox();
            this.cmb_FilterNovelType = new System.Windows.Forms.ComboBox();
            this.cmb_FilterNovelStatus = new System.Windows.Forms.ComboBox();
            this.btn_FilterDGVNovels = new System.Windows.Forms.Button();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tscmi_OpenPage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Novels)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Tags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_FilterNovelID)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_Menu,
            this.tsmi_Scrape});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1133, 24);
            this.menuStrip.TabIndex = 2;
            this.menuStrip.Text = "menuStrip1";
            // 
            // tsmi_Menu
            // 
            this.tsmi_Menu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiM_Settings,
            this.tsmiM_Export,
            this.tsmiM_Import,
            this.toolStripSeparator1,
            this.tsmiM_About,
            this.toolStripSeparator2,
            this.tsmiM_Exit});
            this.tsmi_Menu.Name = "tsmi_Menu";
            this.tsmi_Menu.Size = new System.Drawing.Size(50, 20);
            this.tsmi_Menu.Text = "&Menu";
            // 
            // tsmiM_Settings
            // 
            this.tsmiM_Settings.Name = "tsmiM_Settings";
            this.tsmiM_Settings.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.tsmiM_Settings.Size = new System.Drawing.Size(193, 22);
            this.tsmiM_Settings.Text = "&Settings";
            this.tsmiM_Settings.Click += new System.EventHandler(this.TsmiM_Settings_Click);
            // 
            // tsmiM_Export
            // 
            this.tsmiM_Export.Name = "tsmiM_Export";
            this.tsmiM_Export.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.tsmiM_Export.Size = new System.Drawing.Size(193, 22);
            this.tsmiM_Export.Text = "&Export Settings";
            this.tsmiM_Export.Click += new System.EventHandler(this.TsmiM_Export_Click);
            // 
            // tsmiM_Import
            // 
            this.tsmiM_Import.Name = "tsmiM_Import";
            this.tsmiM_Import.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.tsmiM_Import.Size = new System.Drawing.Size(193, 22);
            this.tsmiM_Import.Text = "&Import Settings";
            this.tsmiM_Import.Click += new System.EventHandler(this.TsmiM_Import_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(190, 6);
            // 
            // tsmiM_About
            // 
            this.tsmiM_About.Name = "tsmiM_About";
            this.tsmiM_About.Size = new System.Drawing.Size(193, 22);
            this.tsmiM_About.Text = "About";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(190, 6);
            // 
            // tsmiM_Exit
            // 
            this.tsmiM_Exit.Name = "tsmiM_Exit";
            this.tsmiM_Exit.Size = new System.Drawing.Size(193, 22);
            this.tsmiM_Exit.Text = "Exit";
            // 
            // tsmi_Scrape
            // 
            this.tsmi_Scrape.Name = "tsmi_Scrape";
            this.tsmi_Scrape.Size = new System.Drawing.Size(54, 20);
            this.tsmi_Scrape.Text = "Scrape";
            this.tsmi_Scrape.Click += new System.EventHandler(this.Tsmi_Scrape_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // dgv_Novels
            // 
            this.dgv_Novels.AllowUserToAddRows = false;
            this.dgv_Novels.AllowUserToDeleteRows = false;
            this.dgv_Novels.AllowUserToOrderColumns = true;
            this.dgv_Novels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_Novels.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Novels.Location = new System.Drawing.Point(12, 56);
            this.dgv_Novels.Name = "dgv_Novels";
            this.dgv_Novels.RowTemplate.Height = 25;
            this.dgv_Novels.Size = new System.Drawing.Size(800, 545);
            this.dgv_Novels.TabIndex = 3;
            this.dgv_Novels.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.Dgv_Novels_CellValueChanged);
            this.dgv_Novels.SelectionChanged += new System.EventHandler(this.Dgv_Novels_SelectionChanged);
            this.dgv_Novels.Sorted += new System.EventHandler(this.Dgv_Novels_Sorted);
            // 
            // dgv_Tags
            // 
            this.dgv_Tags.AllowUserToAddRows = false;
            this.dgv_Tags.AllowUserToDeleteRows = false;
            this.dgv_Tags.AllowUserToOrderColumns = true;
            this.dgv_Tags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_Tags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Tags.Location = new System.Drawing.Point(818, 56);
            this.dgv_Tags.Name = "dgv_Tags";
            this.dgv_Tags.RowTemplate.Height = 25;
            this.dgv_Tags.Size = new System.Drawing.Size(303, 545);
            this.dgv_Tags.TabIndex = 4;
            // 
            // txt_FilterNickname
            // 
            this.txt_FilterNickname.Location = new System.Drawing.Point(78, 27);
            this.txt_FilterNickname.Name = "txt_FilterNickname";
            this.txt_FilterNickname.PlaceholderText = "Nickname";
            this.txt_FilterNickname.Size = new System.Drawing.Size(120, 23);
            this.txt_FilterNickname.TabIndex = 5;
            // 
            // btn_UpdateDGVNovels
            // 
            this.btn_UpdateDGVNovels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_UpdateDGVNovels.Location = new System.Drawing.Point(1046, 25);
            this.btn_UpdateDGVNovels.Name = "btn_UpdateDGVNovels";
            this.btn_UpdateDGVNovels.Size = new System.Drawing.Size(75, 23);
            this.btn_UpdateDGVNovels.TabIndex = 6;
            this.btn_UpdateDGVNovels.Text = "Update";
            this.btn_UpdateDGVNovels.UseVisualStyleBackColor = true;
            this.btn_UpdateDGVNovels.Click += new System.EventHandler(this.Btn_UpdateDGVNovels_Click);
            // 
            // nud_FilterNovelID
            // 
            this.nud_FilterNovelID.Location = new System.Drawing.Point(12, 27);
            this.nud_FilterNovelID.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.nud_FilterNovelID.Name = "nud_FilterNovelID";
            this.nud_FilterNovelID.Size = new System.Drawing.Size(60, 23);
            this.nud_FilterNovelID.TabIndex = 7;
            // 
            // txt_FilterEnglishName
            // 
            this.txt_FilterEnglishName.Location = new System.Drawing.Point(204, 27);
            this.txt_FilterEnglishName.Name = "txt_FilterEnglishName";
            this.txt_FilterEnglishName.PlaceholderText = "English Name";
            this.txt_FilterEnglishName.Size = new System.Drawing.Size(120, 23);
            this.txt_FilterEnglishName.TabIndex = 8;
            // 
            // txt_FilterJapaneseName
            // 
            this.txt_FilterJapaneseName.Location = new System.Drawing.Point(330, 27);
            this.txt_FilterJapaneseName.Name = "txt_FilterJapaneseName";
            this.txt_FilterJapaneseName.PlaceholderText = "Japanese Name";
            this.txt_FilterJapaneseName.Size = new System.Drawing.Size(120, 23);
            this.txt_FilterJapaneseName.TabIndex = 9;
            // 
            // cmb_FilterNovelType
            // 
            this.cmb_FilterNovelType.FormattingEnabled = true;
            this.cmb_FilterNovelType.Location = new System.Drawing.Point(456, 27);
            this.cmb_FilterNovelType.Name = "cmb_FilterNovelType";
            this.cmb_FilterNovelType.Size = new System.Drawing.Size(120, 23);
            this.cmb_FilterNovelType.TabIndex = 10;
            // 
            // cmb_FilterNovelStatus
            // 
            this.cmb_FilterNovelStatus.FormattingEnabled = true;
            this.cmb_FilterNovelStatus.Location = new System.Drawing.Point(582, 27);
            this.cmb_FilterNovelStatus.Name = "cmb_FilterNovelStatus";
            this.cmb_FilterNovelStatus.Size = new System.Drawing.Size(120, 23);
            this.cmb_FilterNovelStatus.TabIndex = 11;
            // 
            // btn_FilterDGVNovels
            // 
            this.btn_FilterDGVNovels.Location = new System.Drawing.Point(737, 27);
            this.btn_FilterDGVNovels.Name = "btn_FilterDGVNovels";
            this.btn_FilterDGVNovels.Size = new System.Drawing.Size(75, 23);
            this.btn_FilterDGVNovels.TabIndex = 13;
            this.btn_FilterDGVNovels.Text = "Filter";
            this.btn_FilterDGVNovels.UseVisualStyleBackColor = true;
            this.btn_FilterDGVNovels.Click += new System.EventHandler(this.Btn_FilterDGVNovels_Click);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tscmi_OpenPage});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(133, 26);
            // 
            // tscmi_OpenPage
            // 
            this.tscmi_OpenPage.Name = "tscmi_OpenPage";
            this.tscmi_OpenPage.Size = new System.Drawing.Size(132, 22);
            this.tscmi_OpenPage.Text = "Open Page";
            this.tscmi_OpenPage.Click += new System.EventHandler(this.Tscmi_OpenPage_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1133, 613);
            this.Controls.Add(this.btn_FilterDGVNovels);
            this.Controls.Add(this.cmb_FilterNovelStatus);
            this.Controls.Add(this.cmb_FilterNovelType);
            this.Controls.Add(this.txt_FilterJapaneseName);
            this.Controls.Add(this.txt_FilterEnglishName);
            this.Controls.Add(this.nud_FilterNovelID);
            this.Controls.Add(this.btn_UpdateDGVNovels);
            this.Controls.Add(this.txt_FilterNickname);
            this.Controls.Add(this.dgv_Tags);
            this.Controls.Add(this.dgv_Novels);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "小説家になろう Scraper";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClose);
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Novels)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Tags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_FilterNovelID)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem tsmi_Menu;
        private System.Windows.Forms.ToolStripMenuItem tsmiM_Settings;
        private System.Windows.Forms.ToolStripMenuItem tsmiM_Export;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsmiM_Exit;
        private System.Windows.Forms.ToolStripMenuItem tsmiM_About;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.ToolStripMenuItem tsmiM_Import;
        private System.Windows.Forms.DataGridView dgv_Novels;
        private System.Windows.Forms.DataGridView dgv_Tags;
        private System.Windows.Forms.TextBox txt_FilterNickname;
        private System.Windows.Forms.Button btn_UpdateDGVNovels;
        private System.Windows.Forms.ToolStripMenuItem tsmi_Scrape;
        private System.Windows.Forms.NumericUpDown nud_FilterNovelID;
        private System.Windows.Forms.TextBox txt_FilterEnglishName;
        private System.Windows.Forms.TextBox txt_FilterJapaneseName;
        private System.Windows.Forms.ComboBox cmb_FilterNovelType;
        private System.Windows.Forms.ComboBox cmb_FilterNovelStatus;
        private System.Windows.Forms.Button btn_FilterDGVNovels;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem tscmi_OpenPage;
    }
}