using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ReminderCenterApp
{
    public class ReminderCenterForm : Form
    {
        TextBox txtTitle;
        TextBox txtNotes;
        TextBox txtSearch;

        ComboBox cbPriority;
        ComboBox cbStatus;
        ComboBox cbFilter;
        ComboBox cbSnoozeUnit;

        CheckBox chkEnableSnooze;
        NumericUpDown numSnooze;

        DateTimePicker dpDate;
        DateTimePicker dpTime;

        ListBox lbReminders;

        Button btnAdd;
        Button btnUpdate;
        Button btnDelete;
        Button btnComplete;
        Button btnSnooze;
        Button btnClear;
        Button btnSearch;
        Button btnShowAll;
        Button btnToday;
        Button btnHowToUse;
        Button btnReset;
        Button btnExit;

        Label lblTotal;
        Label lblPending;
        Label lblCompleted;
        Label lblDueToday;
        Label lblNextReminder;
        Label lblStatus;
        Label lblTrayNote;

        ProgressBar progressCompleted;

        Timer reminderTimer;

        NotifyIcon trayIcon;
        ContextMenuStrip trayMenu;
        bool allowExit = false;
        bool firstHideMessageShown = false;

        List<ReminderItem> reminders = new List<ReminderItem>();
        List<int> alreadyAlertedIDs = new List<int>();

        int selectedReminderID = -1;

        static string userName = Environment.UserName;
        static string dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            userName + "_ReminderCenter_Data"
        );

        static string dataFile = Path.Combine(dataFolder, "reminders_data.txt");

        Color bg = Color.FromArgb(250, 247, 242);
        Color dark = Color.FromArgb(40, 44, 52);
        Color navy = Color.FromArgb(30, 45, 70);
        Color red = Color.FromArgb(192, 57, 43);
        Color orange = Color.FromArgb(230, 126, 34);
        Color green = Color.FromArgb(39, 174, 96);
        Color blue = Color.FromArgb(41, 128, 185);
        Color gray = Color.FromArgb(127, 140, 141);
        Color card = Color.White;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ReminderCenterForm());
        }

        public ReminderCenterForm()
        {
            PrepareDataFolder();
            BuildTray();
            BuildUI();
            LoadReminders();
            RefreshList();
            StartReminderTimer();
        }

        void BuildTray()
        {
            trayMenu = new ContextMenuStrip();

            ToolStripMenuItem openItem = new ToolStripMenuItem("Open Reminder Center");
            openItem.Click += OpenItem_Click;

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += ExitItem_Click;

            trayMenu.Items.Add(openItem);
            trayMenu.Items.Add(exitItem);

            trayIcon = new NotifyIcon();

            try
            {
                trayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                trayIcon.Icon = SystemIcons.Information;
            }

            trayIcon.Text = "Reminder Center";
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
        }

        void BuildUI()
        {
            this.Text = "Reminder Center";
            this.Size = new Size(1120, 790);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bg;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.FormClosing += ReminderCenterForm_FormClosing;

            Label title = new Label();
            title.Text = "Reminder Center";
            title.Font = new Font("Segoe UI", 25, FontStyle.Bold);
            title.ForeColor = dark;
            title.Location = new Point(30, 18);
            title.Size = new Size(430, 45);
            this.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Stay on time. Add reminders, snooze them your way, and keep alerts running in the tray.";
            subtitle.Font = new Font("Segoe UI", 10, FontStyle.Italic);
            subtitle.ForeColor = Color.FromArgb(90, 90, 90);
            subtitle.Location = new Point(35, 62);
            subtitle.Size = new Size(760, 25);
            this.Controls.Add(subtitle);

            btnReset = CreateButton("Reset Data", 840, 25, 110, 30, red);
            btnReset.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            btnExit = CreateButton("Exit App", 960, 25, 110, 30, dark);
            btnExit.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnExit.Click += BtnExit_Click;
            this.Controls.Add(btnExit);

            Panel nextCard = CreateCard(30, 100, 1040, 105);
            nextCard.BackColor = Color.FromArgb(255, 252, 246);
            this.Controls.Add(nextCard);

            Label nextTitle = CreateLabel("Next Reminder", 25, 15, 220, 30, FontStyle.Bold);
            nextTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            nextTitle.ForeColor = orange;
            nextCard.Controls.Add(nextTitle);

            lblNextReminder = new Label();
            lblNextReminder.Text = "No pending reminders";
            lblNextReminder.Location = new Point(25, 50);
            lblNextReminder.Size = new Size(730, 32);
            lblNextReminder.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblNextReminder.ForeColor = dark;
            nextCard.Controls.Add(lblNextReminder);

            progressCompleted = new ProgressBar();
            progressCompleted.Location = new Point(780, 48);
            progressCompleted.Size = new Size(220, 22);
            progressCompleted.Minimum = 0;
            progressCompleted.Maximum = 100;
            nextCard.Controls.Add(progressCompleted);

            Label progressLabel = CreateLabel("Completion progress", 780, 20, 220, 25, FontStyle.Bold);
            progressLabel.ForeColor = gray;
            nextCard.Controls.Add(progressLabel);

            Panel statCard = CreateCard(30, 225, 1040, 80);
            this.Controls.Add(statCard);

            lblTotal = CreateStatBox(statCard, "Total", "0", 25, 12);
            lblPending = CreateStatBox(statCard, "Pending", "0", 280, 12);
            lblCompleted = CreateStatBox(statCard, "Completed", "0", 535, 12);
            lblDueToday = CreateStatBox(statCard, "Due Today", "0", 790, 12);

            Panel inputCard = CreateCard(30, 325, 420, 345);
            this.Controls.Add(inputCard);

            Label inputHeader = CreateLabel("Create / Edit", 20, 15, 250, 30, FontStyle.Bold);
            inputHeader.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            inputHeader.ForeColor = orange;
            inputCard.Controls.Add(inputHeader);

            inputCard.Controls.Add(CreateLabel("Title:", 20, 58, 100, 25, FontStyle.Regular));
            txtTitle = new TextBox();
            txtTitle.Location = new Point(130, 58);
            txtTitle.Size = new Size(250, 25);
            inputCard.Controls.Add(txtTitle);

            inputCard.Controls.Add(CreateLabel("Priority:", 20, 95, 100, 25, FontStyle.Regular));
            cbPriority = new ComboBox();
            cbPriority.Location = new Point(130, 95);
            cbPriority.Size = new Size(140, 25);
            cbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPriority.Items.Add("High");
            cbPriority.Items.Add("Medium");
            cbPriority.Items.Add("Low");
            cbPriority.SelectedIndex = 1;
            inputCard.Controls.Add(cbPriority);

            inputCard.Controls.Add(CreateLabel("Status:", 20, 132, 100, 25, FontStyle.Regular));
            cbStatus = new ComboBox();
            cbStatus.Location = new Point(130, 132);
            cbStatus.Size = new Size(140, 25);
            cbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cbStatus.Items.Add("Pending");
            cbStatus.Items.Add("Completed");
            cbStatus.SelectedIndex = 0;
            inputCard.Controls.Add(cbStatus);

            inputCard.Controls.Add(CreateLabel("Date:", 20, 169, 100, 25, FontStyle.Regular));
            dpDate = new DateTimePicker();
            dpDate.Location = new Point(130, 169);
            dpDate.Size = new Size(140, 25);
            dpDate.Format = DateTimePickerFormat.Short;
            inputCard.Controls.Add(dpDate);

            inputCard.Controls.Add(CreateLabel("Time:", 20, 206, 100, 25, FontStyle.Regular));
            dpTime = new DateTimePicker();
            dpTime.Location = new Point(130, 206);
            dpTime.Size = new Size(140, 25);
            dpTime.Format = DateTimePickerFormat.Time;
            dpTime.ShowUpDown = true;
            inputCard.Controls.Add(dpTime);

            inputCard.Controls.Add(CreateLabel("Notes:", 20, 243, 100, 25, FontStyle.Regular));
            txtNotes = new TextBox();
            txtNotes.Location = new Point(130, 243);
            txtNotes.Size = new Size(250, 42);
            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            inputCard.Controls.Add(txtNotes);

            btnAdd = CreateButton("Add", 20, 295, 75, 32, green);
            btnAdd.Click += BtnAdd_Click;
            inputCard.Controls.Add(btnAdd);

            btnUpdate = CreateButton("Update", 105, 295, 85, 32, blue);
            btnUpdate.Click += BtnUpdate_Click;
            inputCard.Controls.Add(btnUpdate);

            btnComplete = CreateButton("Done", 200, 295, 75, 32, orange);
            btnComplete.Click += BtnComplete_Click;
            inputCard.Controls.Add(btnComplete);

            btnDelete = CreateButton("Delete", 285, 295, 95, 32, red);
            btnDelete.Click += BtnDelete_Click;
            inputCard.Controls.Add(btnDelete);

            Panel snoozeCard = CreateCard(470, 325, 600, 105);
            snoozeCard.BackColor = Color.FromArgb(255, 252, 246);
            this.Controls.Add(snoozeCard);

            Label snoozeTitle = CreateLabel("Custom Snooze", 20, 15, 200, 28, FontStyle.Bold);
            snoozeTitle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            snoozeTitle.ForeColor = orange;
            snoozeCard.Controls.Add(snoozeTitle);

            chkEnableSnooze = new CheckBox();
            chkEnableSnooze.Text = "Enable Snooze";
            chkEnableSnooze.Location = new Point(20, 58);
            chkEnableSnooze.Size = new Size(140, 25);
            chkEnableSnooze.CheckedChanged += ChkEnableSnooze_CheckedChanged;
            snoozeCard.Controls.Add(chkEnableSnooze);

            numSnooze = new NumericUpDown();
            numSnooze.Location = new Point(170, 58);
            numSnooze.Size = new Size(75, 25);
            numSnooze.Minimum = 1;
            numSnooze.Maximum = 999;
            numSnooze.Value = 20;
            snoozeCard.Controls.Add(numSnooze);

            cbSnoozeUnit = new ComboBox();
            cbSnoozeUnit.Location = new Point(255, 58);
            cbSnoozeUnit.Size = new Size(115, 25);
            cbSnoozeUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cbSnoozeUnit.Items.Add("Minutes");
            cbSnoozeUnit.Items.Add("Hours");
            cbSnoozeUnit.Items.Add("Days");
            cbSnoozeUnit.SelectedIndex = 0;
            snoozeCard.Controls.Add(cbSnoozeUnit);

            btnSnooze = CreateButton("Snooze Selected", 390, 53, 165, 32, orange);
            btnSnooze.Click += BtnSnooze_Click;
            snoozeCard.Controls.Add(btnSnooze);

            SetSnoozeControls(false);

            Panel listCard = CreateCard(470, 450, 600, 220);
            this.Controls.Add(listCard);

            Label listHeader = CreateLabel("Reminders", 20, 15, 200, 28, FontStyle.Bold);
            listHeader.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            listHeader.ForeColor = navy;
            listCard.Controls.Add(listHeader);

            listCard.Controls.Add(CreateLabel("Search:", 20, 52, 70, 25, FontStyle.Regular));
            txtSearch = new TextBox();
            txtSearch.Location = new Point(90, 52);
            txtSearch.Size = new Size(185, 25);
            listCard.Controls.Add(txtSearch);

            btnSearch = CreateButton("Search", 285, 49, 85, 30, blue);
            btnSearch.Click += BtnSearch_Click;
            listCard.Controls.Add(btnSearch);

            listCard.Controls.Add(CreateLabel("Filter:", 385, 52, 55, 25, FontStyle.Regular));
            cbFilter = new ComboBox();
            cbFilter.Location = new Point(440, 52);
            cbFilter.Size = new Size(135, 25);
            cbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilter.Items.Add("All");
            cbFilter.Items.Add("Pending");
            cbFilter.Items.Add("Completed");
            cbFilter.Items.Add("High");
            cbFilter.Items.Add("Today");
            cbFilter.Items.Add("Overdue");
            cbFilter.SelectedIndex = 0;
            cbFilter.SelectedIndexChanged += CbFilter_SelectedIndexChanged;
            listCard.Controls.Add(cbFilter);

            lbReminders = new ListBox();
            lbReminders.Location = new Point(20, 88);
            lbReminders.Size = new Size(555, 78);
            lbReminders.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lbReminders.SelectedIndexChanged += LbReminders_SelectedIndexChanged;
            listCard.Controls.Add(lbReminders);

            btnShowAll = CreateButton("Show All", 20, 176, 90, 30, dark);
            btnShowAll.Click += BtnShowAll_Click;
            listCard.Controls.Add(btnShowAll);

            btnToday = CreateButton("Today", 120, 176, 75, 30, orange);
            btnToday.Click += BtnToday_Click;
            listCard.Controls.Add(btnToday);

            btnClear = CreateButton("Clear", 205, 176, 75, 30, gray);
            btnClear.Click += BtnClear_Click;
            listCard.Controls.Add(btnClear);

            btnHowToUse = CreateButton("How To Use", 290, 176, 120, 30, navy);
            btnHowToUse.Click += BtnHowToUse_Click;
            listCard.Controls.Add(btnHowToUse);

            lblStatus = new Label();
            lblStatus.Text = "Ready";
            lblStatus.Location = new Point(430, 181);
            lblStatus.Size = new Size(145, 25);
            lblStatus.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
            listCard.Controls.Add(lblStatus);

            lblTrayNote = new Label();
            lblTrayNote.Text = "Tip: Clicking X hides the app to the tray. Reminders keep working. Use Exit App or tray menu to close it fully.";
            lblTrayNote.Location = new Point(30, 690);
            lblTrayNote.Size = new Size(1040, 24);
            lblTrayNote.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblTrayNote.ForeColor = Color.FromArgb(90, 90, 90);
            lblTrayNote.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTrayNote);

            Label developedBy = new Label();
            developedBy.Text = "Developed by Mohammed Ahmed Alhijab";
            developedBy.Location = new Point(0, 718);
            developedBy.Size = new Size(1120, 25);
            developedBy.TextAlign = ContentAlignment.MiddleCenter;
            developedBy.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            developedBy.ForeColor = Color.FromArgb(80, 80, 80);
            this.Controls.Add(developedBy);
        }

        void SetSnoozeControls(bool enabled)
        {
            numSnooze.Enabled = enabled;
            cbSnoozeUnit.Enabled = enabled;
            btnSnooze.Enabled = enabled;

            if (enabled)
                btnSnooze.BackColor = orange;
            else
                btnSnooze.BackColor = gray;
        }

        private void ChkEnableSnooze_CheckedChanged(object sender, EventArgs e)
        {
            SetSnoozeControls(chkEnableSnooze.Checked);
        }

        Label CreateStatBox(Panel parent, string label, string value, int x, int y)
        {
            Label small = new Label();
            small.Text = label;
            small.Location = new Point(x, y);
            small.Size = new Size(190, 22);
            small.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            small.ForeColor = Color.FromArgb(100, 100, 100);
            parent.Controls.Add(small);

            Label big = new Label();
            big.Text = value;
            big.Location = new Point(x, y + 25);
            big.Size = new Size(190, 35);
            big.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            big.ForeColor = orange;
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

        void StartReminderTimer()
        {
            reminderTimer = new Timer();
            reminderTimer.Interval = 30000;
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();
        }

        void ReminderTimer_Tick(object sender, EventArgs e)
        {
            CheckDueReminders();
        }

        void CheckDueReminders()
        {
            DateTime now = DateTime.Now;

            foreach (ReminderItem item in reminders)
            {
                if (item.Status != "Pending")
                    continue;

                DateTime due;

                if (!DateTime.TryParse(item.ReminderDateTime, out due))
                    continue;

                if (due <= now && !alreadyAlertedIDs.Contains(item.ID))
                {
                    alreadyAlertedIDs.Add(item.ID);
                    ShowReminderAlert(item.ID);
                    break;
                }
            }
        }

        void ShowReminderAlert(int reminderID)
        {
            ReminderItem item = FindReminderByID(reminderID);

            if (item == null)
                return;

            if (item.Status != "Pending")
                return;

            if (this.WindowState == FormWindowState.Minimized || !this.Visible)
            {
                trayIcon.ShowBalloonTip(
                    5000,
                    "Reminder Due",
                    item.Title + " - " + item.ReminderDateTime,
                    ToolTipIcon.Info
                );
            }

            Form f = new Form();
            f.Text = "Reminder Alert";
            f.Size = new Size(560, 390);
            f.StartPosition = FormStartPosition.CenterScreen;
            f.BackColor = bg;
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.TopMost = true;

            Label title = new Label();
            title.Text = "Reminder Due";
            title.Location = new Point(25, 20);
            title.Size = new Size(480, 35);
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.ForeColor = orange;
            f.Controls.Add(title);

            Label details = new Label();
            details.Text =
                "Title: " + item.Title + Environment.NewLine +
                "Priority: " + item.Priority + Environment.NewLine +
                "Time: " + item.ReminderDateTime + Environment.NewLine +
                "Notes: " + item.Notes;
            details.Location = new Point(25, 70);
            details.Size = new Size(490, 95);
            details.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            details.ForeColor = dark;
            f.Controls.Add(details);

            Panel snoozePanel = new Panel();
            snoozePanel.Location = new Point(25, 175);
            snoozePanel.Size = new Size(490, 60);
            snoozePanel.BackColor = Color.FromArgb(255, 252, 246);
            snoozePanel.BorderStyle = BorderStyle.FixedSingle;
            f.Controls.Add(snoozePanel);

            Label snoozeText = new Label();
            snoozeText.Text = "Snooze for:";
            snoozeText.Location = new Point(15, 18);
            snoozeText.Size = new Size(95, 25);
            snoozeText.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            snoozeText.ForeColor = dark;
            snoozePanel.Controls.Add(snoozeText);

            NumericUpDown alertSnoozeNumber = new NumericUpDown();
            alertSnoozeNumber.Location = new Point(115, 18);
            alertSnoozeNumber.Size = new Size(80, 25);
            alertSnoozeNumber.Minimum = 1;
            alertSnoozeNumber.Maximum = 999;
            alertSnoozeNumber.Value = 20;
            snoozePanel.Controls.Add(alertSnoozeNumber);

            ComboBox alertSnoozeUnit = new ComboBox();
            alertSnoozeUnit.Location = new Point(205, 18);
            alertSnoozeUnit.Size = new Size(120, 25);
            alertSnoozeUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            alertSnoozeUnit.Items.Add("Minutes");
            alertSnoozeUnit.Items.Add("Hours");
            alertSnoozeUnit.Items.Add("Days");
            alertSnoozeUnit.SelectedIndex = 0;
            snoozePanel.Controls.Add(alertSnoozeUnit);

            Label hint = new Label();
            hint.Text = "Choose when you want this reminder to appear again.";
            hint.Location = new Point(25, 245);
            hint.Size = new Size(490, 25);
            hint.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            hint.ForeColor = gray;
            f.Controls.Add(hint);

            Button btnOk = CreateButton("OK", 35, 295, 90, 35, dark);
            Button btnCompleteAlert = CreateButton("Mark Completed", 145, 295, 155, 35, green);
            Button btnSnoozeAlert = CreateButton("Snooze Later", 320, 295, 145, 35, orange);

            btnOk.Click += (s, e) =>
            {
                f.Close();
            };

            btnCompleteAlert.Click += (s, e) =>
            {
                ReminderItem current = FindReminderByID(reminderID);

                if (current == null)
                {
                    MessageBox.Show("This reminder was deleted.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    f.Close();
                    return;
                }

                current.Status = "Completed";
                current.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

                if (alreadyAlertedIDs.Contains(current.ID))
                    alreadyAlertedIDs.Remove(current.ID);

                SaveReminders();
                RefreshList();
                f.Close();
            };

            btnSnoozeAlert.Click += (s, e) =>
            {
                ReminderItem current = FindReminderByID(reminderID);

                if (current == null)
                {
                    MessageBox.Show("This reminder was deleted, so it cannot be snoozed.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    f.Close();
                    return;
                }

                if (current.Status != "Pending")
                {
                    MessageBox.Show("This reminder is completed, so it cannot be snoozed.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    f.Close();
                    return;
                }

                int amount = Convert.ToInt32(alertSnoozeNumber.Value);
                int minutes = amount;

                if (alertSnoozeUnit.Text == "Hours")
                    minutes = amount * 60;
                else if (alertSnoozeUnit.Text == "Days")
                    minutes = amount * 1440;

                current.ReminderDateTime = DateTime.Now.AddMinutes(minutes).ToString("yyyy/MM/dd HH:mm");
                current.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

                if (alreadyAlertedIDs.Contains(current.ID))
                    alreadyAlertedIDs.Remove(current.ID);

                SaveReminders();
                RefreshList();
                f.Close();

                MessageBox.Show("Reminder snoozed successfully.", "Snoozed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            f.Controls.Add(btnOk);
            f.Controls.Add(btnCompleteAlert);
            f.Controls.Add(btnSnoozeAlert);

            f.Show();
        }

        void LoadReminders()
        {
            reminders.Clear();

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
                    ReminderItem item = new ReminderItem();
                    item.ID = Convert.ToInt32(parts[0]);
                    item.Title = Decode(parts[1]);
                    item.Priority = Decode(parts[2]);
                    item.Status = Decode(parts[3]);
                    item.ReminderDateTime = Decode(parts[4]);
                    item.Notes = Decode(parts[5]);
                    item.UpdatedDate = Decode(parts[6]);
                    reminders.Add(item);
                }
                catch
                {
                }
            }
        }

        void SaveReminders()
        {
            List<string> lines = new List<string>();

            foreach (ReminderItem item in reminders)
            {
                string line =
                    item.ID + "|" +
                    Encode(item.Title) + "|" +
                    Encode(item.Priority) + "|" +
                    Encode(item.Status) + "|" +
                    Encode(item.ReminderDateTime) + "|" +
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

            foreach (ReminderItem item in reminders)
            {
                if (item.ID > max)
                    max = item.ID;
            }

            return max + 1;
        }

        void RefreshList()
        {
            lbReminders.Items.Clear();

            List<ReminderItem> displayItems = new List<ReminderItem>();

            foreach (ReminderItem item in reminders)
                displayItems.Add(item);

            displayItems.Sort((a, b) =>
            {
                DateTime da;
                DateTime db;

                bool va = DateTime.TryParse(a.ReminderDateTime, out da);
                bool vb = DateTime.TryParse(b.ReminderDateTime, out db);

                if (va && vb)
                    return da.CompareTo(db);

                return b.ID.CompareTo(a.ID);
            });

            foreach (ReminderItem item in displayItems)
                lbReminders.Items.Add(item);

            lblStatus.Text = "Total: " + reminders.Count;
            UpdateDashboard();
        }

        void UpdateDashboard()
        {
            int total = reminders.Count;
            int completed = 0;
            int pending = 0;
            int today = 0;

            ReminderItem next = null;
            DateTime nextTime = DateTime.MaxValue;

            foreach (ReminderItem item in reminders)
            {
                if (item.Status == "Completed")
                    completed++;
                else
                    pending++;

                DateTime due;

                if (DateTime.TryParse(item.ReminderDateTime, out due))
                {
                    if (due.Date == DateTime.Today)
                        today++;

                    if (item.Status == "Pending" && due >= DateTime.Now && due < nextTime)
                    {
                        nextTime = due;
                        next = item;
                    }
                }
            }

            lblTotal.Text = total.ToString();
            lblPending.Text = pending.ToString();
            lblCompleted.Text = completed.ToString();
            lblDueToday.Text = today.ToString();

            int percent = 0;

            if (total > 0)
                percent = (completed * 100) / total;

            progressCompleted.Value = percent;

            if (next == null)
                lblNextReminder.Text = "No pending reminders";
            else
                lblNextReminder.Text = next.Title + "  |  " + next.ReminderDateTime + "  |  " + next.Priority;
        }

        DateTime BuildReminderDateTime()
        {
            DateTime date = dpDate.Value.Date;
            DateTime time = dpTime.Value;

            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
        }

        int GetSnoozeMinutes()
        {
            int amount = Convert.ToInt32(numSnooze.Value);

            if (cbSnoozeUnit.Text == "Minutes")
                return amount;

            if (cbSnoozeUnit.Text == "Hours")
                return amount * 60;

            if (cbSnoozeUnit.Text == "Days")
                return amount * 1440;

            return 20;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();

            if (title == "")
            {
                MessageBox.Show("Please enter a reminder title.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime due = BuildReminderDateTime();

            if (due < DateTime.Now.AddMinutes(-1))
            {
                MessageBox.Show("Reminder time cannot be in the past.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ReminderItem item = new ReminderItem();
            item.ID = GetNextID();
            item.Title = title;
            item.Priority = cbPriority.Text;
            item.Status = cbStatus.Text;
            item.ReminderDateTime = due.ToString("yyyy/MM/dd HH:mm");
            item.Notes = txtNotes.Text.Trim();
            item.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            reminders.Add(item);
            SaveReminders();
            RefreshList();
            ClearFields();

            MessageBox.Show("Reminder added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedReminderID == -1)
            {
                MessageBox.Show("Please select a reminder to update.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ReminderItem item = FindReminderByID(selectedReminderID);

            if (item == null)
                return;

            if (txtTitle.Text.Trim() == "")
            {
                MessageBox.Show("Reminder title is required.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime due = BuildReminderDateTime();

            item.Title = txtTitle.Text.Trim();
            item.Priority = cbPriority.Text;
            item.Status = cbStatus.Text;
            item.ReminderDateTime = due.ToString("yyyy/MM/dd HH:mm");
            item.Notes = txtNotes.Text.Trim();
            item.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            if (alreadyAlertedIDs.Contains(item.ID) && item.Status == "Pending" && due > DateTime.Now)
                alreadyAlertedIDs.Remove(item.ID);

            SaveReminders();
            RefreshList();
            ClearFields();

            MessageBox.Show("Reminder updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedReminderID == -1)
            {
                MessageBox.Show("Please select a reminder to delete.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ReminderItem item = FindReminderByID(selectedReminderID);

            if (item == null)
                return;

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to delete this reminder?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            reminders.Remove(item);

            if (alreadyAlertedIDs.Contains(item.ID))
                alreadyAlertedIDs.Remove(item.ID);

            SaveReminders();
            RefreshList();
            ClearFields();

            MessageBox.Show("Reminder deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            if (selectedReminderID == -1)
            {
                MessageBox.Show("Please select a reminder.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ReminderItem item = FindReminderByID(selectedReminderID);

            if (item == null)
                return;

            item.Status = "Completed";
            item.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            if (alreadyAlertedIDs.Contains(item.ID))
                alreadyAlertedIDs.Remove(item.ID);

            SaveReminders();
            RefreshList();
            ClearFields();

            MessageBox.Show("Reminder marked as completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSnooze_Click(object sender, EventArgs e)
        {
            if (!chkEnableSnooze.Checked)
            {
                MessageBox.Show("Enable Snooze first.", "Snooze Disabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedReminderID == -1)
            {
                MessageBox.Show("Please select a reminder to snooze.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ReminderItem item = FindReminderByID(selectedReminderID);

            if (item == null)
            {
                MessageBox.Show("This reminder was deleted.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (item.Status != "Pending")
            {
                MessageBox.Show("Completed reminders cannot be snoozed.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            item.ReminderDateTime = DateTime.Now.AddMinutes(GetSnoozeMinutes()).ToString("yyyy/MM/dd HH:mm");
            item.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            if (alreadyAlertedIDs.Contains(item.ID))
                alreadyAlertedIDs.Remove(item.ID);

            SaveReminders();
            RefreshList();
            ClearFields();

            MessageBox.Show("Reminder snoozed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim().ToLower();

            if (search == "")
            {
                RefreshList();
                return;
            }

            lbReminders.Items.Clear();

            foreach (ReminderItem item in reminders)
            {
                if (item.Title.ToLower().Contains(search) ||
                    item.Priority.ToLower().Contains(search) ||
                    item.Status.ToLower().Contains(search) ||
                    item.Notes.ToLower().Contains(search))
                {
                    lbReminders.Items.Add(item);
                }
            }

            lblStatus.Text = "Search: " + lbReminders.Items.Count;
        }

        private void CbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        void ApplyFilter()
        {
            string filter = cbFilter.Text;

            lbReminders.Items.Clear();

            foreach (ReminderItem item in reminders)
            {
                bool show = false;

                if (filter == "All")
                    show = true;
                else if (filter == "Pending" && item.Status == "Pending")
                    show = true;
                else if (filter == "Completed" && item.Status == "Completed")
                    show = true;
                else if (filter == "High" && item.Priority == "High")
                    show = true;
                else if (filter == "Today" && IsToday(item.ReminderDateTime))
                    show = true;
                else if (filter == "Overdue" && IsOverdue(item))
                    show = true;

                if (show)
                    lbReminders.Items.Add(item);
            }

            lblStatus.Text = filter + ": " + lbReminders.Items.Count;
        }

        bool IsToday(string dateText)
        {
            DateTime date;

            if (!DateTime.TryParse(dateText, out date))
                return false;

            return date.Date == DateTime.Today;
        }

        bool IsOverdue(ReminderItem item)
        {
            if (item.Status == "Completed")
                return false;

            DateTime due;

            if (!DateTime.TryParse(item.ReminderDateTime, out due))
                return false;

            return due < DateTime.Now;
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cbFilter.SelectedIndex = 0;
            RefreshList();
        }

        private void BtnToday_Click(object sender, EventArgs e)
        {
            cbFilter.Text = "Today";
            ApplyFilter();
        }

        private void LbReminders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbReminders.SelectedItem == null)
                return;

            ReminderItem item = (ReminderItem)lbReminders.SelectedItem;

            selectedReminderID = item.ID;
            txtTitle.Text = item.Title;
            cbPriority.Text = item.Priority;
            cbStatus.Text = item.Status;
            txtNotes.Text = item.Notes;

            DateTime due;

            if (DateTime.TryParse(item.ReminderDateTime, out due))
            {
                dpDate.Value = due.Date;
                dpTime.Value = due;
            }

            lblStatus.Text = "Selected ID: " + item.ID;
        }

        ReminderItem FindReminderByID(int id)
        {
            foreach (ReminderItem item in reminders)
            {
                if (item.ID == id)
                    return item;
            }

            return null;
        }

        void ClearFields()
        {
            selectedReminderID = -1;
            txtTitle.Clear();
            txtNotes.Clear();
            txtSearch.Clear();
            cbPriority.SelectedIndex = 1;
            cbStatus.SelectedIndex = 0;
            cbFilter.SelectedIndex = 0;
            chkEnableSnooze.Checked = false;
            cbSnoozeUnit.SelectedIndex = 0;
            numSnooze.Value = 20;
            dpDate.Value = DateTime.Today;
            dpTime.Value = DateTime.Now;
            lbReminders.ClearSelected();
            lblStatus.Text = "Ready";
        }

        private void BtnHowToUse_Click(object sender, EventArgs e)
        {
            string msg = "";

            msg += "Reminder Center - How To Use" + Environment.NewLine;
            msg += "----------------------------" + Environment.NewLine + Environment.NewLine;

            msg += "Add Reminder:" + Environment.NewLine;
            msg += "- Write a title." + Environment.NewLine;
            msg += "- Choose priority and status." + Environment.NewLine;
            msg += "- Choose reminder date and time." + Environment.NewLine;
            msg += "- Add notes if needed." + Environment.NewLine;
            msg += "- Click Add." + Environment.NewLine + Environment.NewLine;

            msg += "Snooze from Alert:" + Environment.NewLine;
            msg += "- When a reminder alert appears, choose the number." + Environment.NewLine;
            msg += "- Choose Minutes, Hours, or Days." + Environment.NewLine;
            msg += "- Click Snooze Later." + Environment.NewLine + Environment.NewLine;

            msg += "Snooze Selected:" + Environment.NewLine;
            msg += "- Check Enable Snooze first." + Environment.NewLine;
            msg += "- Choose the number and unit." + Environment.NewLine;
            msg += "- Select a Pending reminder." + Environment.NewLine;
            msg += "- Click Snooze Selected." + Environment.NewLine;
            msg += "- Completed or deleted reminders cannot be snoozed." + Environment.NewLine + Environment.NewLine;

            msg += "Reminder Alerts:" + Environment.NewLine;
            msg += "- Alerts work while the app is running." + Environment.NewLine;
            msg += "- If you click X, the app hides to the tray and reminders keep working." + Environment.NewLine;
            msg += "- If you choose Exit App or tray menu > Exit, the app closes fully and reminders stop." + Environment.NewLine + Environment.NewLine;

            msg += "Tray Icon:" + Environment.NewLine;
            msg += "- Double-click the tray icon to open the app again." + Environment.NewLine;
            msg += "- Right-click the tray icon to choose Open or Exit." + Environment.NewLine + Environment.NewLine;

            msg += "Reset Data:" + Environment.NewLine;
            msg += "- Reset Data deletes all reminders." + Environment.NewLine;
            msg += "- It requires two confirmations." + Environment.NewLine + Environment.NewLine;

            msg += "Data path:" + Environment.NewLine;
            msg += dataFile;

            MessageBox.Show(msg, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            DialogResult firstConfirm = MessageBox.Show(
                "Warning: This will delete all saved reminders." + Environment.NewLine + Environment.NewLine +
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

            reminders.Clear();
            alreadyAlertedIDs.Clear();
            SaveReminders();
            RefreshList();
            ClearFields();

            MessageBox.Show("All reminders were deleted successfully.", "Reset Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            lbl.Text = "Final confirmation:" + Environment.NewLine + "Type RESET exactly to delete all reminders.";
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

        private void ReminderCenterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allowExit)
            {
                e.Cancel = true;
                this.Hide();

                if (!firstHideMessageShown)
                {
                    firstHideMessageShown = true;
                    trayIcon.ShowBalloonTip(
                        4000,
                        "Reminder Center is still running",
                        "The app is hidden in the tray. Reminders will still work.",
                        ToolTipIcon.Info
                    );
                }
            }
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowApp();
        }

        private void OpenItem_Click(object sender, EventArgs e)
        {
            ShowApp();
        }

        void ShowApp()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }

        void ExitApplication()
        {
            DialogResult confirm = MessageBox.Show(
                "Exit Reminder Center completely?" + Environment.NewLine +
                "Reminders will stop until you open the app again.",
                "Exit App",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            allowExit = true;

            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            Application.Exit();
        }

        public class ReminderItem
        {
            public int ID;
            public string Title;
            public string Priority;
            public string Status;
            public string ReminderDateTime;
            public string Notes;
            public string UpdatedDate;

            public override string ToString()
            {
                string flag = "";

                if (Status == "Completed")
                    flag = "[DONE] ";
                else if (Priority == "High")
                    flag = "[HIGH] ";
                else
                {
                    DateTime due;

                    if (DateTime.TryParse(ReminderDateTime, out due) && due < DateTime.Now && Status == "Pending")
                        flag = "[OVERDUE] ";
                }

                return flag + ID + ": " + Title + " | " + Priority + " | " + Status + " | " + ReminderDateTime;
            }
        }
    }
}