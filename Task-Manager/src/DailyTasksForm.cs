using System;
using System.Windows.Forms;
using System.Drawing;
using System.Data.SQLite;
using System.Collections.Generic;

namespace TaskManagerApp
{
    public class DailyTasksForm : Form
    {
        TextBox txtName;
        ComboBox cbPriority;
        DateTimePicker dpDate;
        TextBox txtNotes;
        TextBox txtSearch;
        ListBox lbTasks;

        Button btnMain;
        Button btnAdd;
        Button btnSearch;
        Button btnComplete;
        Button btnDelete;
        Button btnWeekly;
        Button btnHelp;
        Button btnShowPending;
        Button btnShowCompleted;
        Button btnShowAll;

        Label lblReminder;
        CheckBox chkReminder;
        NumericUpDown numReminder;
        ComboBox cbReminderUnit;

        List<DailyTask> allTasks = new List<DailyTask>();
        string currentFilter = "Pending";

        public DailyTasksForm()
        {
            AppStyle.ApplyForm(this, "Daily Task Manager", 860, 720);
            AppTray.Register(this);
            BuildUI();
            LoadDailyTasks("Pending");
        }

        void BuildUI()
        {
            Label title = AppStyle.Label("Daily Task Manager", 30, 20, 430, 40, AppStyle.TitleFont);
            Controls.Add(title);

            btnMain = AppStyle.Button("Main Page", 660, 25, 150, 35, AppStyle.Secondary);
            btnMain.Click += BtnMain_Click;
            Controls.Add(btnMain);

            btnWeekly = AppStyle.Button("Weekly Tasks", 660, 70, 150, 35, AppStyle.Primary);
            btnWeekly.Click += BtnWeekly_Click;
            Controls.Add(btnWeekly);

            btnHelp = AppStyle.Button("How To Use", 660, 115, 150, 35, AppStyle.Warning);
            btnHelp.Click += BtnHelp_Click;
            Controls.Add(btnHelp);

            Panel formCard = AppStyle.Card(30, 80, 590, 330);
            Controls.Add(formCard);

            formCard.Controls.Add(AppStyle.Label("Task Name:", 20, 25, 120, 25, AppStyle.NormalFont));
            txtName = AppStyle.TextBox(150, 25, 360, 25);
            formCard.Controls.Add(txtName);

            formCard.Controls.Add(AppStyle.Label("Priority:", 20, 65, 120, 25, AppStyle.NormalFont));
            cbPriority = new ComboBox();
            cbPriority.Location = new Point(150, 65);
            cbPriority.Size = new Size(160, 25);
            cbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPriority.Items.Add("High");
            cbPriority.Items.Add("Medium");
            cbPriority.Items.Add("Low");
            cbPriority.SelectedIndex = 1;
            formCard.Controls.Add(cbPriority);

            formCard.Controls.Add(AppStyle.Label("Date:", 20, 105, 120, 25, AppStyle.NormalFont));
            dpDate = new DateTimePicker();
            dpDate.Location = new Point(150, 105);
            dpDate.Size = new Size(160, 25);
            dpDate.Format = DateTimePickerFormat.Short;
            formCard.Controls.Add(dpDate);

            formCard.Controls.Add(AppStyle.Label("Notes:", 20, 145, 120, 25, AppStyle.NormalFont));
            txtNotes = AppStyle.TextBox(150, 145, 360, 55);
            txtNotes.Multiline = true;
            formCard.Controls.Add(txtNotes);

            chkReminder = new CheckBox();
            chkReminder.Text = "Set reminder";
            chkReminder.Location = new Point(150, 220);
            chkReminder.Size = new Size(130, 25);
            chkReminder.Font = AppStyle.NormalFont;
            chkReminder.CheckedChanged += ChkReminder_CheckedChanged;
            formCard.Controls.Add(chkReminder);

            lblReminder = AppStyle.Label("Remind after:", 20, 260, 120, 25, AppStyle.NormalFont);
            formCard.Controls.Add(lblReminder);

            numReminder = new NumericUpDown();
            numReminder.Location = new Point(150, 260);
            numReminder.Size = new Size(70, 25);
            numReminder.Minimum = 1;
            numReminder.Maximum = 999;
            numReminder.Value = 5;
            formCard.Controls.Add(numReminder);

            cbReminderUnit = new ComboBox();
            cbReminderUnit.Location = new Point(230, 260);
            cbReminderUnit.Size = new Size(120, 25);
            cbReminderUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            ReminderHelper.FillUnits(cbReminderUnit);
            formCard.Controls.Add(cbReminderUnit);

            SetReminderControls(false);

            btnAdd = AppStyle.Button("Add Task", 660, 170, 150, 35, AppStyle.Success);
            btnAdd.Click += BtnAdd_Click;
            Controls.Add(btnAdd);

            Controls.Add(AppStyle.Label("Search Task:", 30, 430, 120, 25, AppStyle.NormalFont));

            txtSearch = AppStyle.TextBox(150, 430, 300, 25);
            Controls.Add(txtSearch);

            btnSearch = AppStyle.Button("Search", 460, 425, 120, 35, AppStyle.Primary);
            btnSearch.Click += BtnSearch_Click;
            Controls.Add(btnSearch);

            btnShowPending = AppStyle.Button("Pending", 30, 470, 120, 35, AppStyle.Warning);
            btnShowPending.Click += BtnShowPending_Click;
            Controls.Add(btnShowPending);

            btnShowCompleted = AppStyle.Button("Completed", 160, 470, 130, 35, AppStyle.Success);
            btnShowCompleted.Click += BtnShowCompleted_Click;
            Controls.Add(btnShowCompleted);

            btnShowAll = AppStyle.Button("All Tasks", 300, 470, 120, 35, AppStyle.Secondary);
            btnShowAll.Click += BtnShowAll_Click;
            Controls.Add(btnShowAll);

            Controls.Add(AppStyle.Label("Daily Tasks List", 30, 515, 300, 25, AppStyle.HeaderFont));

            lbTasks = new ListBox();
            lbTasks.Location = new Point(30, 545);
            lbTasks.Size = new Size(780, 75);
            lbTasks.Font = AppStyle.NormalFont;
            Controls.Add(lbTasks);

            btnComplete = AppStyle.Button("Mark Complete", 30, 635, 150, 35, AppStyle.Success);
            btnComplete.Click += BtnComplete_Click;
            Controls.Add(btnComplete);

            btnDelete = AppStyle.Button("Delete Task", 190, 635, 150, 35, AppStyle.Danger);
            btnDelete.Click += BtnDelete_Click;
            Controls.Add(btnDelete);
        }

