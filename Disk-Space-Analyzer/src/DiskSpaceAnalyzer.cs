using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DiskSpaceAnalyzerApp
{
    public class DiskSpaceAnalyzerForm : Form
    {
        TextBox txtFolder;
        TextBox txtSearch;

        ComboBox cbScanMode;
        ComboBox cbTypeFilter;
        ComboBox cbSizeFilter;
        ComboBox cbSort;

        ListBox lbLargestFiles;
        ListBox lbLargestFolders;
        ListBox lbTypeSummary;
        ListBox lbExtensionSummary;
        ListBox lbAgeSummary;

        Button btnBrowse;
        Button btnDownloads;
        Button btnDesktop;
        Button btnDocuments;
        Button btnScan;
        Button btnStop;
        Button btnRefresh;
        Button btnSearch;
        Button btnShowAll;
        Button btnSelectAllFiles;
        Button btnSelectAllFolders;
        Button btnOpenFile;
        Button btnOpenLocation;
        Button btnCopyPath;
        Button btnMoveSelected;
        Button btnDeleteSelected;
        Button btnExport;
        Button btnHowToUse;
        Button btnReset;

        Label lblTotalSize;
        Label lblFilesCount;
        Label lblFoldersCount;
        Label lblLargestFile;
        Label lblMostUsedType;
        Label lblStatus;
        Label lblSmartMessage;
        Label lblScanInfo;

        ProgressBar progressBar;

        List<FileRecord> files = new List<FileRecord>();
        List<FolderRecord> folders = new List<FolderRecord>();
        Dictionary<string, long> typeTotals = new Dictionary<string, long>();
        Dictionary<string, int> typeCounts = new Dictionary<string, int>();
        Dictionary<string, long> extensionTotals = new Dictionary<string, long>();
        Dictionary<string, int> extensionCounts = new Dictionary<string, int>();

        bool stopScan = false;
        int scannedFiles = 0;
        int scannedFolders = 0;
        DateTime lastScanDate = DateTime.MinValue;

        static string userName = Environment.UserName;
        static string dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            userName + "_DiskSpaceAnalyzer_Data"
        );

        static string settingsFile = Path.Combine(dataFolder, "settings.txt");

        Color bg = Color.FromArgb(235, 241, 247);
        Color dark = Color.FromArgb(25, 42, 65);
        Color navy = Color.FromArgb(20, 35, 55);
        Color blue = Color.FromArgb(41, 128, 185);
        Color cyan = Color.FromArgb(22, 160, 180);
        Color green = Color.FromArgb(39, 174, 96);
        Color red = Color.FromArgb(192, 57, 43);
        Color orange = Color.FromArgb(243, 156, 18);
        Color gray = Color.FromArgb(127, 140, 141);
        Color purple = Color.FromArgb(91, 75, 138);
        Color card = Color.White;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DiskSpaceAnalyzerForm());
        }

        public DiskSpaceAnalyzerForm()
        {
            PrepareDataFolder();
            BuildUI();
            LoadSettings();
        }

        void BuildUI()
        {
            this.Text = "Disk Space Analyzer";
            this.Size = new Size(1220, 820);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bg;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            this.AutoScroll = true;
            this.AutoScrollMinSize = new Size(1180, 960);

            Label title = new Label();
            title.Text = "Disk Space Analyzer";
            title.Font = new Font("Segoe UI", 25, FontStyle.Bold);
            title.ForeColor = dark;
            title.Location = new Point(30, 18);
            title.Size = new Size(460, 45);
            this.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Analyze storage usage, find large files and folders, and export reports. Developed by Mohammed Ahmed.";
            subtitle.Font = new Font("Segoe UI", 10, FontStyle.Italic);
            subtitle.ForeColor = Color.FromArgb(90, 90, 90);
            subtitle.Location = new Point(35, 62);
            subtitle.Size = new Size(850, 25);
            this.Controls.Add(subtitle);

            btnHowToUse = CreateButton("How To Use", 900, 25, 120, 30, navy);
            btnHowToUse.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnHowToUse.Click += BtnHowToUse_Click;
            this.Controls.Add(btnHowToUse);

            btnReset = CreateButton("Reset Data", 1035, 25, 120, 30, red);
            btnReset.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            Panel folderCard = CreateCard(30, 100, 1125, 115);
            this.Controls.Add(folderCard);

            folderCard.Controls.Add(CreateLabel("Folder:", 20, 20, 70, 25, FontStyle.Bold));

            txtFolder = new TextBox();
            txtFolder.Location = new Point(90, 20);
            txtFolder.Size = new Size(670, 25);
            txtFolder.ReadOnly = true;
            folderCard.Controls.Add(txtFolder);

            btnBrowse = CreateButton("Browse", 775, 16, 95, 32, blue);
            btnBrowse.Click += BtnBrowse_Click;
            folderCard.Controls.Add(btnBrowse);

            btnScan = CreateButton("Scan Folder", 880, 16, 115, 32, green);
            btnScan.Click += BtnScan_Click;
            folderCard.Controls.Add(btnScan);

            btnStop = CreateButton("Stop", 1005, 16, 80, 32, red);
            btnStop.Click += BtnStop_Click;
            folderCard.Controls.Add(btnStop);

            folderCard.Controls.Add(CreateLabel("Quick:", 20, 68, 70, 25, FontStyle.Bold));

            btnDownloads = CreateButton("Downloads", 90, 63, 110, 32, navy);
            btnDownloads.Click += BtnDownloads_Click;
            folderCard.Controls.Add(btnDownloads);

            btnDesktop = CreateButton("Desktop", 210, 63, 95, 32, navy);
            btnDesktop.Click += BtnDesktop_Click;
            folderCard.Controls.Add(btnDesktop);

            btnDocuments = CreateButton("Documents", 315, 63, 110, 32, navy);
            btnDocuments.Click += BtnDocuments_Click;
            folderCard.Controls.Add(btnDocuments);

            folderCard.Controls.Add(CreateLabel("Scan Mode:", 455, 68, 90, 25, FontStyle.Bold));

            cbScanMode = new ComboBox();
            cbScanMode.Location = new Point(550, 68);
            cbScanMode.Size = new Size(150, 25);
            cbScanMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbScanMode.Items.Add("Full Scan");
            cbScanMode.Items.Add("Quick Scan");
            cbScanMode.Items.Add("Large Files Scan");
            cbScanMode.Items.Add("Old Files Scan");
            cbScanMode.SelectedIndex = 0;
            folderCard.Controls.Add(cbScanMode);

            btnRefresh = CreateButton("Refresh", 715, 63, 95, 32, cyan);
            btnRefresh.Click += BtnRefresh_Click;
            folderCard.Controls.Add(btnRefresh);

            lblScanInfo = new Label();
            lblScanInfo.Text = "Ready";
            lblScanInfo.Location = new Point(830, 68);
            lblScanInfo.Size = new Size(270, 25);
            lblScanInfo.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblScanInfo.ForeColor = Color.FromArgb(80, 80, 80);
            folderCard.Controls.Add(lblScanInfo);

            Panel statCard = CreateCard(30, 235, 1125, 90);
            this.Controls.Add(statCard);

            lblTotalSize = CreateStatBox(statCard, "Total Size", "0 KB", 20, 14, blue);
            lblFilesCount = CreateStatBox(statCard, "Files", "0", 245, 14, cyan);
            lblFoldersCount = CreateStatBox(statCard, "Folders", "0", 450, 14, green);
            lblLargestFile = CreateStatBox(statCard, "Largest File", "None", 655, 14, orange);
            lblMostUsedType = CreateStatBox(statCard, "Most Used Type", "None", 895, 14, red);

            Panel smartCard = CreateCard(30, 345, 1125, 65);
            smartCard.BackColor = Color.FromArgb(255, 252, 246);
            this.Controls.Add(smartCard);

            Label smartTitle = CreateLabel("Smart Health Message:", 20, 13, 190, 25, FontStyle.Bold);
            smartTitle.ForeColor = orange;
            smartCard.Controls.Add(smartTitle);

            lblSmartMessage = new Label();
            lblSmartMessage.Text = "Choose a folder and scan it to see storage insights.";
            lblSmartMessage.Location = new Point(210, 13);
            lblSmartMessage.Size = new Size(875, 25);
            lblSmartMessage.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblSmartMessage.ForeColor = dark;
            smartCard.Controls.Add(lblSmartMessage);

            progressBar = new ProgressBar();
            progressBar.Location = new Point(20, 42);
            progressBar.Size = new Size(1080, 12);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Style = ProgressBarStyle.Continuous;
            smartCard.Controls.Add(progressBar);

            Panel filterCard = CreateCard(30, 430, 1125, 70);
            this.Controls.Add(filterCard);

            filterCard.Controls.Add(CreateLabel("Search:", 20, 22, 70, 25, FontStyle.Bold));
            txtSearch = new TextBox();
            txtSearch.Location = new Point(90, 22);
            txtSearch.Size = new Size(185, 25);
            txtSearch.KeyDown += TxtSearch_KeyDown;
            filterCard.Controls.Add(txtSearch);

            btnSearch = CreateButton("Search", 285, 18, 85, 32, blue);
            btnSearch.Click += BtnSearch_Click;
            filterCard.Controls.Add(btnSearch);

            filterCard.Controls.Add(CreateLabel("Type:", 390, 22, 50, 25, FontStyle.Bold));
            cbTypeFilter = new ComboBox();
            cbTypeFilter.Location = new Point(440, 22);
            cbTypeFilter.Size = new Size(135, 25);
            cbTypeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTypeFilter.Items.Add("All");
            cbTypeFilter.Items.Add("Videos");
            cbTypeFilter.Items.Add("Images");
            cbTypeFilter.Items.Add("Documents");
            cbTypeFilter.Items.Add("Applications");
            cbTypeFilter.Items.Add("Archives");
            cbTypeFilter.Items.Add("Audio");
            cbTypeFilter.Items.Add("Code Files");
            cbTypeFilter.Items.Add("Other");
            cbTypeFilter.SelectedIndex = 0;
            cbTypeFilter.SelectedIndexChanged += FilterChanged;
            filterCard.Controls.Add(cbTypeFilter);

            filterCard.Controls.Add(CreateLabel("Size:", 595, 22, 45, 25, FontStyle.Bold));
            cbSizeFilter = new ComboBox();
            cbSizeFilter.Location = new Point(640, 22);
            cbSizeFilter.Size = new Size(110, 25);
            cbSizeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cbSizeFilter.Items.Add("All Sizes");
            cbSizeFilter.Items.Add("10 MB+");
            cbSizeFilter.Items.Add("100 MB+");
            cbSizeFilter.Items.Add("500 MB+");
            cbSizeFilter.Items.Add("1 GB+");
            cbSizeFilter.SelectedIndex = 0;
            cbSizeFilter.SelectedIndexChanged += FilterChanged;
            filterCard.Controls.Add(cbSizeFilter);

            filterCard.Controls.Add(CreateLabel("Sort:", 770, 22, 45, 25, FontStyle.Bold));
            cbSort = new ComboBox();
            cbSort.Location = new Point(815, 22);
            cbSort.Size = new Size(135, 25);
            cbSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cbSort.Items.Add("Largest First");
            cbSort.Items.Add("Smallest First");
            cbSort.Items.Add("Newest First");
            cbSort.Items.Add("Oldest First");
            cbSort.Items.Add("Name A-Z");
            cbSort.SelectedIndex = 0;
            cbSort.SelectedIndexChanged += FilterChanged;
            filterCard.Controls.Add(cbSort);

            btnShowAll = CreateButton("Show All", 970, 18, 100, 32, navy);
            btnShowAll.Click += BtnShowAll_Click;
            filterCard.Controls.Add(btnShowAll);

            Panel filesCard = CreateCard(30, 555, 550, 125);
            this.Controls.Add(filesCard);

            Label largestFilesTitle = CreateLabel("Top 20 Largest Files", 15, 10, 220, 25, FontStyle.Bold);
            largestFilesTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            largestFilesTitle.ForeColor = blue;
            filesCard.Controls.Add(largestFilesTitle);

            btnSelectAllFiles = CreateButton("Select All", 420, 8, 105, 28, gray);
            btnSelectAllFiles.Click += BtnSelectAllFiles_Click;
            filesCard.Controls.Add(btnSelectAllFiles);

            lbLargestFiles = new ListBox();
            lbLargestFiles.Location = new Point(15, 38);
            lbLargestFiles.Size = new Size(520, 75);
            lbLargestFiles.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            lbLargestFiles.SelectionMode = SelectionMode.MultiExtended;
            lbLargestFiles.SelectedIndexChanged += LbLargestFiles_SelectedIndexChanged;
            lbLargestFiles.DoubleClick += LbLargestFiles_DoubleClick;
            filesCard.Controls.Add(lbLargestFiles);

            Panel foldersCard = CreateCard(605, 555, 550, 125);
            this.Controls.Add(foldersCard);

            Label largestFoldersTitle = CreateLabel("Largest Folders", 15, 10, 220, 25, FontStyle.Bold);
            largestFoldersTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            largestFoldersTitle.ForeColor = green;
            foldersCard.Controls.Add(largestFoldersTitle);

            btnSelectAllFolders = CreateButton("Select All", 420, 8, 105, 28, gray);
            btnSelectAllFolders.Click += BtnSelectAllFolders_Click;
            foldersCard.Controls.Add(btnSelectAllFolders);

            lbLargestFolders = new ListBox();
            lbLargestFolders.Location = new Point(15, 38);
            lbLargestFolders.Size = new Size(520, 75);
            lbLargestFolders.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            lbLargestFolders.SelectionMode = SelectionMode.MultiExtended;
            lbLargestFolders.SelectedIndexChanged += LbLargestFolders_SelectedIndexChanged;
            lbLargestFolders.DoubleClick += LbLargestFolders_DoubleClick;
            foldersCard.Controls.Add(lbLargestFolders);

            Panel summaryCard = CreateCard(30, 710, 1125, 145);
            this.Controls.Add(summaryCard);

            Label typeTitle = CreateLabel("Type Summary", 20, 10, 150, 25, FontStyle.Bold);
            typeTitle.ForeColor = cyan;
            summaryCard.Controls.Add(typeTitle);

            lbTypeSummary = new ListBox();
            lbTypeSummary.Location = new Point(20, 40);
            lbTypeSummary.Size = new Size(330, 90);
            lbTypeSummary.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            summaryCard.Controls.Add(lbTypeSummary);

            Label extTitle = CreateLabel("Extension Summary", 395, 10, 170, 25, FontStyle.Bold);
            extTitle.ForeColor = purple;
            summaryCard.Controls.Add(extTitle);

            lbExtensionSummary = new ListBox();
            lbExtensionSummary.Location = new Point(395, 40);
            lbExtensionSummary.Size = new Size(330, 90);
            lbExtensionSummary.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            summaryCard.Controls.Add(lbExtensionSummary);

            Label ageTitle = CreateLabel("File Age Summary", 770, 10, 170, 25, FontStyle.Bold);
            ageTitle.ForeColor = orange;
            summaryCard.Controls.Add(ageTitle);

            lbAgeSummary = new ListBox();
            lbAgeSummary.Location = new Point(770, 40);
            lbAgeSummary.Size = new Size(330, 90);
            lbAgeSummary.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            summaryCard.Controls.Add(lbAgeSummary);

            lblStatus = new Label();
            lblStatus.Text = "Ready";
            lblStatus.Location = new Point(930, 10);
            lblStatus.Size = new Size(170, 22);
            lblStatus.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
            lblStatus.Visible = false;
            summaryCard.Controls.Add(lblStatus);

            btnOpenFile = CreateButton("Open Selected", 30, 510, 105, 30, green);
            btnOpenFile.Click += BtnOpenFile_Click;
            this.Controls.Add(btnOpenFile);

            btnOpenLocation = CreateButton("Open Location", 145, 510, 125, 30, blue);
            btnOpenLocation.Click += BtnOpenLocation_Click;
            this.Controls.Add(btnOpenLocation);

            btnCopyPath = CreateButton("Copy Path", 280, 510, 105, 30, gray);
            btnCopyPath.Click += BtnCopyPath_Click;
            this.Controls.Add(btnCopyPath);

            btnMoveSelected = CreateButton("Move Selected", 395, 510, 130, 30, orange);
            btnMoveSelected.Click += BtnMoveSelected_Click;
            this.Controls.Add(btnMoveSelected);

            btnDeleteSelected = CreateButton("Delete Selected", 535, 510, 135, 30, red);
            btnDeleteSelected.Click += BtnDeleteSelected_Click;
            this.Controls.Add(btnDeleteSelected);

            btnExport = CreateButton("Export Report", 680, 510, 130, 30, cyan);
            btnExport.Click += BtnExport_Click;
            this.Controls.Add(btnExport);

            Label developedBy = new Label();
            developedBy.Text = "Developed by Mohammed Ahmed";
            developedBy.Location = new Point(850, 62);
            developedBy.Size = new Size(310, 25);
            developedBy.TextAlign = ContentAlignment.MiddleRight;
            developedBy.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            developedBy.ForeColor = Color.FromArgb(25, 42, 65);
            this.Controls.Add(developedBy);
        }

        bool IsProtectedPath(string path)
        {
            if (path == null)
                return true;

            string p = path.ToLower();

            string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows).ToLower();
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToLower();
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLower();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToLower();
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToLower();
            string commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).ToLower();

            if (windows != "" && p.StartsWith(windows))
                return true;

            if (programFiles != "" && p.StartsWith(programFiles))
                return true;

            if (programFilesX86 != "" && p.StartsWith(programFilesX86))
                return true;

            if (appData != "" && p.StartsWith(appData))
                return true;

            if (localAppData != "" && p.StartsWith(localAppData))
                return true;

            if (commonAppData != "" && p.StartsWith(commonAppData))
                return true;

            if (p.Contains("\\system32\\"))
                return true;

            if (p.Contains("\\syswow64\\"))
                return true;

            if (p.Contains("\\windowsapps\\"))
                return true;

            if (p.Contains("\\microsoft\\windows\\"))
                return true;

            return false;
        }

        bool IsSafeToDelete(string path)
        {
            if (IsProtectedPath(path))
                return false;

            return true;
        }

        Button CreateButton(string text, int x, int y, int w, int h, Color color)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(w, h);
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            return btn;
        }

        Label CreateLabel(string text, int x, int y, int w, int h, FontStyle style)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(w, h);
            lbl.Font = new Font("Segoe UI", 9, style);
            lbl.ForeColor = dark;
            return lbl;
        }

        Panel CreateCard(int x, int y, int w, int h)
        {
            Panel p = new Panel();
            p.Location = new Point(x, y);
            p.Size = new Size(w, h);
            p.BackColor = card;
            p.BorderStyle = BorderStyle.FixedSingle;
            return p;
        }

        Label CreateStatBox(Panel parent, string label, string value, int x, int y, Color color)
        {
            Label small = new Label();
            small.Text = label;
            small.Location = new Point(x + 8, y);
            small.Size = new Size(180, 22);
            small.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            small.ForeColor = Color.FromArgb(100, 100, 100);
            parent.Controls.Add(small);

            Label big = new Label();
            big.Text = value;
            big.Location = new Point(x + 8, y + 25);
            big.Size = new Size(205, 35);
            big.AutoEllipsis = true;
            big.TextAlign = ContentAlignment.MiddleLeft;
            big.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            big.ForeColor = color;
            parent.Controls.Add(big);

            return big;
        }

        static void PrepareDataFolder()
        {
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            if (!File.Exists(settingsFile))
                File.WriteAllText(settingsFile, "");
        }

        void LoadSettings()
        {
            try
            {
                if (!File.Exists(settingsFile))
                {
                    txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    return;
                }

                string[] lines = File.ReadAllLines(settingsFile);

                foreach (string line in lines)
                {
                    if (line.StartsWith("Folder="))
                    {
                        string folder = line.Substring("Folder=".Length);

                        if (Directory.Exists(folder) && !IsProtectedPath(folder))
                            txtFolder.Text = folder;
                    }

                    if (line.StartsWith("ScanMode="))
                    {
                        string mode = line.Substring("ScanMode=".Length);

                        if (cbScanMode.Items.Contains(mode))
                            cbScanMode.Text = mode;
                    }

                    if (line.StartsWith("TypeFilter="))
                    {
                        string filter = line.Substring("TypeFilter=".Length);

                        if (cbTypeFilter.Items.Contains(filter))
                            cbTypeFilter.Text = filter;
                    }

                    if (line.StartsWith("SizeFilter="))
                    {
                        string filter = line.Substring("SizeFilter=".Length);

                        if (cbSizeFilter.Items.Contains(filter))
                            cbSizeFilter.Text = filter;
                    }

                    if (line.StartsWith("Sort="))
                    {
                        string sort = line.Substring("Sort=".Length);

                        if (cbSort.Items.Contains(sort))
                            cbSort.Text = sort;
                    }
                }

                if (txtFolder.Text.Trim() == "")
                    txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            catch
            {
                txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
        }

        void SaveSettings()
        {
            try
            {
                List<string> lines = new List<string>();
                lines.Add("Folder=" + txtFolder.Text);
                lines.Add("ScanMode=" + cbScanMode.Text);
                lines.Add("TypeFilter=" + cbTypeFilter.Text);
                lines.Add("SizeFilter=" + cbSizeFilter.Text);
                lines.Add("Sort=" + cbSort.Text);

                File.WriteAllLines(settingsFile, lines.ToArray());
            }
            catch
            {
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (Directory.Exists(txtFolder.Text) && !IsProtectedPath(txtFolder.Text))
                dialog.SelectedPath = txtFolder.Text;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (IsProtectedPath(dialog.SelectedPath))
                {
                    MessageBox.Show(
                        "This folder is protected and cannot be scanned for safety reasons." + Environment.NewLine +
                        "Windows, Program Files, AppData, System32, and system locations are blocked.",
                        "Protected Folder Blocked",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                txtFolder.Text = dialog.SelectedPath;
                SaveSettings();
            }
        }

        private void BtnDownloads_Click(object sender, EventArgs e)
        {
            txtFolder.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            SaveSettings();
        }

        private void BtnDesktop_Click(object sender, EventArgs e)
        {
            txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            SaveSettings();
        }

        private void BtnDocuments_Click(object sender, EventArgs e)
        {
            txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            SaveSettings();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtFolder.Text))
                ScanFolder();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            stopScan = true;
            lblStatus.Text = "Stopping scan...";
            lblScanInfo.Text = "Stop requested";
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            ScanFolder();
        }

        void ScanFolder()
        {
            string root = txtFolder.Text;

            if (!Directory.Exists(root))
            {
                MessageBox.Show("Please choose a valid folder first.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IsProtectedPath(root))
            {
                MessageBox.Show(
                    "This folder is protected and cannot be scanned." + Environment.NewLine +
                    "The app blocks Windows, Program Files, AppData, System32, and other system locations for safety.",
                    "Scan Blocked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            stopScan = false;
            scannedFiles = 0;
            scannedFolders = 0;
            lastScanDate = DateTime.Now;

            files.Clear();
            folders.Clear();
            typeTotals.Clear();
            typeCounts.Clear();
            extensionTotals.Clear();
            extensionCounts.Clear();

            ClearDisplay();

            lblStatus.Text = "Scanning...";
            lblScanInfo.Text = "Scanning started...";
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.MarqueeAnimationSpeed = 30;
            Application.DoEvents();

            try
            {
                ScanDirectory(root, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Scan error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 100;

            SaveSettings();
            BuildResults();

            if (stopScan)
                lblStatus.Text = "Scan stopped";
            else
                lblStatus.Text = "Scan complete";

            lblScanInfo.Text = "Files scanned: " + scannedFiles + " | Folders scanned: " + scannedFolders;
        }

        long ScanDirectory(string folderPath, bool isRoot)
        {
            if (IsProtectedPath(folderPath))
                return 0;

            if (stopScan)
                return 0;

            long folderSize = 0;
            scannedFolders++;

            if (scannedFolders % 20 == 0)
            {
                lblScanInfo.Text = "Scanning folders: " + scannedFolders + " | Files: " + scannedFiles;
                Application.DoEvents();
            }

            try
            {
                string[] filePaths = Directory.GetFiles(folderPath);

                foreach (string filePath in filePaths)
                {
                    if (stopScan)
                        break;

                    if (IsProtectedPath(filePath))
                        continue;

                    try
                    {
                        FileInfo info = new FileInfo(filePath);

                        if (!ShouldIncludeByScanMode(info))
                            continue;

                        FileRecord file = new FileRecord();
                        file.Name = info.Name;
                        file.FullPath = info.FullName;
                        file.Extension = info.Extension.ToLower();
                        file.SizeBytes = info.Length;
                        file.LastModified = info.LastWriteTime;
                        file.Category = GetCategory(file.Extension);
                        file.SafetyLabel = GetSafetyLabel(file.FullPath, file.Extension);

                        files.Add(file);
                        folderSize += info.Length;
                        scannedFiles++;

                        AddToSummary(typeTotals, file.Category, file.SizeBytes);
                        AddToCount(typeCounts, file.Category);

                        string ext = file.Extension;

                        if (ext == "")
                            ext = "[no extension]";

                        AddToSummary(extensionTotals, ext, file.SizeBytes);
                        AddToCount(extensionCounts, ext);

                        if (scannedFiles % 100 == 0)
                        {
                            lblScanInfo.Text = "Files scanned: " + scannedFiles + " | Current: " + info.Name;
                            Application.DoEvents();
                        }
                    }
                    catch
                    {
                    }
                }

                string[] subFolders = Directory.GetDirectories(folderPath);

                foreach (string subFolder in subFolders)
                {
                    if (stopScan)
                        break;

                    if (IsProtectedPath(subFolder))
                        continue;

                    folderSize += ScanDirectory(subFolder, false);
                }
            }
            catch
            {
            }

            try
            {
                if (!isRoot && !IsProtectedPath(folderPath))
                {
                    FolderRecord folder = new FolderRecord();
                    folder.Name = Path.GetFileName(folderPath);
                    folder.FullPath = folderPath;
                    folder.SizeBytes = folderSize;
                    folders.Add(folder);
                }
            }
            catch
            {
            }

            return folderSize;
        }

        bool ShouldIncludeByScanMode(FileInfo info)
        {
            string mode = cbScanMode.Text;

            if (mode == "Full Scan")
                return true;

            if (mode == "Quick Scan")
                return true;

            if (mode == "Large Files Scan")
                return info.Length >= 100L * 1024L * 1024L;

            if (mode == "Old Files Scan")
                return info.LastWriteTime <= DateTime.Now.AddDays(-30);

            return true;
        }

        void AddToSummary(Dictionary<string, long> dict, string key, long value)
        {
            if (!dict.ContainsKey(key))
                dict[key] = 0;

            dict[key] += value;
        }

        void AddToCount(Dictionary<string, int> dict, string key)
        {
            if (!dict.ContainsKey(key))
                dict[key] = 0;

            dict[key]++;
        }

        void ClearDisplay()
        {
            lbLargestFiles.Items.Clear();
            lbLargestFolders.Items.Clear();
            lbTypeSummary.Items.Clear();
            lbExtensionSummary.Items.Clear();
            lbAgeSummary.Items.Clear();

            lblTotalSize.Text = "0 KB";
            lblFilesCount.Text = "0";
            lblFoldersCount.Text = "0";
            lblLargestFile.Text = "None";
            lblMostUsedType.Text = "None";
            lblSmartMessage.Text = "Scanning...";
        }

        void BuildResults()
        {
            List<FileRecord> filtered = GetFilteredFiles();
            DisplayLargestFiles(filtered);
            DisplayLargestFolders();
            DisplayTypeSummary();
            DisplayExtensionSummary();
            DisplayAgeSummary();
            UpdateDashboard();
            BuildSmartMessage();
        }

        List<FileRecord> GetFilteredFiles()
        {
            List<FileRecord> result = new List<FileRecord>();
            string search = txtSearch.Text.Trim().ToLower();
            string typeFilter = cbTypeFilter.Text;
            long minSize = GetSizeFilterBytes();

            foreach (FileRecord file in files)
            {
                bool show = true;

                if (search != "")
                {
                    if (!file.Name.ToLower().Contains(search) &&
                        !file.FullPath.ToLower().Contains(search) &&
                        !file.Extension.ToLower().Contains(search) &&
                        !file.Category.ToLower().Contains(search))
                    {
                        show = false;
                    }
                }

                if (typeFilter != "All" && file.Category != typeFilter)
                    show = false;

                if (file.SizeBytes < minSize)
                    show = false;

                if (show)
                    result.Add(file);
            }

            SortFiles(result);
            return result;
        }

        long GetSizeFilterBytes()
        {
            string filter = cbSizeFilter.Text;

            if (filter == "10 MB+")
                return 10L * 1024L * 1024L;

            if (filter == "100 MB+")
                return 100L * 1024L * 1024L;

            if (filter == "500 MB+")
                return 500L * 1024L * 1024L;

            if (filter == "1 GB+")
                return 1024L * 1024L * 1024L;

            return 0;
        }

        void SortFiles(List<FileRecord> list)
        {
            string sort = cbSort.Text;

            list.Sort((a, b) =>
            {
                if (sort == "Largest First")
                    return b.SizeBytes.CompareTo(a.SizeBytes);

                if (sort == "Smallest First")
                    return a.SizeBytes.CompareTo(b.SizeBytes);

                if (sort == "Newest First")
                    return b.LastModified.CompareTo(a.LastModified);

                if (sort == "Oldest First")
                    return a.LastModified.CompareTo(b.LastModified);

                return a.Name.CompareTo(b.Name);
            });
        }

        void DisplayLargestFiles(List<FileRecord> list)
        {
            lbLargestFiles.Items.Clear();

            int count = 0;

            foreach (FileRecord file in list)
            {
                lbLargestFiles.Items.Add(file);
                count++;

                if (count >= 20)
                    break;
            }

            lblStatus.Text = "Showing: " + lbLargestFiles.Items.Count;
        }

        void DisplayLargestFolders()
        {
            lbLargestFolders.Items.Clear();

            List<FolderRecord> sorted = new List<FolderRecord>();

            foreach (FolderRecord folder in folders)
                sorted.Add(folder);

            sorted.Sort((a, b) => b.SizeBytes.CompareTo(a.SizeBytes));

            int count = 0;

            foreach (FolderRecord folder in sorted)
            {
                lbLargestFolders.Items.Add(folder);
                count++;

                if (count >= 20)
                    break;
            }
        }

        void DisplayTypeSummary()
        {
            lbTypeSummary.Items.Clear();

            List<SummaryRecord> list = new List<SummaryRecord>();

            foreach (string key in typeTotals.Keys)
            {
                SummaryRecord item = new SummaryRecord();
                item.Name = key;
                item.SizeBytes = typeTotals[key];
                item.Count = typeCounts.ContainsKey(key) ? typeCounts[key] : 0;
                list.Add(item);
            }

            list.Sort((a, b) => b.SizeBytes.CompareTo(a.SizeBytes));

            foreach (SummaryRecord item in list)
                lbTypeSummary.Items.Add(item);
        }

        void DisplayExtensionSummary()
        {
            lbExtensionSummary.Items.Clear();

            List<SummaryRecord> list = new List<SummaryRecord>();

            foreach (string key in extensionTotals.Keys)
            {
                SummaryRecord item = new SummaryRecord();
                item.Name = key;
                item.SizeBytes = extensionTotals[key];
                item.Count = extensionCounts.ContainsKey(key) ? extensionCounts[key] : 0;
                list.Add(item);
            }

            list.Sort((a, b) => b.SizeBytes.CompareTo(a.SizeBytes));

            int count = 0;

            foreach (SummaryRecord item in list)
            {
                lbExtensionSummary.Items.Add(item);
                count++;

                if (count >= 15)
                    break;
            }
        }

        void DisplayAgeSummary()
        {
            lbAgeSummary.Items.Clear();

            int last7 = 0;
            int last30 = 0;
            int old30 = 0;
            int old180 = 0;

            long last7Size = 0;
            long last30Size = 0;
            long old30Size = 0;
            long old180Size = 0;

            foreach (FileRecord file in files)
            {
                double days = (DateTime.Now - file.LastModified).TotalDays;

                if (days <= 7)
                {
                    last7++;
                    last7Size += file.SizeBytes;
                }
                else if (days <= 30)
                {
                    last30++;
                    last30Size += file.SizeBytes;
                }
                else if (days <= 180)
                {
                    old30++;
                    old30Size += file.SizeBytes;
                }
                else
                {
                    old180++;
                    old180Size += file.SizeBytes;
                }
            }

            lbAgeSummary.Items.Add("Last 7 days: " + last7 + " files | " + FormatSize(last7Size));
            lbAgeSummary.Items.Add("Last 30 days: " + last30 + " files | " + FormatSize(last30Size));
            lbAgeSummary.Items.Add("30+ days: " + old30 + " files | " + FormatSize(old30Size));
            lbAgeSummary.Items.Add("180+ days: " + old180 + " files | " + FormatSize(old180Size));
        }

        void UpdateDashboard()
        {
            long totalSize = 0;

            foreach (FileRecord file in files)
                totalSize += file.SizeBytes;

            lblTotalSize.Text = FormatSize(totalSize);
            lblFilesCount.Text = files.Count.ToString();
            lblFoldersCount.Text = folders.Count.ToString();

            FileRecord largest = null;

            foreach (FileRecord file in files)
            {
                if (largest == null || file.SizeBytes > largest.SizeBytes)
                    largest = file;
            }

            if (largest != null)
            {
                lblLargestFile.Text = " " + ShortText(largest.Name, 10) + " (" + FormatSize(largest.SizeBytes) + ")";
                ToolTip t1 = new ToolTip();
                t1.SetToolTip(lblLargestFile, largest.Name + Environment.NewLine + largest.FullPath);
            }
            else
                lblLargestFile.Text = "None";

            string mostType = "None";
            long mostSize = 0;

            foreach (string key in typeTotals.Keys)
            {
                if (typeTotals[key] > mostSize)
                {
                    mostSize = typeTotals[key];
                    mostType = key;
                }
            }

            if (mostType != "None")
                lblMostUsedType.Text = " " + ShortText(mostType, 10) + " (" + FormatSize(mostSize) + ")";
            else
                lblMostUsedType.Text = "None";
        }

        void BuildSmartMessage()
        {
            if (files.Count == 0)
            {
                lblSmartMessage.Text = "No files found. Protected system locations were skipped for safety.";
                return;
            }

            string mostType = "";
            long mostSize = 0;

            foreach (string key in typeTotals.Keys)
            {
                if (typeTotals[key] > mostSize)
                {
                    mostSize = typeTotals[key];
                    mostType = key;
                }
            }

            int largeFiles = 0;
            int oldFiles = 0;
            int installers = 0;

            foreach (FileRecord file in files)
            {
                if (file.SizeBytes >= 500L * 1024L * 1024L)
                    largeFiles++;

                if (file.LastModified <= DateTime.Now.AddDays(-180))
                    oldFiles++;

                if (file.Extension == ".exe" || file.Extension == ".msi" || file.Extension == ".iso")
                    installers++;
            }

            if (mostType == "Videos")
            {
                lblSmartMessage.Text = "Most of your storage is used by videos. Review large video files first.";
                return;
            }

            if (installers > 0)
            {
                lblSmartMessage.Text = "Installer or ISO files were found. Old setup files may be worth reviewing.";
                return;
            }

            if (largeFiles > 0)
            {
                lblSmartMessage.Text = "Large files were found. Start by checking the Top 20 Largest Files list.";
                return;
            }

            if (oldFiles > 0)
            {
                lblSmartMessage.Text = "Many old files were found. Review older files if you need to free space.";
                return;
            }

            lblSmartMessage.Text = "Storage looks normal. System folders are blocked from scan and delete for safety.";
        }

        string ShortText(string text, int length)
        {
            if (text == null)
                return "";

            if (text.Length <= length)
                return text;

            return text.Substring(0, length) + "...";
        }

        string GetCategory(string extension)
        {
            string[] videos = { ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".3gp" };
            string[] images = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".heic", ".svg", ".ico" };
            string[] documents = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".rtf", ".csv" };
            string[] apps = { ".exe", ".msi", ".bat", ".cmd", ".ps1", ".lnk" };
            string[] archives = { ".zip", ".rar", ".7z", ".tar", ".gz", ".iso" };
            string[] audio = { ".mp3", ".wav", ".aac", ".flac", ".m4a", ".ogg" };
            string[] code = { ".cs", ".java", ".py", ".js", ".html", ".css", ".cpp", ".c", ".php", ".sql", ".json", ".xml" };

            if (ArrayContains(videos, extension))
                return "Videos";

            if (ArrayContains(images, extension))
                return "Images";

            if (ArrayContains(documents, extension))
                return "Documents";

            if (ArrayContains(apps, extension))
                return "Applications";

            if (ArrayContains(archives, extension))
                return "Archives";

            if (ArrayContains(audio, extension))
                return "Audio";

            if (ArrayContains(code, extension))
                return "Code Files";

            return "Other";
        }

        bool ArrayContains(string[] arr, string value)
        {
            foreach (string item in arr)
            {
                if (item == value)
                    return true;
            }

            return false;
        }

        string GetSafetyLabel(string fullPath, string extension)
        {
            if (IsProtectedPath(fullPath))
                return "Blocked";

            if (extension == ".mp4" || extension == ".mov" || extension == ".zip" || extension == ".rar" || extension == ".iso")
                return "Safe to Review";

            if (extension == ".exe" || extension == ".msi")
                return "Review Carefully";

            return "Normal";
        }

        string FormatSize(long bytes)
        {
            double size = bytes;

            if (size < 1024)
                return size.ToString("0") + " B";

            size = size / 1024;

            if (size < 1024)
                return size.ToString("0.0") + " KB";

            size = size / 1024;

            if (size < 1024)
                return size.ToString("0.0") + " MB";

            size = size / 1024;
            return size.ToString("0.0") + " GB";
        }



        private void BtnSelectAllFiles_Click(object sender, EventArgs e)
        {
            lbLargestFolders.ClearSelected();

            for (int i = 0; i < lbLargestFiles.Items.Count; i++)
                lbLargestFiles.SetSelected(i, true);

            lblStatus.Text = "Selected files: " + lbLargestFiles.SelectedItems.Count;
        }

        private void BtnSelectAllFolders_Click(object sender, EventArgs e)
        {
            lbLargestFiles.ClearSelected();

            for (int i = 0; i < lbLargestFolders.Items.Count; i++)
                lbLargestFolders.SetSelected(i, true);

            lblStatus.Text = "Selected folders: " + lbLargestFolders.SelectedItems.Count;
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BuildResults();
                e.SuppressKeyPress = true;
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            SaveSettings();
            BuildResults();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            BuildResults();
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cbTypeFilter.SelectedIndex = 0;
            cbSizeFilter.SelectedIndex = 0;
            cbSort.SelectedIndex = 0;
            BuildResults();
        }

        private void LbLargestFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbLargestFiles.SelectedItem != null)
                lbLargestFolders.ClearSelected();
        }

        private void LbLargestFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbLargestFolders.SelectedItem != null)
                lbLargestFiles.ClearSelected();
        }

        private void LbLargestFiles_DoubleClick(object sender, EventArgs e)
        {
            OpenSelectedLocation();
        }

        private void LbLargestFolders_DoubleClick(object sender, EventArgs e)
        {
            OpenSelectedLocation();
        }

        FileRecord GetSelectedFile()
        {
            if (lbLargestFiles.SelectedItem == null)
                return null;

            return lbLargestFiles.SelectedItem as FileRecord;
        }

        FolderRecord GetSelectedFolder()
        {
            if (lbLargestFolders.SelectedItem == null)
                return null;

            return lbLargestFolders.SelectedItem as FolderRecord;
        }

        List<FileRecord> GetSelectedFiles()
        {
            List<FileRecord> selected = new List<FileRecord>();

            foreach (object item in lbLargestFiles.SelectedItems)
            {
                FileRecord file = item as FileRecord;

                if (file != null)
                    selected.Add(file);
            }

            return selected;
        }

        List<FolderRecord> GetSelectedFolders()
        {
            List<FolderRecord> selected = new List<FolderRecord>();

            foreach (object item in lbLargestFolders.SelectedItems)
            {
                FolderRecord folder = item as FolderRecord;

                if (folder != null)
                    selected.Add(folder);
            }

            return selected;
        }

        private void BtnOpenFile_Click(object sender, EventArgs e)
        {
            FileRecord file = GetSelectedFile();
            FolderRecord folder = GetSelectedFolder();

            try
            {
                if (file != null)
                {
                    if (File.Exists(file.FullPath))
                    {
                        Process.Start(file.FullPath);
                        lblStatus.Text = "File opened";
                    }
                    else
                    {
                        MessageBox.Show("File was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    return;
                }

                if (folder != null)
                {
                    if (Directory.Exists(folder.FullPath))
                    {
                        Process.Start(folder.FullPath);
                        lblStatus.Text = "Folder opened";
                    }
                    else
                    {
                        MessageBox.Show("Folder was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    return;
                }

                MessageBox.Show("Please select a file or folder first.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Open error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenLocation_Click(object sender, EventArgs e)
        {
            OpenSelectedLocation();
        }

        void OpenSelectedLocation()
        {
            FileRecord file = GetSelectedFile();
            FolderRecord folder = GetSelectedFolder();

            try
            {
                if (file != null)
                {
                    if (File.Exists(file.FullPath))
                    {
                        Process.Start("explorer.exe", "/select,\"" + file.FullPath + "\"");
                        lblStatus.Text = "File location opened";
                    }
                    else
                    {
                        MessageBox.Show("File was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    return;
                }

                if (folder != null)
                {
                    if (Directory.Exists(folder.FullPath))
                    {
                        Process.Start(folder.FullPath);
                        lblStatus.Text = "Folder opened";
                    }
                    else
                    {
                        MessageBox.Show("Folder was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    return;
                }

                MessageBox.Show("Please select a file or folder first.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Open location error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCopyPath_Click(object sender, EventArgs e)
        {
            List<FileRecord> selectedFiles = GetSelectedFiles();
            List<FolderRecord> selectedFolders = GetSelectedFolders();
            List<string> paths = new List<string>();

            foreach (FileRecord file in selectedFiles)
                paths.Add(file.FullPath);

            foreach (FolderRecord folder in selectedFolders)
                paths.Add(folder.FullPath);

            if (paths.Count == 0)
            {
                MessageBox.Show("Please select one or more files or folders first.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Clipboard.SetText(string.Join(Environment.NewLine, paths.ToArray()));
            lblStatus.Text = "Copied " + paths.Count + " path(s)";
        }

        private void BtnMoveSelected_Click(object sender, EventArgs e)
        {
            FileRecord file = GetSelectedFile();
            FolderRecord folder = GetSelectedFolder();

            if (file == null && folder == null)
            {
                MessageBox.Show("Please select a file or folder first.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (file != null)
            {
                MoveSelectedFile(file);
                return;
            }

            if (folder != null)
            {
                MoveSelectedFolder(folder);
                return;
            }
        }

        void MoveSelectedFile(FileRecord file)
        {
            if (!File.Exists(file.FullPath))
            {
                MessageBox.Show("File was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (IsProtectedPath(file.FullPath))
            {
                MessageBox.Show("This file is protected and cannot be moved by this app.", "Move Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (IsProtectedPath(dialog.SelectedPath))
            {
                MessageBox.Show("You cannot move files into a protected system folder.", "Move Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string target = Path.Combine(dialog.SelectedPath, file.Name);
                target = GetUniqueFilePath(target);

                File.Move(file.FullPath, target);
                MessageBox.Show("File moved successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ScanFolder();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Move error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void MoveSelectedFolder(FolderRecord folder)
        {
            if (!Directory.Exists(folder.FullPath))
            {
                MessageBox.Show("Folder was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (IsProtectedPath(folder.FullPath))
            {
                MessageBox.Show("This folder is protected and cannot be moved by this app.", "Move Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (IsProtectedPath(dialog.SelectedPath))
            {
                MessageBox.Show("You cannot move folders into a protected system folder.", "Move Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string target = Path.Combine(dialog.SelectedPath, folder.Name);
                target = GetUniqueFolderPath(target);

                Directory.Move(folder.FullPath, target);
                MessageBox.Show("Folder moved successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ScanFolder();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Move folder error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        string GetUniqueFilePath(string path)
        {
            if (!File.Exists(path))
                return path;

            string folder = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);

            int counter = 1;
            string newPath = "";

            do
            {
                newPath = Path.Combine(folder, name + " (" + counter + ")" + ext);
                counter++;
            }
            while (File.Exists(newPath));

            return newPath;
        }

        string GetUniqueFolderPath(string path)
        {
            if (!Directory.Exists(path))
                return path;

            string parent = Path.GetDirectoryName(path);
            string name = Path.GetFileName(path);

            int counter = 1;
            string newPath = "";

            do
            {
                newPath = Path.Combine(parent, name + " (" + counter + ")");
                counter++;
            }
            while (Directory.Exists(newPath));

            return newPath;
        }

        private void BtnDeleteSelected_Click(object sender, EventArgs e)
        {
            List<FileRecord> selectedFiles = GetSelectedFiles();
            List<FolderRecord> selectedFolders = GetSelectedFolders();

            if (selectedFiles.Count == 0 && selectedFolders.Count == 0)
            {
                MessageBox.Show("Please select one or more files or folders first.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DeleteSelectedItems(selectedFiles, selectedFolders);
        }

        void DeleteSelectedItems(List<FileRecord> selectedFiles, List<FolderRecord> selectedFolders)
        {
            int total = selectedFiles.Count + selectedFolders.Count;

            DialogResult first = MessageBox.Show(
                "Warning: You selected " + total + " item(s) for deletion." + Environment.NewLine + Environment.NewLine +
                "This app blocks protected system paths, but you should still review your selection carefully." + Environment.NewLine + Environment.NewLine +
                "Do you want to continue?",
                "Delete Confirmation 1 of 2",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (first != DialogResult.Yes)
                return;

            DialogResult second = MessageBox.Show(
                "Final confirmation: delete the selected item(s) now?" + Environment.NewLine + Environment.NewLine +
                "Files/Folders that are protected or missing will be skipped.",
                "Delete Confirmation 2 of 2",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (second != DialogResult.Yes)
                return;

            int deleted = 0;
            int skipped = 0;
            int failed = 0;

            foreach (FileRecord file in selectedFiles)
            {
                if (!File.Exists(file.FullPath))
                {
                    skipped++;
                    continue;
                }

                if (!IsSafeToDelete(file.FullPath))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    File.Delete(file.FullPath);
                    deleted++;
                }
                catch
                {
                    failed++;
                }
            }

            foreach (FolderRecord folder in selectedFolders)
            {
                if (!Directory.Exists(folder.FullPath))
                {
                    skipped++;
                    continue;
                }

                if (!IsSafeToDelete(folder.FullPath))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    Directory.Delete(folder.FullPath, true);
                    deleted++;
                }
                catch
                {
                    failed++;
                }
            }

            MessageBox.Show(
                "Delete finished." + Environment.NewLine +
                "Deleted: " + deleted + Environment.NewLine +
                "Skipped: " + skipped + Environment.NewLine +
                "Failed: " + failed,
                "Delete Result",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            ScanFolder();
        }

        void DeleteSelectedFile(FileRecord file)
        {
            if (!File.Exists(file.FullPath))
            {
                MessageBox.Show("File was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsSafeToDelete(file.FullPath))
            {
                MessageBox.Show(
                    "This file is protected and cannot be deleted by this app.",
                    "Delete Blocked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            DialogResult first = MessageBox.Show(
                "Warning: This will delete the selected file." + Environment.NewLine + Environment.NewLine +
                file.Name + Environment.NewLine +
                file.FullPath + Environment.NewLine + Environment.NewLine +
                "Do you want to continue?",
                "Delete File Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (first != DialogResult.Yes)
                return;

            DialogResult second = MessageBox.Show(
                "Final confirmation: delete this file now?",
                "Delete Confirmation 2 of 2",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (second != DialogResult.Yes)
                return;

            try
            {
                File.Delete(file.FullPath);
                MessageBox.Show("File deleted successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ScanFolder();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "Windows blocked this delete action because you do not have permission.",
                    "Permission Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Delete error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void DeleteSelectedFolder(FolderRecord folder)
        {
            if (!Directory.Exists(folder.FullPath))
            {
                MessageBox.Show("Folder was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsSafeToDelete(folder.FullPath))
            {
                MessageBox.Show(
                    "This folder is protected and cannot be deleted by this app.",
                    "Delete Blocked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            DialogResult first = MessageBox.Show(
                "Warning: This will delete the selected folder and everything inside it." + Environment.NewLine + Environment.NewLine +
                folder.Name + Environment.NewLine +
                folder.FullPath + Environment.NewLine + Environment.NewLine +
                "This action can be dangerous. Do you want to continue?",
                "Delete Folder Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (first != DialogResult.Yes)
                return;

            DialogResult second = MessageBox.Show(
                "Final confirmation: delete this folder and everything inside it now?",
                "Delete Confirmation 2 of 2",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (second != DialogResult.Yes)
                return;

            try
            {
                Directory.Delete(folder.FullPath, true);
                MessageBox.Show("Folder deleted successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ScanFolder();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "Windows blocked this delete action because you do not have permission.",
                    "Permission Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Delete folder error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        string AskTextConfirmation(string titleText, string message)
        {
            Form f = new Form();
            f.Text = titleText;
            f.Size = new Size(480, 230);
            f.StartPosition = FormStartPosition.CenterParent;
            f.BackColor = bg;
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;

            Label lbl = new Label();
            lbl.Text = message;
            lbl.Location = new Point(25, 25);
            lbl.Size = new Size(410, 55);
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            f.Controls.Add(lbl);

            TextBox tb = new TextBox();
            tb.Location = new Point(25, 90);
            tb.Size = new Size(410, 25);
            f.Controls.Add(tb);

            Button ok = CreateButton("Confirm", 90, 140, 130, 35, red);
            Button cancel = CreateButton("Cancel", 240, 140, 120, 35, dark);

            string value = "";

            ok.Click += (s, e) =>
            {
                value = tb.Text;
                f.DialogResult = DialogResult.OK;
                f.Close();
            };

            cancel.Click += (s, e) =>
            {
                f.DialogResult = DialogResult.Cancel;
                f.Close();
            };

            f.Controls.Add(ok);
            f.Controls.Add(cancel);

            DialogResult result = f.ShowDialog();

            if (result == DialogResult.OK)
                return value;

            return "";
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (files.Count == 0)
            {
                MessageBox.Show("Please scan a folder first.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export Disk Space Report";
            dialog.Filter = "Text File|*.txt";
            dialog.FileName = "DiskSpaceAnalyzer_Report.txt";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                File.WriteAllText(dialog.FileName, BuildReport());
                MessageBox.Show("Report exported successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        string BuildReport()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Disk Space Analyzer Report");
            sb.AppendLine("==========================");
            sb.AppendLine();
            sb.AppendLine("Developer: Mohammed Ahmed");
            sb.AppendLine("Scan Date: " + lastScanDate.ToString("yyyy/MM/dd HH:mm"));
            sb.AppendLine("Selected Folder: " + txtFolder.Text);
            sb.AppendLine("Scan Mode: " + cbScanMode.Text);
            sb.AppendLine("Safety Mode: Protected system folders are blocked from scan and delete.");
            sb.AppendLine();

            sb.AppendLine("Summary");
            sb.AppendLine("-------");
            sb.AppendLine("Total Size: " + lblTotalSize.Text);
            sb.AppendLine("Files Count: " + lblFilesCount.Text);
            sb.AppendLine("Folders Count: " + lblFoldersCount.Text);
            sb.AppendLine("Largest File: " + lblLargestFile.Text);
            sb.AppendLine("Most Used Type: " + lblMostUsedType.Text);
            sb.AppendLine("Smart Message: " + lblSmartMessage.Text);
            sb.AppendLine();

            sb.AppendLine("Top 20 Largest Files");
            sb.AppendLine("--------------------");

            int fileCount = 0;
            List<FileRecord> sortedFiles = new List<FileRecord>();

            foreach (FileRecord file in files)
                sortedFiles.Add(file);

            sortedFiles.Sort((a, b) => b.SizeBytes.CompareTo(a.SizeBytes));

            foreach (FileRecord file in sortedFiles)
            {
                sb.AppendLine((fileCount + 1) + ". " + file.Name + " | " + FormatSize(file.SizeBytes) + " | " + file.Category + " | " + file.SafetyLabel);
                sb.AppendLine("   Path: " + file.FullPath);
                fileCount++;

                if (fileCount >= 20)
                    break;
            }

            sb.AppendLine();
            sb.AppendLine("Largest Folders");
            sb.AppendLine("---------------");

            int folderCount = 0;
            List<FolderRecord> sortedFolders = new List<FolderRecord>();

            foreach (FolderRecord folder in folders)
                sortedFolders.Add(folder);

            sortedFolders.Sort((a, b) => b.SizeBytes.CompareTo(a.SizeBytes));

            foreach (FolderRecord folder in sortedFolders)
            {
                sb.AppendLine((folderCount + 1) + ". " + folder.Name + " | " + FormatSize(folder.SizeBytes));
                sb.AppendLine("   Path: " + folder.FullPath);
                folderCount++;

                if (folderCount >= 20)
                    break;
            }

            sb.AppendLine();
            sb.AppendLine("File Type Summary");
            sb.AppendLine("-----------------");

            foreach (string key in typeTotals.Keys)
                sb.AppendLine(key + ": " + FormatSize(typeTotals[key]) + " | Files: " + typeCounts[key]);

            sb.AppendLine();
            sb.AppendLine("Extension Summary");
            sb.AppendLine("-----------------");

            foreach (string key in extensionTotals.Keys)
                sb.AppendLine(key + ": " + FormatSize(extensionTotals[key]) + " | Files: " + extensionCounts[key]);

            sb.AppendLine();
            sb.AppendLine("Developed by Mohammed Ahmed");

            return sb.ToString();
        }

        private void BtnHowToUse_Click(object sender, EventArgs e)
        {
            string msg = "";

            msg += "Disk Space Analyzer - How To Use" + Environment.NewLine;
            msg += "--------------------------------" + Environment.NewLine + Environment.NewLine;

            msg += "Developer:" + Environment.NewLine;
            msg += "- Developed by Mohammed Ahmed." + Environment.NewLine + Environment.NewLine;

            msg += "Safety Mode:" + Environment.NewLine;
            msg += "- The app blocks Windows, Program Files, AppData, System32, and system folders." + Environment.NewLine;
            msg += "- Protected system folders are not scanned." + Environment.NewLine;
            msg += "- Protected system files and folders cannot be deleted or moved by this app." + Environment.NewLine + Environment.NewLine;

            msg += "1. Choose Folder:" + Environment.NewLine;
            msg += "- Click Browse or use quick buttons like Downloads, Desktop, or Documents." + Environment.NewLine + Environment.NewLine;

            msg += "2. Scan Modes:" + Environment.NewLine;
            msg += "- Full Scan: scans all allowed files." + Environment.NewLine;
            msg += "- Quick Scan: normal scan mode." + Environment.NewLine;
            msg += "- Large Files Scan: shows files 100MB or bigger." + Environment.NewLine;
            msg += "- Old Files Scan: shows files older than 30 days." + Environment.NewLine + Environment.NewLine;

            msg += "3. Results:" + Environment.NewLine;
            msg += "- Shows total size, files count, folders count, largest files, largest folders, file type summary, extension summary, and file age summary." + Environment.NewLine + Environment.NewLine;

            msg += "4. Open Location:" + Environment.NewLine;
            msg += "- Select a file then click Open Location to show it in File Explorer." + Environment.NewLine;
            msg += "- Select a folder then click Open Location to open that folder." + Environment.NewLine + Environment.NewLine;

            msg += "5. Safe Actions:" + Environment.NewLine;
            msg += "- The app does not delete or move anything automatically." + Environment.NewLine;
            msg += "- Delete requires two confirmations." + Environment.NewLine;
            msg += "- Move asks you to choose the destination folder." + Environment.NewLine + Environment.NewLine;

            msg += "6. Export Report:" + Environment.NewLine;
            msg += "- Export Report creates a text report with scan results." + Environment.NewLine + Environment.NewLine;

            msg += "Data Storage:" + Environment.NewLine;
            msg += "- Only settings are saved in AppData." + Environment.NewLine;
            msg += "- No Access, SQLite, SQL Server, or external database is required." + Environment.NewLine + Environment.NewLine;

            msg += "Settings path:" + Environment.NewLine;
            msg += settingsFile;

            MessageBox.Show(msg, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            DialogResult first = MessageBox.Show(
                "Reset Data will clear saved settings only." + Environment.NewLine +
                "It will not delete any files." + Environment.NewLine + Environment.NewLine +
                "Do you want to continue?",
                "Reset Data",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (first != DialogResult.Yes)
                return;

            string typed = AskTextConfirmation("Final Reset Confirmation", "Type RESET exactly to clear saved settings.");

            if (typed != "RESET")
            {
                MessageBox.Show("Reset cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                File.WriteAllText(settingsFile, "");
                txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                cbScanMode.SelectedIndex = 0;
                cbTypeFilter.SelectedIndex = 0;
                cbSizeFilter.SelectedIndex = 0;
                cbSort.SelectedIndex = 0;
                txtSearch.Clear();

                files.Clear();
                folders.Clear();
                typeTotals.Clear();
                typeCounts.Clear();
                extensionTotals.Clear();
                extensionCounts.Clear();

                ClearDisplay();
                lblStatus.Text = "Reset complete";
                lblSmartMessage.Text = "Settings reset. Choose a folder and scan again.";

                MessageBox.Show("Settings reset successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reset error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class FileRecord
        {
            public string Name;
            public string FullPath;
            public string Extension;
            public string Category;
            public string SafetyLabel;
            public long SizeBytes;
            public DateTime LastModified;

            public override string ToString()
            {
                return Name + " | " + FormatSizeStatic(SizeBytes) + " | " + Category + " | " + SafetyLabel + " | " + LastModified.ToString("yyyy/MM/dd");
            }

            static string FormatSizeStatic(long bytes)
            {
                double size = bytes;

                if (size < 1024)
                    return size.ToString("0") + " B";

                size = size / 1024;

                if (size < 1024)
                    return size.ToString("0.0") + " KB";

                size = size / 1024;

                if (size < 1024)
                    return size.ToString("0.0") + " MB";

                size = size / 1024;
                return size.ToString("0.0") + " GB";
            }
        }

        public class FolderRecord
        {
            public string Name;
            public string FullPath;
            public long SizeBytes;

            public override string ToString()
            {
                return Name + " | " + FormatSizeStatic(SizeBytes) + " | " + FullPath;
            }

            static string FormatSizeStatic(long bytes)
            {
                double size = bytes;

                if (size < 1024)
                    return size.ToString("0") + " B";

                size = size / 1024;

                if (size < 1024)
                    return size.ToString("0.0") + " KB";

                size = size / 1024;

                if (size < 1024)
                    return size.ToString("0.0") + " MB";

                size = size / 1024;
                return size.ToString("0.0") + " GB";
            }
        }

        public class SummaryRecord
        {
            public string Name;
            public long SizeBytes;
            public int Count;

            public override string ToString()
            {
                return Name + " | " + FormatSizeStatic(SizeBytes) + " | Files: " + Count;
            }

            static string FormatSizeStatic(long bytes)
            {
                double size = bytes;

                if (size < 1024)
                    return size.ToString("0") + " B";

                size = size / 1024;

                if (size < 1024)
                    return size.ToString("0.0") + " KB";

                size = size / 1024;

                if (size < 1024)
                    return size.ToString("0.0") + " MB";

                size = size / 1024;
                return size.ToString("0.0") + " GB";
            }
        }
    }
}
