using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DownloadCleanerApp
{
    public class DownloadCleanerForm : Form
    {
        TextBox txtFolder;
        TextBox txtSearch;
        ListBox lbFiles;
        ComboBox cbScanType;

        Button btnBrowse;
        Button btnScan;
        Button btnSelectAll;
        Button btnClearSelection;
        Button btnMoveToCleaned;
        Button btnDeleteSelected;
        Button btnOrganizeByType;
        Button btnSearch;
        Button btnShowAll;
        Button btnHowToUse;
        Button btnReset;

        Label lblStatus;
        Label lblTotalFiles;
        Label lblTotalSize;
        Label lblSelected;

        List<FileItem> allFiles = new List<FileItem>();
        List<FileItem> currentFiles = new List<FileItem>();

        static string userName = Environment.UserName;
        static string dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            userName + "_DownloadCleaner_Data"
        );

        static string settingsFile = Path.Combine(dataFolder, "settings.txt");

        Color bg = Color.FromArgb(245, 247, 250);
        Color dark = Color.FromArgb(31, 41, 55);
        Color blue = Color.FromArgb(35, 97, 146);
        Color green = Color.FromArgb(39, 174, 96);
        Color red = Color.FromArgb(192, 57, 43);
        Color orange = Color.FromArgb(243, 156, 18);
        Color gray = Color.FromArgb(127, 140, 141);
        Color card = Color.White;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DownloadCleanerForm());
        }

        public DownloadCleanerForm()
        {
            PrepareDataFolder();
            BuildUI();
            LoadSettings();
        }

        void BuildUI()
        {
            this.Text = "Download Cleaner";
            this.Size = new Size(1080, 740);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bg;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            Label title = new Label();
            title.Text = "Download Cleaner";
            title.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            title.ForeColor = dark;
            title.Location = new Point(30, 18);
            title.Size = new Size(430, 45);
            this.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Scan, preview, move, delete, and organize files safely.";
            subtitle.Font = new Font("Segoe UI", 10, FontStyle.Italic);
            subtitle.ForeColor = Color.FromArgb(90, 90, 90);
            subtitle.Location = new Point(35, 60);
            subtitle.Size = new Size(650, 25);
            this.Controls.Add(subtitle);

            btnReset = CreateButton("Reset Data", 920, 25, 110, 30, red);
            btnReset.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            Panel topCard = CreateCard(30, 100, 1000, 130);
            this.Controls.Add(topCard);

            topCard.Controls.Add(CreateLabel("Folder:", 20, 25, 80, 25, FontStyle.Bold));

            txtFolder = new TextBox();
            txtFolder.Location = new Point(100, 25);
            txtFolder.Size = new Size(640, 25);
            txtFolder.ReadOnly = true;
            topCard.Controls.Add(txtFolder);

            btnBrowse = CreateButton("Browse", 760, 21, 100, 32, blue);
            btnBrowse.Click += BtnBrowse_Click;
            topCard.Controls.Add(btnBrowse);

            btnScan = CreateButton("Scan", 870, 21, 100, 32, green);
            btnScan.Click += BtnScan_Click;
            topCard.Controls.Add(btnScan);

            topCard.Controls.Add(CreateLabel("Scan Type:", 20, 75, 100, 25, FontStyle.Bold));

            cbScanType = new ComboBox();
            cbScanType.Location = new Point(120, 75);
            cbScanType.Size = new Size(210, 25);
            cbScanType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbScanType.Items.Add("All Files");
            cbScanType.Items.Add("Old Files - 30+ Days");
            cbScanType.Items.Add("Large Files - 100MB+");
            cbScanType.Items.Add("Temporary Files");
            cbScanType.Items.Add("Images");
            cbScanType.Items.Add("Videos");
            cbScanType.Items.Add("Documents");
            cbScanType.Items.Add("Compressed Files");
            cbScanType.SelectedIndex = 0;
            topCard.Controls.Add(cbScanType);

            topCard.Controls.Add(CreateLabel("Search:", 360, 75, 70, 25, FontStyle.Bold));

            txtSearch = new TextBox();
            txtSearch.Location = new Point(430, 75);
            txtSearch.Size = new Size(210, 25);
            topCard.Controls.Add(txtSearch);

            btnSearch = CreateButton("Search", 650, 71, 90, 32, blue);
            btnSearch.Click += BtnSearch_Click;
            topCard.Controls.Add(btnSearch);

            btnShowAll = CreateButton("Show All", 750, 71, 100, 32, dark);
            btnShowAll.Click += BtnShowAll_Click;
            topCard.Controls.Add(btnShowAll);

            btnHowToUse = CreateButton("How To Use", 860, 71, 110, 32, gray);
            btnHowToUse.Click += BtnHowToUse_Click;
            topCard.Controls.Add(btnHowToUse);

            Panel statsCard = CreateCard(30, 250, 1000, 80);
            this.Controls.Add(statsCard);

            lblTotalFiles = CreateStatLabel("Files: 0", 25, 20, 240, 35);
            statsCard.Controls.Add(lblTotalFiles);

            lblTotalSize = CreateStatLabel("Size: 0 KB", 290, 20, 300, 35);
            statsCard.Controls.Add(lblTotalSize);

            lblSelected = CreateStatLabel("Selected: 0", 620, 20, 250, 35);
            statsCard.Controls.Add(lblSelected);

            Panel listCard = CreateCard(30, 350, 1000, 250);
            this.Controls.Add(listCard);

            Label listHeader = CreateLabel("Preview Files", 20, 15, 200, 25, FontStyle.Bold);
            listHeader.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            listCard.Controls.Add(listHeader);

            lbFiles = new ListBox();
            lbFiles.Location = new Point(20, 50);
            lbFiles.Size = new Size(960, 175);
            lbFiles.SelectionMode = SelectionMode.MultiExtended;
            lbFiles.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lbFiles.SelectedIndexChanged += LbFiles_SelectedIndexChanged;
            listCard.Controls.Add(lbFiles);

            Panel actionCard = CreateCard(30, 620, 1000, 65);
            this.Controls.Add(actionCard);

            btnSelectAll = CreateButton("Select All", 20, 15, 110, 35, blue);
            btnSelectAll.Click += BtnSelectAll_Click;
            actionCard.Controls.Add(btnSelectAll);

            btnClearSelection = CreateButton("Clear Selection", 140, 15, 140, 35, dark);
            btnClearSelection.Click += BtnClearSelection_Click;
            actionCard.Controls.Add(btnClearSelection);

            btnMoveToCleaned = CreateButton("Move to Cleaned Folder", 300, 15, 200, 35, green);
            btnMoveToCleaned.Click += BtnMoveToCleaned_Click;
            actionCard.Controls.Add(btnMoveToCleaned);

            btnOrganizeByType = CreateButton("Organize by Type", 515, 15, 165, 35, orange);
            btnOrganizeByType.Click += BtnOrganizeByType_Click;
            actionCard.Controls.Add(btnOrganizeByType);

            btnDeleteSelected = CreateButton("Delete Selected", 700, 15, 150, 35, red);
            btnDeleteSelected.Click += BtnDeleteSelected_Click;
            actionCard.Controls.Add(btnDeleteSelected);

            lblStatus = new Label();
            lblStatus.Text = "Ready";
            lblStatus.Location = new Point(865, 20);
            lblStatus.Size = new Size(120, 25);
            lblStatus.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
            actionCard.Controls.Add(lblStatus);

            Label developedBy = new Label();
            developedBy.Text = "Developed by Mohammed Ahmed Alhijab";
            developedBy.Location = new Point(0, 690);
            developedBy.Size = new Size(1080, 25);
            developedBy.TextAlign = ContentAlignment.MiddleCenter;
            developedBy.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            developedBy.ForeColor = Color.FromArgb(80, 80, 80);
            this.Controls.Add(developedBy);
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

        Label CreateLabel(string text, int x, int y, int w, int h, FontStyle style)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(w, h);
            lbl.Font = new Font("Segoe UI", 10, style);
            lbl.ForeColor = dark;
            return lbl;
        }

        Label CreateStatLabel(string text, int x, int y, int w, int h)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(w, h);
            lbl.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            lbl.ForeColor = blue;
            return lbl;
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
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            return btn;
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
                if (File.Exists(settingsFile))
                {
                    string path = File.ReadAllText(settingsFile);

                    if (Directory.Exists(path))
                        txtFolder.Text = path;
                    else
                        txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                }
                else
                {
                    txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                }
            }
            catch
            {
                txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            }
        }

        void SaveSettings()
        {
            File.WriteAllText(settingsFile, txtFolder.Text);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (Directory.Exists(txtFolder.Text))
                dialog.SelectedPath = txtFolder.Text;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = dialog.SelectedPath;
                SaveSettings();
            }
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            ScanFiles();
        }

        void ScanFiles()
        {
            string folder = txtFolder.Text;

            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show("Please choose a valid folder first.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            allFiles.Clear();
            currentFiles.Clear();
            lbFiles.Items.Clear();

            try
            {
                string[] files = Directory.GetFiles(folder);

                foreach (string path in files)
                {
                    FileInfo info = new FileInfo(path);

                    FileItem item = new FileItem();
                    item.FullPath = path;
                    item.Name = info.Name;
                    item.Extension = info.Extension.ToLower();
                    item.SizeBytes = info.Length;
                    item.LastModified = info.LastWriteTime;
                    item.Category = GetCategory(item.Extension);

                    if (MatchesScanType(item))
                        allFiles.Add(item);
                }

                currentFiles.AddRange(allFiles);
                RefreshList(currentFiles);
                UpdateStats(currentFiles);

                lblStatus.Text = "Scanned";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Scan error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        bool MatchesScanType(FileItem item)
        {
            string type = cbScanType.Text;

            if (type == "All Files")
                return true;

            if (type == "Old Files - 30+ Days")
                return item.LastModified <= DateTime.Now.AddDays(-30);

            if (type == "Large Files - 100MB+")
                return item.SizeBytes >= 100 * 1024 * 1024;

            if (type == "Temporary Files")
                return IsTemporary(item.Extension);

            if (type == "Images")
                return item.Category == "Images";

            if (type == "Videos")
                return item.Category == "Videos";

            if (type == "Documents")
                return item.Category == "Documents";

            if (type == "Compressed Files")
                return item.Category == "Compressed Files";

            return true;
        }

        string GetCategory(string extension)
        {
            string[] images = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".heic", ".svg", ".ico" };
            string[] videos = { ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".3gp" };
            string[] documents = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".rtf", ".csv" };
            string[] compressed = { ".zip", ".rar", ".7z", ".tar", ".gz", ".iso" };
            string[] apps = { ".exe", ".msi", ".bat", ".cmd", ".ps1" };

            if (ArrayHasValue(images, extension))
                return "Images";

            if (ArrayHasValue(videos, extension))
                return "Videos";

            if (ArrayHasValue(documents, extension))
                return "Documents";

            if (ArrayHasValue(compressed, extension))
                return "Compressed Files";

            if (ArrayHasValue(apps, extension))
                return "Applications";

            if (IsTemporary(extension))
                return "Temporary Files";

            return "Other Files";
        }

        bool IsTemporary(string extension)
        {
            string[] temp = { ".tmp", ".temp", ".bak", ".old", ".log", ".cache", ".crdownload", ".part" };
            return ArrayHasValue(temp, extension);
        }

        bool ArrayHasValue(string[] arr, string value)
        {
            foreach (string item in arr)
            {
                if (item == value)
                    return true;
            }

            return false;
        }

        void RefreshList(List<FileItem> files)
        {
            lbFiles.Items.Clear();

            foreach (FileItem item in files)
                lbFiles.Items.Add(item);

            UpdateSelectedLabel();
        }

        void UpdateStats(List<FileItem> files)
        {
            long total = 0;

            foreach (FileItem item in files)
                total += item.SizeBytes;

            lblTotalFiles.Text = "Files: " + files.Count;
            lblTotalSize.Text = "Size: " + FormatSize(total);
            UpdateSelectedLabel();
        }

        void UpdateSelectedLabel()
        {
            lblSelected.Text = "Selected: " + lbFiles.SelectedItems.Count;
        }

        private void LbFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedLabel();
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

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim().ToLower();

            if (search == "")
            {
                RefreshList(currentFiles);
                UpdateStats(currentFiles);
                return;
            }

            List<FileItem> results = new List<FileItem>();

            foreach (FileItem item in currentFiles)
            {
                if (item.Name.ToLower().Contains(search) ||
                    item.Extension.ToLower().Contains(search) ||
                    item.Category.ToLower().Contains(search))
                {
                    results.Add(item);
                }
            }

            RefreshList(results);
            UpdateStats(results);
            lblStatus.Text = "Search";
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            RefreshList(currentFiles);
            UpdateStats(currentFiles);
            lblStatus.Text = "All";
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbFiles.Items.Count; i++)
                lbFiles.SetSelected(i, true);

            UpdateSelectedLabel();
        }

        private void BtnClearSelection_Click(object sender, EventArgs e)
        {
            lbFiles.ClearSelected();
            UpdateSelectedLabel();
        }

        List<FileItem> GetSelectedFiles()
        {
            List<FileItem> selected = new List<FileItem>();

            foreach (object obj in lbFiles.SelectedItems)
            {
                FileItem item = obj as FileItem;

                if (item != null)
                    selected.Add(item);
            }

            return selected;
        }

        private void BtnMoveToCleaned_Click(object sender, EventArgs e)
        {
            List<FileItem> selected = GetSelectedFiles();

            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one file.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Move selected files to a folder named Cleaned_Files inside the selected folder?",
                "Confirm Move",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            string targetFolder = Path.Combine(txtFolder.Text, "Cleaned_Files");

            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            int moved = 0;

            foreach (FileItem item in selected)
            {
                try
                {
                    if (File.Exists(item.FullPath))
                    {
                        string targetPath = Path.Combine(targetFolder, item.Name);
                        targetPath = GetUniquePath(targetPath);
                        File.Move(item.FullPath, targetPath);
                        moved++;
                    }
                }
                catch
                {
                }
            }

            MessageBox.Show("Moved files: " + moved, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ScanFiles();
        }

        private void BtnDeleteSelected_Click(object sender, EventArgs e)
        {
            List<FileItem> selected = GetSelectedFiles();

            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one file.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult firstConfirm = MessageBox.Show(
                "Warning: This will delete selected files." + Environment.NewLine +
                "Files selected: " + selected.Count + Environment.NewLine + Environment.NewLine +
                "Do you want to continue?",
                "Delete Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (firstConfirm != DialogResult.Yes)
                return;

            string typedText = AskDeleteConfirmation();

            if (typedText != "DELETE")
            {
                MessageBox.Show("Delete cancelled. You must type DELETE exactly.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int deleted = 0;

            foreach (FileItem item in selected)
            {
                try
                {
                    if (File.Exists(item.FullPath))
                    {
                        File.Delete(item.FullPath);
                        deleted++;
                    }
                }
                catch
                {
                }
            }

            MessageBox.Show("Deleted files: " + deleted, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ScanFiles();
        }

        string AskDeleteConfirmation()
        {
            Form f = new Form();
            f.Text = "Final Delete Confirmation";
            f.Size = new Size(470, 230);
            f.StartPosition = FormStartPosition.CenterParent;
            f.BackColor = bg;
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;

            Label lbl = new Label();
            lbl.Text = "Final confirmation:" + Environment.NewLine + "Type DELETE exactly to delete selected files.";
            lbl.Location = new Point(25, 25);
            lbl.Size = new Size(400, 55);
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            f.Controls.Add(lbl);

            TextBox tb = new TextBox();
            tb.Location = new Point(25, 90);
            tb.Size = new Size(400, 25);
            f.Controls.Add(tb);

            Button ok = CreateButton("Confirm Delete", 75, 140, 140, 35, red);
            Button cancel = CreateButton("Cancel", 230, 140, 120, 35, dark);

            string value = "";

            ok.Click += (s, ev) =>
            {
                value = tb.Text;
                f.DialogResult = DialogResult.OK;
                f.Close();
            };

            cancel.Click += (s, ev) =>
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

        private void BtnOrganizeByType_Click(object sender, EventArgs e)
        {
            List<FileItem> selected = GetSelectedFiles();

            if (selected.Count == 0)
            {
                MessageBox.Show("Please select files to organize.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Organize selected files into folders by file type?",
                "Confirm Organize",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            int moved = 0;

            foreach (FileItem item in selected)
            {
                try
                {
                    if (File.Exists(item.FullPath))
                    {
                        string folder = Path.Combine(txtFolder.Text, item.Category);

                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        string targetPath = Path.Combine(folder, item.Name);
                        targetPath = GetUniquePath(targetPath);

                        File.Move(item.FullPath, targetPath);
                        moved++;
                    }
                }
                catch
                {
                }
            }

            MessageBox.Show("Organized files: " + moved, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ScanFiles();
        }

        string GetUniquePath(string path)
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

        private void BtnHowToUse_Click(object sender, EventArgs e)
        {
            string msg = "";

            msg += "Download Cleaner - How To Use" + Environment.NewLine;
            msg += "-----------------------------" + Environment.NewLine + Environment.NewLine;

            msg += "1. Choose Folder:" + Environment.NewLine;
            msg += "- Click Browse and choose Downloads or any folder." + Environment.NewLine + Environment.NewLine;

            msg += "2. Scan Type:" + Environment.NewLine;
            msg += "- All Files: shows all files in the selected folder." + Environment.NewLine;
            msg += "- Old Files: shows files older than 30 days." + Environment.NewLine;
            msg += "- Large Files: shows files 100MB or bigger." + Environment.NewLine;
            msg += "- Temporary Files: shows temp, log, cache, part, and similar files." + Environment.NewLine + Environment.NewLine;

            msg += "3. Safe Preview:" + Environment.NewLine;
            msg += "- The app does not delete anything after scan." + Environment.NewLine;
            msg += "- You must select files first." + Environment.NewLine + Environment.NewLine;

            msg += "4. Move to Cleaned Folder:" + Environment.NewLine;
            msg += "- Moves selected files into Cleaned_Files inside the chosen folder." + Environment.NewLine + Environment.NewLine;

            msg += "5. Organize by Type:" + Environment.NewLine;
            msg += "- Moves selected files into folders like Images, Videos, Documents." + Environment.NewLine + Environment.NewLine;

            msg += "6. Delete Selected:" + Environment.NewLine;
            msg += "- Requires two confirmations." + Environment.NewLine;
            msg += "- Final confirmation requires typing DELETE." + Environment.NewLine + Environment.NewLine;

            msg += "Data Storage:" + Environment.NewLine;
            msg += "- Only settings are saved in AppData." + Environment.NewLine;
            msg += "- No Access database is required." + Environment.NewLine;
            msg += "- No SQLite files are required." + Environment.NewLine + Environment.NewLine;

            msg += "Settings path:" + Environment.NewLine;
            msg += settingsFile;

            MessageBox.Show(msg, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            DialogResult firstConfirm = MessageBox.Show(
                "Reset Data will clear saved settings only." + Environment.NewLine +
                "It will not delete your files." + Environment.NewLine + Environment.NewLine +
                "Do you want to continue?",
                "Reset Data",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (firstConfirm != DialogResult.Yes)
                return;

            string typedText = AskResetConfirmation();

            if (typedText != "RESET")
            {
                MessageBox.Show("Reset cancelled. You must type RESET exactly.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                File.WriteAllText(settingsFile, "");
                txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                allFiles.Clear();
                currentFiles.Clear();
                lbFiles.Items.Clear();
                UpdateStats(currentFiles);
                lblStatus.Text = "Reset";

                MessageBox.Show("Settings reset successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reset error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        string AskResetConfirmation()
        {
            Form f = new Form();
            f.Text = "Final Reset Confirmation";
            f.Size = new Size(470, 230);
            f.StartPosition = FormStartPosition.CenterParent;
            f.BackColor = bg;
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;

            Label lbl = new Label();
            lbl.Text = "Final confirmation:" + Environment.NewLine + "Type RESET exactly to clear saved settings.";
            lbl.Location = new Point(25, 25);
            lbl.Size = new Size(400, 55);
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            f.Controls.Add(lbl);

            TextBox tb = new TextBox();
            tb.Location = new Point(25, 90);
            tb.Size = new Size(400, 25);
            f.Controls.Add(tb);

            Button ok = CreateButton("Confirm Reset", 75, 140, 140, 35, red);
            Button cancel = CreateButton("Cancel", 230, 140, 120, 35, dark);

            string value = "";

            ok.Click += (s, ev) =>
            {
                value = tb.Text;
                f.DialogResult = DialogResult.OK;
                f.Close();
            };

            cancel.Click += (s, ev) =>
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

        public class FileItem
        {
            public string FullPath;
            public string Name;
            public string Extension;
            public string Category;
            public long SizeBytes;
            public DateTime LastModified;

            public override string ToString()
            {
                return Name + " | " + Category + " | " + FormatSizeStatic(SizeBytes) + " | Modified: " + LastModified.ToString("yyyy/MM/dd HH:mm");
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