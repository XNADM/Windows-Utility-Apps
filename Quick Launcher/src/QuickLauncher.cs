using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace QuickLauncherApp
{
    public class QuickLauncherForm : Form
    {
        TextBox txtName;
        TextBox txtPath;
        TextBox txtNotes;
        TextBox txtSearch;

        ComboBox cbType;
        ComboBox cbCategory;
        ComboBox cbFilter;

        ListBox lbShortcuts;

        Button btnAdd;
        Button btnUpdate;
        Button btnDelete;
        Button btnOpen;
        Button btnBrowse;
        Button btnClear;
        Button btnSearch;
        Button btnShowAll;
        Button btnHowToUse;
        Button btnReset;
        Button btnCopyPath;

        Label lblTotal;
        Label lblWebsites;
        Label lblApps;
        Label lblFilesFolders;
        Label lblStatus;

        List<ShortcutItem> shortcuts = new List<ShortcutItem>();
        int selectedID = -1;

        static string userName = Environment.UserName;
        static string dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            userName + "_QuickLauncher_Data"
        );

        static string dataFile = Path.Combine(dataFolder, "launcher_data.txt");

        Color bg = Color.FromArgb(239, 243, 248);
        Color dark = Color.FromArgb(31, 41, 55);
        Color blue = Color.FromArgb(41, 128, 185);
        Color green = Color.FromArgb(39, 174, 96);
        Color red = Color.FromArgb(192, 57, 43);
        Color orange = Color.FromArgb(243, 156, 18);
        Color purple = Color.FromArgb(91, 75, 138);
        Color gray = Color.FromArgb(127, 140, 141);
        Color card = Color.White;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new QuickLauncherForm());
        }

        public QuickLauncherForm()
        {
            PrepareDataFolder();
            BuildUI();
            LoadShortcuts();
            RefreshList();
        }

        void BuildUI()
        {
            this.Text = "Quick Launcher";
            this.Size = new Size(1120, 760);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bg;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            Label title = new Label();
            title.Text = "Quick Launcher";
            title.Font = new Font("Segoe UI", 25, FontStyle.Bold);
            title.ForeColor = dark;
            title.Location = new Point(30, 18);
            title.Size = new Size(430, 45);
            this.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Open your favorite websites, apps, files, and folders from one clean dashboard.";
            subtitle.Font = new Font("Segoe UI", 10, FontStyle.Italic);
            subtitle.ForeColor = Color.FromArgb(90, 90, 90);
            subtitle.Location = new Point(35, 62);
            subtitle.Size = new Size(760, 25);
            this.Controls.Add(subtitle);

            btnReset = CreateButton("Reset Data", 950, 25, 120, 30, red);
            btnReset.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            Panel statCard = CreateCard(30, 100, 1040, 82);
            this.Controls.Add(statCard);

            lblTotal = CreateStatBox(statCard, "Total Shortcuts", "0", 25, 12, blue);
            lblWebsites = CreateStatBox(statCard, "Websites", "0", 285, 12, purple);
            lblApps = CreateStatBox(statCard, "Apps", "0", 545, 12, green);
            lblFilesFolders = CreateStatBox(statCard, "Files / Folders", "0", 805, 12, orange);

            Panel inputCard = CreateCard(30, 205, 420, 410);
            this.Controls.Add(inputCard);

            Label inputHeader = CreateLabel("Create / Edit Shortcut", 20, 15, 300, 30, FontStyle.Bold);
            inputHeader.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            inputHeader.ForeColor = blue;
            inputCard.Controls.Add(inputHeader);

            inputCard.Controls.Add(CreateLabel("Name:", 20, 58, 100, 25, FontStyle.Regular));
            txtName = new TextBox();
            txtName.Location = new Point(130, 58);
            txtName.Size = new Size(250, 25);
            inputCard.Controls.Add(txtName);

            inputCard.Controls.Add(CreateLabel("Type:", 20, 98, 100, 25, FontStyle.Regular));
            cbType = new ComboBox();
            cbType.Location = new Point(130, 98);
            cbType.Size = new Size(150, 25);
            cbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbType.Items.Add("Website");
            cbType.Items.Add("App");
            cbType.Items.Add("Folder");
            cbType.Items.Add("File");
            cbType.SelectedIndex = 0;
            inputCard.Controls.Add(cbType);

            inputCard.Controls.Add(CreateLabel("Category:", 20, 138, 100, 25, FontStyle.Regular));
            cbCategory = new ComboBox();
            cbCategory.Location = new Point(130, 138);
            cbCategory.Size = new Size(150, 25);
            cbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCategory.Items.Add("General");
            cbCategory.Items.Add("Study");
            cbCategory.Items.Add("Work");
            cbCategory.Items.Add("Tools");
            cbCategory.Items.Add("Gaming");
            cbCategory.Items.Add("Personal");
            cbCategory.SelectedIndex = 0;
            inputCard.Controls.Add(cbCategory);

            inputCard.Controls.Add(CreateLabel("Path / URL:", 20, 178, 100, 25, FontStyle.Regular));
            txtPath = new TextBox();
            txtPath.Location = new Point(130, 178);
            txtPath.Size = new Size(250, 25);
            inputCard.Controls.Add(txtPath);

            btnBrowse = CreateButton("Browse", 130, 213, 95, 30, purple);
            btnBrowse.Click += BtnBrowse_Click;
            inputCard.Controls.Add(btnBrowse);

            btnCopyPath = CreateButton("Copy Path", 235, 213, 145, 30, gray);
            btnCopyPath.Click += BtnCopyPath_Click;
            inputCard.Controls.Add(btnCopyPath);

            inputCard.Controls.Add(CreateLabel("Notes:", 20, 258, 100, 25, FontStyle.Regular));
            txtNotes = new TextBox();
            txtNotes.Location = new Point(130, 258);
            txtNotes.Size = new Size(250, 55);
            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            inputCard.Controls.Add(txtNotes);

            btnAdd = CreateButton("Add", 20, 340, 75, 32, green);
            btnAdd.Click += BtnAdd_Click;
            inputCard.Controls.Add(btnAdd);

            btnUpdate = CreateButton("Update", 105, 340, 85, 32, blue);
            btnUpdate.Click += BtnUpdate_Click;
            inputCard.Controls.Add(btnUpdate);

            btnOpen = CreateButton("Open", 200, 340, 75, 32, purple);
            btnOpen.Click += BtnOpen_Click;
            inputCard.Controls.Add(btnOpen);

            btnDelete = CreateButton("Delete", 285, 340, 95, 32, red);
            btnDelete.Click += BtnDelete_Click;
            inputCard.Controls.Add(btnDelete);

            Panel tipsCard = CreateCard(30, 635, 420, 45);
            tipsCard.BackColor = Color.FromArgb(255, 252, 246);
            this.Controls.Add(tipsCard);

            Label tips = new Label();
            tips.Text = "Tip: For websites, write the full URL like https://example.com";
            tips.Location = new Point(15, 12);
            tips.Size = new Size(390, 25);
            tips.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            tips.ForeColor = Color.FromArgb(90, 90, 90);
            tipsCard.Controls.Add(tips);

            Panel listCard = CreateCard(470, 205, 600, 410);
            this.Controls.Add(listCard);

            Label listHeader = CreateLabel("Launcher List", 20, 15, 220, 30, FontStyle.Bold);
            listHeader.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            listHeader.ForeColor = dark;
            listCard.Controls.Add(listHeader);

            listCard.Controls.Add(CreateLabel("Search:", 20, 58, 70, 25, FontStyle.Regular));
            txtSearch = new TextBox();
            txtSearch.Location = new Point(90, 58);
            txtSearch.Size = new Size(190, 25);
            listCard.Controls.Add(txtSearch);

            btnSearch = CreateButton("Search", 290, 55, 85, 30, blue);
            btnSearch.Click += BtnSearch_Click;
            listCard.Controls.Add(btnSearch);

            listCard.Controls.Add(CreateLabel("Filter:", 390, 58, 55, 25, FontStyle.Regular));
            cbFilter = new ComboBox();
            cbFilter.Location = new Point(445, 58);
            cbFilter.Size = new Size(130, 25);
            cbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilter.Items.Add("All");
            cbFilter.Items.Add("Website");
            cbFilter.Items.Add("App");
            cbFilter.Items.Add("Folder");
            cbFilter.Items.Add("File");
            cbFilter.Items.Add("Study");
            cbFilter.Items.Add("Work");
            cbFilter.Items.Add("Tools");
            cbFilter.Items.Add("Gaming");
            cbFilter.Items.Add("Personal");
            cbFilter.SelectedIndex = 0;
            cbFilter.SelectedIndexChanged += CbFilter_SelectedIndexChanged;
            listCard.Controls.Add(cbFilter);

            lbShortcuts = new ListBox();
            lbShortcuts.Location = new Point(20, 100);
            lbShortcuts.Size = new Size(555, 230);
            lbShortcuts.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lbShortcuts.SelectedIndexChanged += LbShortcuts_SelectedIndexChanged;
            lbShortcuts.DoubleClick += LbShortcuts_DoubleClick;
            listCard.Controls.Add(lbShortcuts);

            btnShowAll = CreateButton("Show All", 20, 350, 95, 32, dark);
            btnShowAll.Click += BtnShowAll_Click;
            listCard.Controls.Add(btnShowAll);

            btnClear = CreateButton("Clear Fields", 125, 350, 120, 32, gray);
            btnClear.Click += BtnClear_Click;
            listCard.Controls.Add(btnClear);

            btnHowToUse = CreateButton("How To Use", 255, 350, 125, 32, orange);
            btnHowToUse.Click += BtnHowToUse_Click;
            listCard.Controls.Add(btnHowToUse);

            lblStatus = new Label();
            lblStatus.Text = "Ready";
            lblStatus.Location = new Point(400, 355);
            lblStatus.Size = new Size(175, 25);
            lblStatus.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
            listCard.Controls.Add(lblStatus);

            Label developedBy = new Label();
            developedBy.Text = "Developed by Mohammed Ahmed Alhijab";
            developedBy.Location = new Point(0, 705);
            developedBy.Size = new Size(1120, 25);
            developedBy.TextAlign = ContentAlignment.MiddleCenter;
            developedBy.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            developedBy.ForeColor = Color.FromArgb(80, 80, 80);
            this.Controls.Add(developedBy);
        }

        Label CreateStatBox(Panel parent, string label, string value, int x, int y, Color color)
        {
            Label small = new Label();
            small.Text = label;
            small.Location = new Point(x, y);
            small.Size = new Size(210, 22);
            small.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            small.ForeColor = Color.FromArgb(100, 100, 100);
            parent.Controls.Add(small);

            Label big = new Label();
            big.Text = value;
            big.Location = new Point(x, y + 25);
            big.Size = new Size(210, 35);
            big.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            big.ForeColor = color;
            parent.Controls.Add(big);

            return big;
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

            if (!File.Exists(dataFile))
                File.WriteAllText(dataFile, "");
        }

        void LoadShortcuts()
        {
            shortcuts.Clear();

            if (!File.Exists(dataFile))
                return;

            string[] lines = File.ReadAllLines(dataFile);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('|');

                if (parts.Length != 7)
                    continue;

                try
                {
                    ShortcutItem item = new ShortcutItem();
                    item.ID = Convert.ToInt32(parts[0]);
                    item.Name = Decode(parts[1]);
                    item.Type = Decode(parts[2]);
                    item.Category = Decode(parts[3]);
                    item.PathOrUrl = Decode(parts[4]);
                    item.Notes = Decode(parts[5]);
                    item.UpdatedDate = Decode(parts[6]);

                    shortcuts.Add(item);
                }
                catch
                {
                }
            }
        }

        void SaveShortcuts()
        {
            List<string> lines = new List<string>();

            foreach (ShortcutItem item in shortcuts)
            {
                string line =
                    item.ID + "|" +
                    Encode(item.Name) + "|" +
                    Encode(item.Type) + "|" +
                    Encode(item.Category) + "|" +
                    Encode(item.PathOrUrl) + "|" +
                    Encode(item.Notes) + "|" +
                    Encode(item.UpdatedDate);

                lines.Add(line);
            }

            File.WriteAllLines(dataFile, lines.ToArray());
        }

        string Encode(string value)
        {
            if (value == null)
                value = "";

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        string Decode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        int GetNextID()
        {
            int max = 0;

            foreach (ShortcutItem item in shortcuts)
            {
                if (item.ID > max)
                    max = item.ID;
            }

            return max + 1;
        }

        void RefreshList()
        {
            lbShortcuts.Items.Clear();

            List<ShortcutItem> displayItems = new List<ShortcutItem>();

            foreach (ShortcutItem item in shortcuts)
                displayItems.Add(item);

            displayItems.Sort((a, b) =>
            {
                int typeCompare = a.Type.CompareTo(b.Type);

                if (typeCompare != 0)
                    return typeCompare;

                return a.Name.CompareTo(b.Name);
            });

            foreach (ShortcutItem item in displayItems)
                lbShortcuts.Items.Add(item);

            lblStatus.Text = "Total: " + shortcuts.Count;
            UpdateStats();
        }

        void UpdateStats()
        {
            int websites = 0;
            int apps = 0;
            int filesFolders = 0;

            foreach (ShortcutItem item in shortcuts)
            {
                if (item.Type == "Website")
                    websites++;

                if (item.Type == "App")
                    apps++;

                if (item.Type == "File" || item.Type == "Folder")
                    filesFolders++;
            }

            lblTotal.Text = shortcuts.Count.ToString();
            lblWebsites.Text = websites.ToString();
            lblApps.Text = apps.ToString();
            lblFilesFolders.Text = filesFolders.ToString();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string path = txtPath.Text.Trim();

            if (name == "")
            {
                MessageBox.Show("Please enter a shortcut name.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (path == "")
            {
                MessageBox.Show("Please enter a path or URL.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidatePathByType(cbType.Text, path))
                return;

            ShortcutItem item = new ShortcutItem();
            item.ID = GetNextID();
            item.Name = name;
            item.Type = cbType.Text;
            item.Category = cbCategory.Text;
            item.PathOrUrl = path;
            item.Notes = txtNotes.Text.Trim();
            item.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            shortcuts.Add(item);
            SaveShortcuts();
            RefreshList();
            ClearFields();

            MessageBox.Show("Shortcut added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedID == -1)
            {
                MessageBox.Show("Please select a shortcut to update.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShortcutItem item = FindByID(selectedID);

            if (item == null)
            {
                MessageBox.Show("Selected shortcut was not found.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtName.Text.Trim();
            string path = txtPath.Text.Trim();

            if (name == "")
            {
                MessageBox.Show("Please enter a shortcut name.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (path == "")
            {
                MessageBox.Show("Please enter a path or URL.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidatePathByType(cbType.Text, path))
                return;

            item.Name = name;
            item.Type = cbType.Text;
            item.Category = cbCategory.Text;
            item.PathOrUrl = path;
            item.Notes = txtNotes.Text.Trim();
            item.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            SaveShortcuts();
            RefreshList();
            ClearFields();

            MessageBox.Show("Shortcut updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        bool ValidatePathByType(string type, string path)
        {
            if (type == "Website")
            {
                if (!path.StartsWith("http://") && !path.StartsWith("https://"))
                {
                    MessageBox.Show("Website URL must start with http:// or https://", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                return true;
            }

            if (type == "Folder")
            {
                if (!Directory.Exists(path))
                {
                    MessageBox.Show("Folder path does not exist.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                return true;
            }

            if (type == "File" || type == "App")
            {
                if (!File.Exists(path))
                {
                    MessageBox.Show("File or app path does not exist.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                return true;
            }

            return true;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            string type = cbType.Text;

            if (type == "Website")
            {
                MessageBox.Show("For websites, type or paste the URL manually. Example: https://google.com", "Website URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (type == "Folder")
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = dialog.SelectedPath;
                }

                return;
            }

            OpenFileDialog fileDialog = new OpenFileDialog();

            if (type == "App")
                fileDialog.Filter = "Applications|*.exe;*.lnk;*.bat;*.cmd|All Files|*.*";
            else
                fileDialog.Filter = "All Files|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = fileDialog.FileName;
            }
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            if (selectedID == -1)
            {
                MessageBox.Show("Please select a shortcut to open.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShortcutItem item = FindByID(selectedID);

            if (item == null)
            {
                MessageBox.Show("Selected shortcut was not found.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenShortcut(item);
        }

        private void LbShortcuts_DoubleClick(object sender, EventArgs e)
        {
            if (lbShortcuts.SelectedItem == null)
                return;

            ShortcutItem item = (ShortcutItem)lbShortcuts.SelectedItem;
            OpenShortcut(item);
        }

        void OpenShortcut(ShortcutItem item)
        {
            try
            {
                if (item.Type == "Website")
                {
                    Process.Start(item.PathOrUrl);
                    lblStatus.Text = "Opened website";
                    return;
                }

                if (item.Type == "Folder")
                {
                    if (!Directory.Exists(item.PathOrUrl))
                    {
                        MessageBox.Show("Folder was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Process.Start(item.PathOrUrl);
                    lblStatus.Text = "Opened folder";
                    return;
                }

                if (item.Type == "File" || item.Type == "App")
                {
                    if (!File.Exists(item.PathOrUrl))
                    {
                        MessageBox.Show("File or app was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Process.Start(item.PathOrUrl);
                    lblStatus.Text = "Opened item";
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Open error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Open error";
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedID == -1)
            {
                MessageBox.Show("Please select a shortcut to delete.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShortcutItem item = FindByID(selectedID);

            if (item == null)
            {
                MessageBox.Show("Selected shortcut was not found.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to delete this shortcut?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            shortcuts.Remove(item);
            SaveShortcuts();
            RefreshList();
            ClearFields();

            MessageBox.Show("Shortcut deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCopyPath_Click(object sender, EventArgs e)
        {
            if (txtPath.Text.Trim() == "")
            {
                MessageBox.Show("No path or URL to copy.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Clipboard.SetText(txtPath.Text.Trim());
            lblStatus.Text = "Copied";
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim().ToLower();

            if (search == "")
            {
                RefreshList();
                return;
            }

            lbShortcuts.Items.Clear();

            foreach (ShortcutItem item in shortcuts)
            {
                if (item.Name.ToLower().Contains(search) ||
                    item.Type.ToLower().Contains(search) ||
                    item.Category.ToLower().Contains(search) ||
                    item.PathOrUrl.ToLower().Contains(search) ||
                    item.Notes.ToLower().Contains(search))
                {
                    lbShortcuts.Items.Add(item);
                }
            }

            lblStatus.Text = "Search: " + lbShortcuts.Items.Count;
        }

        private void CbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        void ApplyFilter()
        {
            string filter = cbFilter.Text;

            lbShortcuts.Items.Clear();

            foreach (ShortcutItem item in shortcuts)
            {
                bool show = false;

                if (filter == "All")
                    show = true;
                else if (item.Type == filter)
                    show = true;
                else if (item.Category == filter)
                    show = true;

                if (show)
                    lbShortcuts.Items.Add(item);
            }

            lblStatus.Text = filter + ": " + lbShortcuts.Items.Count;
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cbFilter.SelectedIndex = 0;
            RefreshList();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void LbShortcuts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbShortcuts.SelectedItem == null)
                return;

            ShortcutItem item = (ShortcutItem)lbShortcuts.SelectedItem;

            selectedID = item.ID;
            txtName.Text = item.Name;
            cbType.Text = item.Type;
            cbCategory.Text = item.Category;
            txtPath.Text = item.PathOrUrl;
            txtNotes.Text = item.Notes;

            lblStatus.Text = "Selected ID: " + item.ID;
        }

        ShortcutItem FindByID(int id)
        {
            foreach (ShortcutItem item in shortcuts)
            {
                if (item.ID == id)
                    return item;
            }

            return null;
        }

        void ClearFields()
        {
            selectedID = -1;
            txtName.Clear();
            txtPath.Clear();
            txtNotes.Clear();
            txtSearch.Clear();
            cbType.SelectedIndex = 0;
            cbCategory.SelectedIndex = 0;
            cbFilter.SelectedIndex = 0;
            lbShortcuts.ClearSelected();
            lblStatus.Text = "Ready";
        }

        private void BtnHowToUse_Click(object sender, EventArgs e)
        {
            string msg = "";

            msg += "Quick Launcher - How To Use" + Environment.NewLine;
            msg += "---------------------------" + Environment.NewLine + Environment.NewLine;

            msg += "Add Shortcut:" + Environment.NewLine;
            msg += "- Write a shortcut name." + Environment.NewLine;
            msg += "- Choose type: Website, App, Folder, or File." + Environment.NewLine;
            msg += "- Choose a category." + Environment.NewLine;
            msg += "- Add the path or URL." + Environment.NewLine;
            msg += "- Click Add." + Environment.NewLine + Environment.NewLine;

            msg += "Website:" + Environment.NewLine;
            msg += "- URL must start with http:// or https://." + Environment.NewLine;
            msg += "- Example: https://google.com" + Environment.NewLine + Environment.NewLine;

            msg += "App / File / Folder:" + Environment.NewLine;
            msg += "- Use Browse to choose the file, app, or folder." + Environment.NewLine;
            msg += "- Double-click any item in the list to open it quickly." + Environment.NewLine + Environment.NewLine;

            msg += "Update / Delete:" + Environment.NewLine;
            msg += "- Select a shortcut from the list." + Environment.NewLine;
            msg += "- Edit the fields then click Update." + Environment.NewLine;
            msg += "- Click Delete to remove it." + Environment.NewLine + Environment.NewLine;

            msg += "Search and Filter:" + Environment.NewLine;
            msg += "- Search by name, type, category, path, or notes." + Environment.NewLine;
            msg += "- Filter by type or category." + Environment.NewLine + Environment.NewLine;

            msg += "Reset Data:" + Environment.NewLine;
            msg += "- Reset Data deletes all saved shortcuts." + Environment.NewLine;
            msg += "- It requires two confirmations." + Environment.NewLine + Environment.NewLine;

            msg += "Data Storage:" + Environment.NewLine;
            msg += "- Shortcuts are saved automatically in AppData." + Environment.NewLine;
            msg += "- No Access database is required." + Environment.NewLine;
            msg += "- No SQLite files are required." + Environment.NewLine + Environment.NewLine;

            msg += "Data path:" + Environment.NewLine;
            msg += dataFile;

            MessageBox.Show(msg, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            DialogResult firstConfirm = MessageBox.Show(
                "Warning: This will delete all saved shortcuts." + Environment.NewLine + Environment.NewLine +
                "Do you want to continue?",
                "Reset Data - First Confirmation",
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

            shortcuts.Clear();
            SaveShortcuts();
            RefreshList();
            ClearFields();

            MessageBox.Show("All shortcuts were deleted successfully.", "Reset Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            lbl.Text = "Final confirmation:" + Environment.NewLine + "Type RESET exactly to delete all shortcuts.";
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

        public class ShortcutItem
        {
            public int ID;
            public string Name;
            public string Type;
            public string Category;
            public string PathOrUrl;
            public string Notes;
            public string UpdatedDate;

            public override string ToString()
            {
                return "[" + Type + "] " + ID + ": " + Name + " | " + Category + " | " + PathOrUrl;
            }
        }
    }
}