using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace TaskManagerApp
{
    public static class AppData
    {
        public static string UserName = Environment.UserName;
        public static string FolderName = UserName + "_TaskManager_Database";
        public static string DbName = "Tasks.db";

        public static string DbFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            FolderName
        );

        public static string DbPath = Path.Combine(DbFolder, DbName);
        public static string ConnString = "Data Source=" + DbPath + ";Version=3;";

        public static void PrepareDatabase()
        {
            try
            {
                if (!Directory.Exists(DbFolder))
                    Directory.CreateDirectory(DbFolder);

                if (!File.Exists(DbPath))
                    SQLiteConnection.CreateFile(DbPath);

                CreateTablesIfMissing();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Database setup error: " + ex.Message + Environment.NewLine + Environment.NewLine +
                    "Database path:" + Environment.NewLine + DbPath,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        static void CreateTablesIfMissing()
        {
            using (SQLiteConnection cn = new SQLiteConnection(ConnString))
            {
                cn.Open();

                string dailySql =
                    "CREATE TABLE IF NOT EXISTS DTasks (" +
                    "ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Name TEXT, " +
                    "Priority TEXT, " +
                    "Date TEXT, " +
                    "Notes TEXT, " +
                    "Status TEXT" +
                    ")";

                string weeklySql =
                    "CREATE TABLE IF NOT EXISTS Tasks (" +
                    "ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Name TEXT, " +
                    "Priority TEXT, " +
                    "Date TEXT, " +
                    "Notes TEXT, " +
                    "Status TEXT" +
                    ")";

                string settingsSql =
                    "CREATE TABLE IF NOT EXISTS AppSettings (" +
                    "SettingKey TEXT PRIMARY KEY, " +
                    "SettingValue TEXT" +
                    ")";

                new SQLiteCommand(dailySql, cn).ExecuteNonQuery();
                new SQLiteCommand(weeklySql, cn).ExecuteNonQuery();
                new SQLiteCommand(settingsSql, cn).ExecuteNonQuery();
            }
        }

        public static string GetSetting(string key)
        {
            using (SQLiteConnection cn = new SQLiteConnection(ConnString))
            {
                cn.Open();

                SQLiteCommand cmd = new SQLiteCommand("SELECT SettingValue FROM AppSettings WHERE SettingKey=@Key", cn);
                cmd.Parameters.AddWithValue("@Key", key);

                object result = cmd.ExecuteScalar();

                if (result == null)
                    return "";

                return result.ToString();
            }
        }

        public static void SetSetting(string key, string value)
        {
            using (SQLiteConnection cn = new SQLiteConnection(ConnString))
            {
                cn.Open();

                SQLiteCommand cmd = new SQLiteCommand(
                    "INSERT OR REPLACE INTO AppSettings (SettingKey, SettingValue) VALUES (@Key, @Value)",
                    cn
                );

                cmd.Parameters.AddWithValue("@Key", key);
                cmd.Parameters.AddWithValue("@Value", value);
                cmd.ExecuteNonQuery();
            }
        }

        public static void ResetDatabase()
        {
            using (SQLiteConnection cn = new SQLiteConnection(ConnString))
            {
                cn.Open();

                new SQLiteCommand("DELETE FROM DTasks", cn).ExecuteNonQuery();
                new SQLiteCommand("DELETE FROM Tasks", cn).ExecuteNonQuery();
                new SQLiteCommand("DELETE FROM AppSettings", cn).ExecuteNonQuery();

                new SQLiteCommand("DELETE FROM sqlite_sequence WHERE name='DTasks'", cn).ExecuteNonQuery();
                new SQLiteCommand("DELETE FROM sqlite_sequence WHERE name='Tasks'", cn).ExecuteNonQuery();
            }
        }
    }

    public static class AppSecurity
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                    sb.Append(hash[i].ToString("x2"));

                return sb.ToString();
            }
        }

        public static bool HasDeletedPassword()
        {
            return AppData.GetSetting("DeletedPasswordHash") != "";
        }

        public static bool CheckDeletedPassword(string password)
        {
            string savedHash = AppData.GetSetting("DeletedPasswordHash");

            if (savedHash == "")
                return true;

            return savedHash == HashPassword(password);
        }

        public static void SetDeletedPassword(string password)
        {
            AppData.SetSetting("DeletedPasswordHash", HashPassword(password));
        }

        public static void RemoveDeletedPassword()
        {
            AppData.SetSetting("DeletedPasswordHash", "");
        }
    }

    public static class AppStyle
    {
        public static Color BackColor = Color.FromArgb(245, 247, 250);
        public static Color CardColor = Color.White;
        public static Color Primary = Color.FromArgb(35, 97, 146);
        public static Color Secondary = Color.FromArgb(44, 62, 80);
        public static Color Success = Color.FromArgb(39, 174, 96);
        public static Color Danger = Color.FromArgb(192, 57, 43);
        public static Color Warning = Color.FromArgb(243, 156, 18);
        public static Color TextColor = Color.FromArgb(35, 35, 35);

        public static Font TitleFont = new Font("Segoe UI", 20, FontStyle.Bold);
        public static Font HeaderFont = new Font("Segoe UI", 14, FontStyle.Bold);
        public static Font NormalFont = new Font("Segoe UI", 10, FontStyle.Regular);
        public static Font ButtonFont = new Font("Segoe UI", 10, FontStyle.Bold);

        public static void ApplyForm(Form form, string title, int width, int height)
        {
            form.Text = title;
            form.Size = new Size(width, height);
            form.StartPosition = FormStartPosition.CenterScreen;
            form.BackColor = BackColor;
            form.Font = NormalFont;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
        }

        public static Label Label(string text, int x, int y, int w, int h, Font font)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(w, h);
            lbl.Font = font;
            lbl.ForeColor = TextColor;
            lbl.BackColor = Color.Transparent;
            return lbl;
        }

        public static Button Button(string text, int x, int y, int w, int h, Color color)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(w, h);
            btn.Font = ButtonFont;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        public static TextBox TextBox(int x, int y, int w, int h)
        {
            TextBox tb = new TextBox();
            tb.Location = new Point(x, y);
            tb.Size = new Size(w, h);
            tb.Font = NormalFont;
            return tb;
        }

        public static Panel Card(int x, int y, int w, int h)
        {
            Panel p = new Panel();
            p.Location = new Point(x, y);
            p.Size = new Size(w, h);
            p.BackColor = CardColor;
            p.BorderStyle = BorderStyle.FixedSingle;
            return p;
        }
    }

    public static class AppTray
    {
        static NotifyIcon trayIcon;
        static ContextMenuStrip trayMenu;
        static bool realExit = false;

        public static void Initialize()
        {
            trayMenu = new ContextMenuStrip();

            ToolStripMenuItem openItem = new ToolStripMenuItem("Open Task Manager");
            openItem.Click += OpenItem_Click;

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += ExitItem_Click;

            trayMenu.Items.Add(openItem);
            trayMenu.Items.Add(exitItem);

            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Text = "Task Manager";
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
        }

        public static void Register(Form form)
        {
            form.FormClosing += Form_FormClosing;
        }

        static void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!realExit)
            {
                e.Cancel = true;

                Form form = sender as Form;

                if (form != null)
                    form.Hide();

                ShowBalloon("Task Manager is still running", "The app is hidden in the system tray. Reminders will still work.");
            }
        }

        static void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowHomePage();
        }

        static void OpenItem_Click(object sender, EventArgs e)
        {
            ShowHomePage();
        }

        static void ExitItem_Click(object sender, EventArgs e)
        {
            ExitApp();
        }

        public static void ExitApp()
        {
            realExit = true;

            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            Application.Exit();
        }

        public static void ShowHomePage()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is HomeForm)
                {
                    form.Show();
                    form.WindowState = FormWindowState.Normal;
                    form.Activate();
                    return;
                }
            }

            HomeForm main = new HomeForm();
            main.Show();
        }

        public static void ShowBalloon(string title, string message)
        {
            if (trayIcon != null)
                trayIcon.ShowBalloonTip(4000, title, message, ToolTipIcon.Info);
        }
    }

    public class ReminderItem
    {
        public int TaskID;
        public string Title;
        public string Type;
        public DateTime Time;
        public bool Done;

        public ReminderItem(int taskID, string title, string type, DateTime time)
        {
            TaskID = taskID;
            Title = title;
            Type = type;
            Time = time;
            Done = false;
        }
    }

    public static class ReminderHelper
    {
        public static void FillUnits(ComboBox combo)
        {
            combo.Items.Clear();
            combo.Items.Add("Minutes");
            combo.Items.Add("Hours");
            combo.Items.Add("Days");
            combo.SelectedIndex = 0;
        }

        public static DateTime GetReminderTime(decimal amount, string unit)
        {
            int value = Convert.ToInt32(amount);

            if (unit == "Minutes")
                return DateTime.Now.AddMinutes(value);

            if (unit == "Hours")
                return DateTime.Now.AddHours(value);

            if (unit == "Days")
                return DateTime.Now.AddDays(value);

            return DateTime.MinValue;
        }
    }

    public static class ReminderService
    {
        static List<ReminderItem> reminders = new List<ReminderItem>();
        static Timer timer = new Timer();
        static bool started = false;

        public static void Start()
        {
            if (started)
                return;

            started = true;
            timer.Interval = 30000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public static void AddReminder(int taskID, string title, string type, DateTime time)
        {
            if (time > DateTime.Now)
            {
                RemoveReminder(taskID, type);
                reminders.Add(new ReminderItem(taskID, title, type, time));
            }
        }

        public static void RemoveReminder(int taskID, string type)
        {
            for (int i = reminders.Count - 1; i >= 0; i--)
            {
                if (reminders[i].TaskID == taskID && reminders[i].Type == type)
                    reminders.RemoveAt(i);
            }
        }

        public static void ClearAll()
        {
            reminders.Clear();
        }

        static void Timer_Tick(object sender, EventArgs e)
        {
            for (int i = reminders.Count - 1; i >= 0; i--)
            {
                ReminderItem r = reminders[i];

                if (!r.Done && DateTime.Now >= r.Time)
                {
                    r.Done = true;

                    AppTray.ShowBalloon("Task Reminder", r.Title + " - " + r.Type);

                    ReminderPopupForm popup = new ReminderPopupForm(r.Title, r.Type, r.Time);
                    DialogResult result = popup.ShowDialog();

                    if (result == DialogResult.Retry)
                    {
                        r.Time = ReminderHelper.GetReminderTime(popup.SnoozeAmount, popup.SnoozeUnit);
                        r.Done = false;
                    }
                    else
                    {
                        RemoveReminder(r.TaskID, r.Type);
                    }
                }
            }
        }
    }

    public class ReminderPopupForm : Form
    {
        Label lblTitle;
        Label lblInfo;
        Button btnOk;
        Button btnLater;
        Button btnDismiss;
        NumericUpDown numLater;
        ComboBox cbLaterUnit;

        public decimal SnoozeAmount;
        public string SnoozeUnit;

        public ReminderPopupForm(string title, string type, DateTime time)
        {
            AppStyle.ApplyForm(this, "Task Reminder", 520, 330);
            BuildUI(title, type, time);
        }

        void BuildUI(string taskTitle, string type, DateTime time)
        {
            lblTitle = AppStyle.Label("Task Reminder", 30, 25, 450, 35, AppStyle.HeaderFont);
            Controls.Add(lblTitle);

            lblInfo = AppStyle.Label(
                "Task: " + taskTitle + Environment.NewLine +
                "Type: " + type + Environment.NewLine +
                "Reminder Time: " + time.ToString("g") + Environment.NewLine + Environment.NewLine +
                "Choose OK to close, Dismiss to ignore, or Remind Me Later with your own time.",
                30,
                75,
                450,
                95,
                AppStyle.NormalFont
            );
            Controls.Add(lblInfo);

            Label lblLater = AppStyle.Label("Remind me later after:", 30, 180, 180, 25, AppStyle.NormalFont);
            Controls.Add(lblLater);

            numLater = new NumericUpDown();
            numLater.Location = new Point(210, 180);
            numLater.Size = new Size(70, 25);
            numLater.Minimum = 1;
            numLater.Maximum = 999;
            numLater.Value = 5;
            Controls.Add(numLater);

            cbLaterUnit = new ComboBox();
            cbLaterUnit.Location = new Point(290, 180);
            cbLaterUnit.Size = new Size(120, 25);
            cbLaterUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            ReminderHelper.FillUnits(cbLaterUnit);
            Controls.Add(cbLaterUnit);

            btnOk = AppStyle.Button("OK", 30, 235, 120, 35, AppStyle.Success);
            btnOk.Click += BtnOk_Click;
            Controls.Add(btnOk);

            btnLater = AppStyle.Button("Remind Me Later", 165, 235, 170, 35, AppStyle.Warning);
            btnLater.Click += BtnLater_Click;
            Controls.Add(btnLater);

            btnDismiss = AppStyle.Button("Dismiss", 350, 235, 120, 35, AppStyle.Danger);
            btnDismiss.Click += BtnDismiss_Click;
            Controls.Add(btnDismiss);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnLater_Click(object sender, EventArgs e)
        {
            SnoozeAmount = numLater.Value;
            SnoozeUnit = cbLaterUnit.Text;
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        private void BtnDismiss_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

    public class DeletedTasksForm : Form
    {
        ListBox lbDeleted;

        Button btnRestore;
        Button btnDeleteForever;
        Button btnSelectAll;
        Button btnClearSelection;
        Button btnSetPassword;
        Button btnRemovePassword;
        Button btnBack;
        Button btnHelp;

        Label title;
        Label lblInfo;

        Form previousPage;

        public DeletedTasksForm(Form previous)
        {
            previousPage = previous;
            AppStyle.ApplyForm(this, "Deleted Tasks", 930, 690);
            AppTray.Register(this);
            BuildUI();
            LoadDeletedTasks();
            UpdatePasswordButtonText();
        }

        void BuildUI()
        {
            title = AppStyle.Label("Deleted Tasks", 30, 20, 450, 40, AppStyle.TitleFont);
            Controls.Add(title);

            btnBack = AppStyle.Button("Back", 720, 25, 150, 35, AppStyle.Secondary);
            btnBack.Click += BtnBack_Click;
            Controls.Add(btnBack);

            btnHelp = AppStyle.Button("How To Use", 720, 70, 150, 35, AppStyle.Warning);
            btnHelp.Click += BtnHelp_Click;
            Controls.Add(btnHelp);

            btnSetPassword = AppStyle.Button("Set Password", 30, 85, 150, 35, AppStyle.Primary);
            btnSetPassword.Click += BtnSetPassword_Click;
            Controls.Add(btnSetPassword);

            btnRemovePassword = AppStyle.Button("Remove Password", 190, 85, 170, 35, AppStyle.Danger);
            btnRemovePassword.Click += BtnRemovePassword_Click;
            Controls.Add(btnRemovePassword);

            btnSelectAll = AppStyle.Button("Select All", 380, 85, 130, 35, AppStyle.Success);
            btnSelectAll.Click += BtnSelectAll_Click;
            Controls.Add(btnSelectAll);

            btnClearSelection = AppStyle.Button("Clear Selection", 520, 85, 160, 35, AppStyle.Secondary);
            btnClearSelection.Click += BtnClearSelection_Click;
            Controls.Add(btnClearSelection);

            lblInfo = AppStyle.Label("Tip: Hold Ctrl to select more than one task, or click Select All.", 30, 125, 650, 25, AppStyle.NormalFont);
            Controls.Add(lblInfo);

            lbDeleted = new ListBox();
            lbDeleted.Location = new Point(30, 160);
            lbDeleted.Size = new Size(840, 350);
            lbDeleted.Font = AppStyle.NormalFont;
            lbDeleted.SelectionMode = SelectionMode.MultiExtended;
            Controls.Add(lbDeleted);

            btnRestore = AppStyle.Button("Restore Selected", 30, 535, 180, 40, AppStyle.Success);
            btnRestore.Click += BtnRestore_Click;
            Controls.Add(btnRestore);

            btnDeleteForever = AppStyle.Button("Delete Selected Permanently", 230, 535, 260, 40, AppStyle.Danger);
            btnDeleteForever.Click += BtnDeleteForever_Click;
            Controls.Add(btnDeleteForever);
        }

        void UpdatePasswordButtonText()
        {
            if (AppSecurity.HasDeletedPassword())
                btnSetPassword.Text = "Change Password";
            else
                btnSetPassword.Text = "Set Password";
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            string msg = "";

            msg += "Deleted Tasks - How To Use" + Environment.NewLine;
            msg += "--------------------------" + Environment.NewLine + Environment.NewLine;
            msg += "This page shows deleted Daily Tasks and Weekly Tasks together." + Environment.NewLine;
            msg += "Each task shows its type: Daily Task or Weekly Task." + Environment.NewLine + Environment.NewLine;
            msg += "Select tasks:" + Environment.NewLine;
            msg += "- Click one task to select it." + Environment.NewLine;
            msg += "- Hold Ctrl and click to select more than one task." + Environment.NewLine;
            msg += "- Select All selects all deleted tasks." + Environment.NewLine + Environment.NewLine;
            msg += "Restore Selected:" + Environment.NewLine;
            msg += "- Restores selected tasks to Pending." + Environment.NewLine + Environment.NewLine;
            msg += "Delete Selected Permanently:" + Environment.NewLine;
            msg += "- Permanently deletes selected tasks." + Environment.NewLine;
            msg += "- This action cannot be undone." + Environment.NewLine + Environment.NewLine;
            msg += "Password:" + Environment.NewLine;
            msg += "- Set Password protects this page." + Environment.NewLine;
            msg += "- Change Password appears when a password already exists." + Environment.NewLine;
            msg += "- Password is stored as a hash, not plain text." + Environment.NewLine + Environment.NewLine;
            msg += "Database path:" + Environment.NewLine;
            msg += AppData.DbPath;

            MessageBox.Show(msg, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            previousPage.Show();
            this.Hide();
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbDeleted.Items.Count; i++)
                lbDeleted.SetSelected(i, true);
        }

        private void BtnClearSelection_Click(object sender, EventArgs e)
        {
            lbDeleted.ClearSelected();
        }

        private string AskPassword(string title, string labelText)
        {
            Form f = new Form();
            AppStyle.ApplyForm(f, title, 440, 185);

            Label lbl = AppStyle.Label(labelText, 25, 25, 380, 25, AppStyle.NormalFont);

            TextBox tb = AppStyle.TextBox(25, 55, 380, 25);
            tb.PasswordChar = '*';

            Button ok = AppStyle.Button("OK", 95, 105, 100, 30, AppStyle.Success);
            Button cancel = AppStyle.Button("Cancel", 210, 105, 100, 30, AppStyle.Danger);

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

            f.Controls.Add(lbl);
            f.Controls.Add(tb);
            f.Controls.Add(ok);
            f.Controls.Add(cancel);

            DialogResult result = f.ShowDialog();

            if (result == DialogResult.OK)
                return value;

            return "";
        }

        public static bool CanOpenDeletedTasks()
        {
            if (!AppSecurity.HasDeletedPassword())
                return true;

            Form f = new Form();
            AppStyle.ApplyForm(f, "Deleted Tasks Password", 430, 190);

            Label lbl = AppStyle.Label("Enter Deleted Tasks password:", 25, 25, 360, 25, AppStyle.NormalFont);

            TextBox tb = AppStyle.TextBox(25, 60, 360, 25);
            tb.PasswordChar = '*';

            Button ok = AppStyle.Button("Open", 90, 110, 100, 30, AppStyle.Success);
            Button cancel = AppStyle.Button("Cancel", 205, 110, 100, 30, AppStyle.Danger);

            bool allowed = false;

            ok.Click += (s, e) =>
            {
                if (AppSecurity.CheckDeletedPassword(tb.Text))
                {
                    allowed = true;
                    f.Close();
                }
                else
                {
                    MessageBox.Show("Wrong password.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            cancel.Click += (s, e) =>
            {
                allowed = false;
                f.Close();
            };

            f.Controls.Add(lbl);
            f.Controls.Add(tb);
            f.Controls.Add(ok);
            f.Controls.Add(cancel);

            f.ShowDialog();

            return allowed;
        }

        private void BtnSetPassword_Click(object sender, EventArgs e)
        {
            if (AppSecurity.HasDeletedPassword())
            {
                string oldPass = AskPassword("Change Password", "Enter current password:");

                if (oldPass == "" || !AppSecurity.CheckDeletedPassword(oldPass))
                {
                    MessageBox.Show("Wrong current password.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string newPass = AskPassword("Change Password", "Enter new password:");

                if (newPass == "")
                    return;

                if (newPass.Length < 4)
                {
                    MessageBox.Show("New password must be at least 4 characters.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppSecurity.SetDeletedPassword(newPass);
                UpdatePasswordButtonText();
                MessageBox.Show("Password changed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string newPass = AskPassword("Set Password", "Enter new password:");

                if (newPass == "")
                    return;

                if (newPass.Length < 4)
                {
                    MessageBox.Show("Password must be at least 4 characters.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppSecurity.SetDeletedPassword(newPass);
                UpdatePasswordButtonText();
                MessageBox.Show("Password set successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRemovePassword_Click(object sender, EventArgs e)
        {
            if (!AppSecurity.HasDeletedPassword())
            {
                MessageBox.Show("No password is currently set.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string pass = AskPassword("Remove Password", "Enter current password:");

            if (pass == "" || !AppSecurity.CheckDeletedPassword(pass))
            {
                MessageBox.Show("Wrong password.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppSecurity.RemoveDeletedPassword();
            UpdatePasswordButtonText();

            MessageBox.Show("Password removed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void LoadDeletedTasks()
        {
            lbDeleted.Items.Clear();

            LoadDeletedFromTable("DTasks", "Daily Task");
            LoadDeletedFromTable("Tasks", "Weekly Task");
        }

        void LoadDeletedFromTable(string table, string type)
        {
            using (SQLiteConnection cn = new SQLiteConnection(AppData.ConnString))
            {
                try
                {
                    cn.Open();

                    SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + table + " WHERE Status='Deleted'", cn);
                    SQLiteDataReader r = cmd.ExecuteReader();

                    while (r.Read())
                    {
                        DeletedTask t = new DeletedTask(
                            Convert.ToInt32(r["ID"]),
                            type,
                            table,
                            r["Name"].ToString(),
                            r["Priority"].ToString(),
                            r["Date"].ToString(),
                            r["Notes"].ToString(),
                            r["Status"].ToString()
                        );

                        lbDeleted.Items.Add(t);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading deleted tasks: " + ex.Message);
                }
            }
        }

        private List<DeletedTask> GetSelectedTasks()
        {
            List<DeletedTask> selectedTasks = new List<DeletedTask>();

            foreach (object item in lbDeleted.SelectedItems)
            {
                DeletedTask task = item as DeletedTask;

                if (task != null)
                    selectedTasks.Add(task);
            }

            return selectedTasks;
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            List<DeletedTask> selectedTasks = GetSelectedTasks();

            if (selectedTasks.Count == 0)
            {
                MessageBox.Show("Please select at least one task.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Restore selected task(s) to Pending?",
                "Confirm Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            using (SQLiteConnection cn = new SQLiteConnection(AppData.ConnString))
            {
                try
                {
                    cn.Open();

                    foreach (DeletedTask selected in selectedTasks)
                    {
                        SQLiteCommand cmd = new SQLiteCommand("UPDATE " + selected.TableName + " SET Status='Pending' WHERE ID=@ID", cn);
                        cmd.Parameters.AddWithValue("@ID", selected.ID);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Selected task(s) restored to Pending.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDeletedTasks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error restoring task(s): " + ex.Message);
                }
            }
        }

        private void BtnDeleteForever_Click(object sender, EventArgs e)
        {
            List<DeletedTask> selectedTasks = GetSelectedTasks();

            if (selectedTasks.Count == 0)
            {
                MessageBox.Show("Please select at least one task.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "This will permanently delete the selected task(s). Continue?",
                "Permanent Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes)
                return;

            using (SQLiteConnection cn = new SQLiteConnection(AppData.ConnString))
            {
                try
                {
                    cn.Open();

                    foreach (DeletedTask selected in selectedTasks)
                    {
                        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM " + selected.TableName + " WHERE ID=@ID", cn);
                        cmd.Parameters.AddWithValue("@ID", selected.ID);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Selected task(s) permanently deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDeletedTasks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting task(s) permanently: " + ex.Message);
                }
            }
        }

        public class DeletedTask
        {
            public int ID;
            public string Type;
            public string TableName;
            public string Name;
            public string Priority;
            public string Date;
            public string Notes;
            public string Status;

            public DeletedTask(int id, string type, string tableName, string name, string priority, string date, string notes, string status)
            {
                ID = id;
                Type = type;
                TableName = tableName;
                Name = name;
                Priority = priority;
                Date = date;
                Notes = notes;
                Status = status;
            }

            public override string ToString()
            {
                return string.Format("{0} | {1}: {2} | {3} | {4} | {5} | Notes: {6}", Type, ID, Name, Priority, Date, Status, Notes);
            }
        }
    }

    public class HomeForm : Form
    {
        Label title;
        Label intro;
        Button btnDaily;
        Button btnWeekly;
        Button btnReport;
        Button btnDeleted;
        Button btnResetDatabase;
        Button btnExit;

        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppData.PrepareDatabase();
            ReminderService.Start();
            AppTray.Initialize();

            Application.Run(new HomeForm());
        }

        public HomeForm()
        {
            AppStyle.ApplyForm(this, "Task Manager - Home", 700, 570);
            AppTray.Register(this);
            BuildUI();
        }

        void BuildUI()
        {
            btnResetDatabase = AppStyle.Button("Reset DB", 25, 5, 85, 22, AppStyle.Danger);
            btnResetDatabase.Font = new Font("Segoe UI", 7, FontStyle.Bold);
            btnResetDatabase.Click += BtnResetDatabase_Click;
            Controls.Add(btnResetDatabase);

            Panel card = AppStyle.Card(70, 45, 540, 430);
            Controls.Add(card);

            title = AppStyle.Label("Task Manager", 0, 35, 540, 45, AppStyle.TitleFont);
            title.TextAlign = ContentAlignment.MiddleCenter;
            card.Controls.Add(title);

            intro = AppStyle.Label("Organize your daily and weekly tasks in one clean place.", 0, 85, 540, 30, AppStyle.NormalFont);
            intro.TextAlign = ContentAlignment.MiddleCenter;
            card.Controls.Add(intro);

            btnDaily = AppStyle.Button("Daily Tasks", 150, 140, 240, 42, AppStyle.Primary);
            btnDaily.Click += BtnDaily_Click;
            card.Controls.Add(btnDaily);

            btnWeekly = AppStyle.Button("Weekly Tasks", 150, 190, 240, 42, AppStyle.Primary);
            btnWeekly.Click += BtnWeekly_Click;
            card.Controls.Add(btnWeekly);

            btnReport = AppStyle.Button("General Report", 150, 240, 240, 42, AppStyle.Secondary);
            btnReport.Click += BtnReport_Click;
            card.Controls.Add(btnReport);

            btnDeleted = AppStyle.Button("Deleted Tasks", 150, 290, 240, 42, AppStyle.Warning);
            btnDeleted.Click += BtnDeleted_Click;
            card.Controls.Add(btnDeleted);

            btnExit = AppStyle.Button("Exit", 150, 340, 240, 42, AppStyle.Danger);
            btnExit.Click += BtnExit_Click;
            card.Controls.Add(btnExit);

            Label developedBy = AppStyle.Label("Developed by Mohammed Ahmed Alhijab", 0, 500, 700, 25, new Font("Segoe UI", 9, FontStyle.Italic));
            developedBy.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(developedBy);
        }

        private void BtnDaily_Click(object sender, EventArgs e)
        {
            DailyTasksForm dailyPage = new DailyTasksForm();
            dailyPage.Show();
            this.Hide();
        }

        private void BtnWeekly_Click(object sender, EventArgs e)
        {
            WeeklyTasksForm weeklyPage = new WeeklyTasksForm();
            weeklyPage.Show();
            this.Hide();
        }

        private void BtnReport_Click(object sender, EventArgs e)
        {
            ReportsForm reportPage = new ReportsForm(this);
            reportPage.Show();
            this.Hide();
        }

        private void BtnDeleted_Click(object sender, EventArgs e)
        {
            if (!DeletedTasksForm.CanOpenDeletedTasks())
                return;

            DeletedTasksForm deletedPage = new DeletedTasksForm(this);
            deletedPage.Show();
            this.Hide();
        }

        private void BtnResetDatabase_Click(object sender, EventArgs e)
        {
            DialogResult firstConfirm = MessageBox.Show(
                "Warning: This will delete all data in the database." + Environment.NewLine + Environment.NewLine +
                "This includes:" + Environment.NewLine +
                "- Daily Tasks" + Environment.NewLine +
                "- Weekly Tasks" + Environment.NewLine +
                "- Deleted Tasks" + Environment.NewLine +
                "- Deleted Tasks password" + Environment.NewLine +
                "- Current reminders" + Environment.NewLine + Environment.NewLine +
                "Do you want to continue?",
                "Reset Database - First Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (firstConfirm != DialogResult.Yes)
                return;

            string typedText = AskResetConfirmation();

            if (typedText != "RESET")
            {
                MessageBox.Show(
                    "Reset cancelled. You must type RESET exactly to confirm.",
                    "Cancelled",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            try
            {
                AppData.ResetDatabase();
                ReminderService.ClearAll();

                MessageBox.Show(
                    "Database reset successfully.",
                    "Reset Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error resetting database: " + ex.Message,
                    "Reset Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private string AskResetConfirmation()
        {
            Form f = new Form();
            AppStyle.ApplyForm(f, "Final Reset Confirmation", 470, 230);

            Label lbl = AppStyle.Label(
                "Final confirmation:" + Environment.NewLine +
                "Type RESET exactly to delete all database data.",
                25,
                25,
                400,
                55,
                AppStyle.NormalFont
            );

            TextBox tb = AppStyle.TextBox(25, 90, 400, 25);

            Button ok = AppStyle.Button("Confirm Reset", 75, 140, 140, 35, AppStyle.Danger);
            Button cancel = AppStyle.Button("Cancel", 230, 140, 120, 35, AppStyle.Secondary);

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

            f.Controls.Add(lbl);
            f.Controls.Add(tb);
            f.Controls.Add(ok);
            f.Controls.Add(cancel);

            DialogResult result = f.ShowDialog();

            if (result == DialogResult.OK)
                return value;

            return "";
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            AppTray.ExitApp();
        }
    }
}