        void SetReminderControls(bool enabled)
        {
            lblReminder.Enabled = enabled;
            numReminder.Enabled = enabled;
            cbReminderUnit.Enabled = enabled;
        }

        private void ChkReminder_CheckedChanged(object sender, EventArgs e)
        {
            SetReminderControls(chkReminder.Checked);
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            string msg = "";
            msg += "Daily Tasks - How To Use" + Environment.NewLine;
            msg += "------------------------" + Environment.NewLine + Environment.NewLine;
            msg += "1. Write the daily task name." + Environment.NewLine;
            msg += "2. Choose priority and date." + Environment.NewLine;
            msg += "3. Notes are optional." + Environment.NewLine;
            msg += "4. Reminder is optional." + Environment.NewLine;
            msg += "5. Delete Task moves the task to Deleted Tasks." + Environment.NewLine;
            msg += "6. Deleted tasks can be restored from Deleted Tasks page." + Environment.NewLine + Environment.NewLine;
            msg += "Database path:" + Environment.NewLine;
            msg += AppData.DbPath;

            MessageBox.Show(msg, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnMain_Click(object sender, EventArgs e)
        {
            HomeForm home = new HomeForm();
            home.Show();
            this.Hide();
        }

        private void BtnWeekly_Click(object sender, EventArgs e)
        {
            WeeklyTasksForm weeklyPage = new WeeklyTasksForm();
            weeklyPage.Show();
            this.Hide();
        }

        private int GetLastInsertedID(SQLiteConnection cn)
        {
            SQLiteCommand idCmd = new SQLiteCommand("SELECT last_insert_rowid()", cn);
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string taskName = txtName.Text.Trim();
            string priority = cbPriority.SelectedItem.ToString();
            string date = dpDate.Value.ToString("yyyy/MM/dd");
            string notes = txtNotes.Text.Trim();
            string status = "Pending";

            if (taskName == "")
            {
                MessageBox.Show("Please enter a task name.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dpDate.Value.Date < DateTime.Today)
            {
                MessageBox.Show("You cannot select a past date.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SQLiteConnection cn = new SQLiteConnection(AppData.ConnString))
            {
                try
                {
                    cn.Open();

                    SQLiteCommand cmd = new SQLiteCommand(
                        "INSERT INTO DTasks (Name, Priority, Date, Notes, Status) VALUES (@Name,@Priority,@Date,@Notes,@Status)",
                        cn
                    );

                    cmd.Parameters.AddWithValue("@Name", taskName);
                    cmd.Parameters.AddWithValue("@Priority", priority);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.Parameters.AddWithValue("@Notes", notes);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.ExecuteNonQuery();

                    int newID = GetLastInsertedID(cn);

                    if (chkReminder.Checked)
                    {
                        DateTime reminderTime = ReminderHelper.GetReminderTime(numReminder.Value, cbReminderUnit.Text);
                        ReminderService.AddReminder(newID, taskName, "Daily Task", reminderTime);
                    }

                    MessageBox.Show("Task added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtName.Clear();
                    txtNotes.Clear();
                    chkReminder.Checked = false;
                    LoadDailyTasks("Pending");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving task: " + ex.Message);
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lbTasks.SelectedItem == null)
            {
                MessageBox.Show("Please select a task.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DailyTask selectedTask = (DailyTask)lbTasks.SelectedItem;

            using (SQLiteConnection cn = new SQLiteConnection(AppData.ConnString))
            {
                try
                {
                    cn.Open();

                    SQLiteCommand cmd = new SQLiteCommand("UPDATE DTasks SET Status='Deleted' WHERE ID=@ID", cn);
                    cmd.Parameters.AddWithValue("@ID", selectedTask.ID);
                    cmd.ExecuteNonQuery();

                    ReminderService.RemoveReminder(selectedTask.ID, "Daily Task");
                    lbTasks.Items.Remove(selectedTask);
                    allTasks.Remove(selectedTask);
                    LoadDailyTasks(currentFilter);

                    MessageBox.Show("Task moved to Deleted Tasks.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting task: " + ex.Message);
                }
            }
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            if (lbTasks.SelectedItem == null)
            {
                MessageBox.Show("Please select a task.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DailyTask selectedTask = (DailyTask)lbTasks.SelectedItem;

            using (SQLiteConnection cn = new SQLiteConnection(AppData.ConnString))
            {
                try
                {
                    cn.Open();

                    SQLiteCommand cmd = new SQLiteCommand("UPDATE DTasks SET Status='Completed' WHERE ID=@ID AND Status='Pending'", cn);
                    cmd.Parameters.AddWithValue("@ID", selectedTask.ID);

                    int rows = cmd.ExecuteNonQuery();

                    ReminderService.RemoveReminder(selectedTask.ID, "Daily Task");
                    lbTasks.Items.Remove(selectedTask);
                    allTasks.Remove(selectedTask);
                    LoadDailyTasks(currentFilter);

                    if (rows > 0)
                        MessageBox.Show("Task marked as completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("This task no longer exists or is already completed.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating task: " + ex.Message);
                }
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.ToLower();
            lbTasks.Items.Clear();

            foreach (DailyTask task in allTasks)
            {
                if (task.Name.ToLower().Contains(searchText))
                    lbTasks.Items.Add(task);
            }
        }

        private void BtnShowPending_Click(object sender, EventArgs e)
        {
            LoadDailyTasks("Pending");
        }

        private void BtnShowCompleted_Click(object sender, EventArgs e)
        {
            LoadDailyTasks("Completed");
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            LoadDailyTasks("All");
        }

        void LoadDailyTasks(string filter)
        {
            currentFilter = filter;
            lbTasks.Items.Clear();
            allTasks.Clear();

            string sql = "SELECT * FROM DTasks WHERE Status<>'Deleted'";

            if (filter == "Pending")
                sql = "SELECT * FROM DTasks WHERE Status='Pending'";

            if (filter == "Completed")
                sql = "SELECT * FROM DTasks WHERE Status='Completed'";

            using (SQLiteConnection cn = new SQLiteConnection(AppData.ConnString))
            {
                try
                {
                    cn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(sql, cn);
                    SQLiteDataReader r = cmd.ExecuteReader();

                    while (r.Read())
                    {
                        DailyTask t = new DailyTask(
                            Convert.ToInt32(r["ID"]),
                            r["Name"].ToString(),
                            r["Priority"].ToString(),
                            r["Date"].ToString(),
                            r["Notes"].ToString(),
                            r["Status"].ToString()
                        );

                        lbTasks.Items.Add(t);
                        allTasks.Add(t);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading tasks: " + ex.Message);
                }
            }
        }

        public class DailyTask
        {
            public int ID;
            public string Name;
            public string Priority;
            public string Date;
            public string Notes;
            public string Status;

            public DailyTask(int id, string name, string priority, string date, string notes, string status)
            {
                ID = id;
                Name = name;
                Priority = priority;
                Date = date;
                Notes = notes;
                Status = status;
            }

            public override string ToString()
            {
                return string.Format("{0}: {1} | {2} | {3} | {4}", ID, Name, Priority, Date, Status);
            }
        }
    }
}