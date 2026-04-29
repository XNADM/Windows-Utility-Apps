using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace StudyPlannerApp
{
    public class StudyPlannerForm : Form
    {
        TextBox txtSubject;
        TextBox txtTopic;
        TextBox txtNotes;
        TextBox txtSearch;

        ComboBox cbPriority;
        ComboBox cbStatus;
        ComboBox cbFilter;

        DateTimePicker dpExamDate;
        NumericUpDown numHours;

        ListBox lbPlans;

        Button btnAdd;
        Button btnUpdate;
        Button btnDelete;
        Button btnComplete;
        Button btnClear;
        Button btnExport;
        Button btnSearch;
        Button btnShowAll;
        Button btnHowToUse;
        Button btnReset;

        Label lblTotal;
        Label lblPending;
        Label lblCompleted;
        Label lblUpcoming;
        Label lblCountdown;
        Label lblStatus;

        ProgressBar progressCompleted;

        List<StudyPlanItem> plans = new List<StudyPlanItem>();
        int selectedPlanID = -1;

        static string userName = Environment.UserName;
        static string dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            userName + "_StudyPlanner_Data"
        );

        static string dataFile = Path.Combine(dataFolder, "study_data.txt");

        Color bg = Color.FromArgb(236, 240, 245);
        Color dark = Color.FromArgb(31, 41, 55);
        Color purple = Color.FromArgb(91, 75, 138);
        Color blue = Color.FromArgb(35, 97, 146);
        Color green = Color.FromArgb(39, 174, 96);
        Color red = Color.FromArgb(192, 57, 43);
        Color orange = Color.FromArgb(243, 156, 18);
        Color card = Color.White;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StudyPlannerForm());
        }

        public StudyPlannerForm()
        {
            PrepareDataFolder();
            BuildUI();
            LoadPlans();
            RefreshList();
            UpdateDashboard();
        }

        void BuildUI()
        {
            this.Text = "Study Planner";
            this.Size = new Size(1100, 760);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bg;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            Label title = new Label();
            title.Text = "Study Planner";
            title.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            title.ForeColor = dark;
            title.Location = new Point(30, 18);
            title.Size = new Size(380, 45);
            this.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Plan your subjects, track exam dates, and finish your study goals.";
            subtitle.Font = new Font("Segoe UI", 10, FontStyle.Italic);
            subtitle.ForeColor = Color.FromArgb(90, 90, 90);
            subtitle.Location = new Point(35, 60);
            subtitle.Size = new Size(600, 25);
            this.Controls.Add(subtitle);

            btnReset = CreateButton("Reset Data", 940, 25, 110, 30, red);
            btnReset.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            Panel totalCard = CreateInfoCard(30, 100, 245, 80, "Total Plans");
            lblTotal = CreateBigNumber("0", 20, 34, 190, 35);
            totalCard.Controls.Add(lblTotal);
            this.Controls.Add(totalCard);

            Panel pendingCard = CreateInfoCard(295, 100, 245, 80, "Pending");
            lblPending = CreateBigNumber("0", 20, 34, 190, 35);
            pendingCard.Controls.Add(lblPending);
            this.Controls.Add(pendingCard);

            Panel completedCard = CreateInfoCard(560, 100, 245, 80, "Completed");
            lblCompleted = CreateBigNumber("0", 20, 34, 190, 35);
            completedCard.Controls.Add(lblCompleted);
            this.Controls.Add(completedCard);

            Panel upcomingCard = CreateInfoCard(825, 100, 245, 80, "Upcoming Exams");
            lblUpcoming = CreateBigNumber("0", 20, 34, 190, 35);
            upcomingCard.Controls.Add(lblUpcoming);
            this.Controls.Add(upcomingCard);

            Panel inputCard = CreateCard(30, 200, 420, 440);
            this.Controls.Add(inputCard);

            Label inputHeader = CreateLabel("Create / Edit Study Plan", 20, 15, 300, 30, FontStyle.Bold);
            inputHeader.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            inputCard.Controls.Add(inputHeader);

            inputCard.Controls.Add(CreateLabel("Subject:", 20, 60, 110, 25, FontStyle.Regular));
            txtSubject = new TextBox();
            txtSubject.Location = new Point(140, 60);
            txtSubject.Size = new Size(245, 25);
            inputCard.Controls.Add(txtSubject);

            inputCard.Controls.Add(CreateLabel("Topic:", 20, 100, 110, 25, FontStyle.Regular));
            txtTopic = new TextBox();
            txtTopic.Location = new Point(140, 100);
            txtTopic.Size = new Size(245, 25);
            inputCard.Controls.Add(txtTopic);

            inputCard.Controls.Add(CreateLabel("Priority:", 20, 140, 110, 25, FontStyle.Regular));
            cbPriority = new ComboBox();
            cbPriority.Location = new Point(140, 140);
            cbPriority.Size = new Size(145, 25);
            cbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPriority.Items.Add("High");
            cbPriority.Items.Add("Medium");
            cbPriority.Items.Add("Low");
            cbPriority.SelectedIndex = 1;
            inputCard.Controls.Add(cbPriority);

            inputCard.Controls.Add(CreateLabel("Status:", 20, 180, 110, 25, FontStyle.Regular));
            cbStatus = new ComboBox();
            cbStatus.Location = new Point(140, 180);
            cbStatus.Size = new Size(145, 25);
            cbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cbStatus.Items.Add("Pending");
            cbStatus.Items.Add("Completed");
            cbStatus.SelectedIndex = 0;
            inputCard.Controls.Add(cbStatus);

            inputCard.Controls.Add(CreateLabel("Exam Date:", 20, 220, 110, 25, FontStyle.Regular));
            dpExamDate = new DateTimePicker();
            dpExamDate.Location = new Point(140, 220);
            dpExamDate.Size = new Size(145, 25);
            dpExamDate.Format = DateTimePickerFormat.Short;
            inputCard.Controls.Add(dpExamDate);

            inputCard.Controls.Add(CreateLabel("Study Hours:", 20, 260, 110, 25, FontStyle.Regular));
            numHours = new NumericUpDown();
            numHours.Location = new Point(140, 260);
            numHours.Size = new Size(80, 25);
            numHours.Minimum = 1;
            numHours.Maximum = 200;
            numHours.Value = 2;
            inputCard.Controls.Add(numHours);

            inputCard.Controls.Add(CreateLabel("Notes:", 20, 300, 110, 25, FontStyle.Regular));
            txtNotes = new TextBox();
            txtNotes.Location = new Point(140, 300);
            txtNotes.Size = new Size(245, 55);
            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            inputCard.Controls.Add(txtNotes);

            btnAdd = CreateButton("Add", 20, 375, 80, 35, green);
            btnAdd.Click += BtnAdd_Click;
            inputCard.Controls.Add(btnAdd);

            btnUpdate = CreateButton("Update", 110, 375, 85, 35, blue);
            btnUpdate.Click += BtnUpdate_Click;
            inputCard.Controls.Add(btnUpdate);

            btnComplete = CreateButton("Complete", 205, 375, 90, 35, purple);
            btnComplete.Click += BtnComplete_Click;
            inputCard.Controls.Add(btnComplete);

            btnDelete = CreateButton("Delete", 305, 375, 80, 35, red);
            btnDelete.Click += BtnDelete_Click;
            inputCard.Controls.Add(btnDelete);

            Panel dashboardCard = CreateCard(470, 200, 600, 115);
            this.Controls.Add(dashboardCard);

            Label dashTitle = CreateLabel("Study Progress", 20, 15, 240, 28, FontStyle.Bold);
            dashTitle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            dashboardCard.Controls.Add(dashTitle);

            progressCompleted = new ProgressBar();
            progressCompleted.Location = new Point(20, 55);
            progressCompleted.Size = new Size(360, 22);
            progressCompleted.Minimum = 0;
            progressCompleted.Maximum = 100;
            dashboardCard.Controls.Add(progressCompleted);

            lblCountdown = new Label();
            lblCountdown.Text = "Next exam: No plans yet";
            lblCountdown.Location = new Point(20, 82);
            lblCountdown.Size = new Size(560, 25);
            lblCountdown.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblCountdown.ForeColor = purple;
            dashboardCard.Controls.Add(lblCountdown);

            btnClear = CreateButton("Clear Fields", 405, 52, 145, 34, dark);
            btnClear.Click += BtnClear_Click;
            dashboardCard.Controls.Add(btnClear);

            Panel listCard = CreateCard(470, 335, 600, 305);
            this.Controls.Add(listCard);

            Label listHeader = CreateLabel("Study Plans", 20, 15, 200, 28, FontStyle.Bold);
            listHeader.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            listCard.Controls.Add(listHeader);

            listCard.Controls.Add(CreateLabel("Search:", 20, 55, 75, 25, FontStyle.Regular));
            txtSearch = new TextBox();
            txtSearch.Location = new Point(95, 55);
            txtSearch.Size = new Size(210, 25);
            listCard.Controls.Add(txtSearch);

            btnSearch = CreateButton("Search", 315, 52, 90, 30, blue);
            btnSearch.Click += BtnSearch_Click;
            listCard.Controls.Add(btnSearch);

            listCard.Controls.Add(CreateLabel("Filter:", 420, 55, 55, 25, FontStyle.Regular));
            cbFilter = new ComboBox();
            cbFilter.Location = new Point(475, 55);
            cbFilter.Size = new Size(105, 25);
            cbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilter.Items.Add("All");
            cbFilter.Items.Add("Pending");
            cbFilter.Items.Add("Completed");
            cbFilter.Items.Add("High");
            cbFilter.Items.Add("Upcoming");
            cbFilter.SelectedIndex = 0;
            cbFilter.SelectedIndexChanged += CbFilter_SelectedIndexChanged;
            listCard.Controls.Add(cbFilter);

            lbPlans = new ListBox();
            lbPlans.Location = new Point(20, 95);
            lbPlans.Size = new Size(560, 150);
            lbPlans.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lbPlans.SelectedIndexChanged += LbPlans_SelectedIndexChanged;
            listCard.Controls.Add(lbPlans);

            btnShowAll = CreateButton("Show All", 20, 255, 100, 32, dark);
            btnShowAll.Click += BtnShowAll_Click;
            listCard.Controls.Add(btnShowAll);

            btnExport = CreateButton("Export", 130, 255, 100, 32, orange);
            btnExport.Click += BtnExport_Click;
            listCard.Controls.Add(btnExport);

            btnHowToUse = CreateButton("How To Use", 240, 255, 120, 32, Color.FromArgb(127, 140, 141));
            btnHowToUse.Click += BtnHowToUse_Click;
            listCard.Controls.Add(btnHowToUse);

            lblStatus = new Label();
            lblStatus.Text = "Ready";
            lblStatus.Location = new Point(375, 260);
            lblStatus.Size = new Size(205, 25);
            lblStatus.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
            listCard.Controls.Add(lblStatus);

            Label developedBy = new Label();
            developedBy.Text = "Developed by Mohammed Ahmed Alhijab";
            developedBy.Location = new Point(0, 670);
            developedBy.Size = new Size(1100, 25);
            developedBy.TextAlign = ContentAlignment.MiddleCenter;
            developedBy.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            developedBy.ForeColor = Color.FromArgb(80, 80, 80);
            this.Controls.Add(developedBy);
        }

        Panel CreateInfoCard(int x, int y, int w, int h, string text)
        {
            Panel p = CreateCard(x, y, w, h);

            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(18, 12);
            lbl.Size = new Size(w - 30, 25);
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(90, 90, 90);
            p.Controls.Add(lbl);

            return p;
        }

        Label CreateBigNumber(string text, int x, int y, int w, int h)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(w, h);
            lbl.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lbl.ForeColor = purple;
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

        void LoadPlans()
        {
            plans.Clear();

            if (!File.Exists(dataFile))
                return;

            string[] lines = File.ReadAllLines(dataFile);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('|');

                if (parts.Length != 9)
                    continue;

                try
                {
                    StudyPlanItem item = new StudyPlanItem();
                    item.ID = Convert.ToInt32(parts[0]);
                    item.Subject = Decode(parts[1]);
                    item.Topic = Decode(parts[2]);
                    item.Priority = Decode(parts[3]);
                    item.Status = Decode(parts[4]);
                    item.ExamDate = Decode(parts[5]);
                    item.StudyHours = Decode(parts[6]);
                    item.Notes = Decode(parts[7]);
                    item.UpdatedDate = Decode(parts[8]);
                    plans.Add(item);
                }
                catch
                {
                }
            }
        }

        void SavePlans()
        {
            List<string> lines = new List<string>();

            foreach (StudyPlanItem item in plans)
            {
                string line =
                    item.ID + "|" +
                    Encode(item.Subject) + "|" +
                    Encode(item.Topic) + "|" +
                    Encode(item.Priority) + "|" +
                    Encode(item.Status) + "|" +
                    Encode(item.ExamDate) + "|" +
                    Encode(item.StudyHours) + "|" +
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

            foreach (StudyPlanItem item in plans)
            {
                if (item.ID > max)
                    max = item.ID;
            }

            return max + 1;
        }

        void RefreshList()
        {
            lbPlans.Items.Clear();

            List<StudyPlanItem> displayPlans = new List<StudyPlanItem>();

            foreach (StudyPlanItem item in plans)
                displayPlans.Add(item);

            displayPlans.Sort((a, b) =>
            {
                int pa = GetPriorityValue(a.Priority);
                int pb = GetPriorityValue(b.Priority);

                if (pa != pb)
                    return pa.CompareTo(pb);

                DateTime da;
                DateTime db;

                bool va = DateTime.TryParse(a.ExamDate, out da);
                bool vb = DateTime.TryParse(b.ExamDate, out db);

                if (va && vb)
                    return da.CompareTo(db);

                return b.ID.CompareTo(a.ID);
            });

            foreach (StudyPlanItem item in displayPlans)
                lbPlans.Items.Add(item);

            lblStatus.Text = "Total: " + plans.Count;

            UpdateDashboard();
        }

        void UpdateDashboard()
        {
            int total = plans.Count;
            int completed = 0;
            int pending = 0;
            int upcoming = 0;

            StudyPlanItem nextExam = null;
            int nearestDays = 999999;

            foreach (StudyPlanItem item in plans)
            {
                if (item.Status == "Completed")
                    completed++;
                else
                    pending++;

                DateTime date;

                if (DateTime.TryParse(item.ExamDate, out date))
                {
                    int days = (date.Date - DateTime.Today).Days;

                    if (days >= 0 && days <= 7)
                        upcoming++;

                    if (days >= 0 && days < nearestDays && item.Status != "Completed")
                    {
                        nearestDays = days;
                        nextExam = item;
                    }
                }
            }

            lblTotal.Text = total.ToString();
            lblPending.Text = pending.ToString();
            lblCompleted.Text = completed.ToString();
            lblUpcoming.Text = upcoming.ToString();

            int percent = 0;

            if (total > 0)
                percent = (completed * 100) / total;

            progressCompleted.Value = percent;

            if (nextExam == null)
                lblCountdown.Text = "Next exam: No pending exams";
            else if (nearestDays == 0)
                lblCountdown.Text = "Next exam: " + nextExam.Subject + " - Today";
            else
                lblCountdown.Text = "Next exam: " + nextExam.Subject + " - " + nearestDays + " day(s) left";
        }

        int GetPriorityValue(string priority)
        {
            if (priority == "High")
                return 1;

            if (priority == "Medium")
                return 2;

            return 3;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string subject = txtSubject.Text.Trim();
            string topic = txtTopic.Text.Trim();

            if (subject == "")
            {
                MessageBox.Show("Please enter a subject.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (topic == "")
            {
                MessageBox.Show("Please enter a topic.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dpExamDate.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Exam date cannot be in the past.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StudyPlanItem item = new StudyPlanItem();
            item.ID = GetNextID();
            item.Subject = subject;
            item.Topic = topic;
            item.Priority = cbPriority.Text;
            item.Status = cbStatus.Text;
            item.ExamDate = dpExamDate.Value.ToString("yyyy/MM/dd");
            item.StudyHours = numHours.Value.ToString();
            item.Notes = txtNotes.Text.Trim();
            item.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            plans.Add(item);
            SavePlans();
            RefreshList();
            ClearFields();

            MessageBox.Show("Study plan added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedPlanID == -1)
            {
                MessageBox.Show("Please select a study plan to update.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StudyPlanItem item = FindPlanByID(selectedPlanID);

            if (item == null)
            {
                MessageBox.Show("Selected study plan was not found.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtSubject.Text.Trim() == "" || txtTopic.Text.Trim() == "")
            {
                MessageBox.Show("Subject and topic are required.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            item.Subject = txtSubject.Text.Trim();
            item.Topic = txtTopic.Text.Trim();
            item.Priority = cbPriority.Text;
            item.Status = cbStatus.Text;
            item.ExamDate = dpExamDate.Value.ToString("yyyy/MM/dd");
            item.StudyHours = numHours.Value.ToString();
            item.Notes = txtNotes.Text.Trim();
            item.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            SavePlans();
            RefreshList();
            ClearFields();

            MessageBox.Show("Study plan updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedPlanID == -1)
            {
                MessageBox.Show("Please select a study plan to delete.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StudyPlanItem item = FindPlanByID(selectedPlanID);

            if (item == null)
                return;

            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this study plan?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            plans.Remove(item);
            SavePlans();
            RefreshList();
            ClearFields();

            MessageBox.Show("Study plan deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            if (selectedPlanID == -1)
            {
                MessageBox.Show("Please select a study plan.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StudyPlanItem item = FindPlanByID(selectedPlanID);

            if (item == null)
                return;

            item.Status = "Completed";
            item.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            SavePlans();
            RefreshList();
            ClearFields();

            MessageBox.Show("Study plan marked as completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            lbPlans.Items.Clear();

            foreach (StudyPlanItem item in plans)
            {
                if (item.Subject.ToLower().Contains(search) ||
                    item.Topic.ToLower().Contains(search) ||
                    item.Priority.ToLower().Contains(search) ||
                    item.Status.ToLower().Contains(search) ||
                    item.Notes.ToLower().Contains(search))
                {
                    lbPlans.Items.Add(item);
                }
            }

            lblStatus.Text = "Search: " + lbPlans.Items.Count;
        }

        private void CbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        void ApplyFilter()
        {
            string filter = cbFilter.Text;

            lbPlans.Items.Clear();

            foreach (StudyPlanItem item in plans)
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
                else if (filter == "Upcoming" && IsUpcomingExam(item.ExamDate))
                    show = true;

                if (show)
                    lbPlans.Items.Add(item);
            }

            lblStatus.Text = filter + ": " + lbPlans.Items.Count;
        }

        bool IsUpcomingExam(string examDate)
        {
            DateTime date;

            if (!DateTime.TryParse(examDate, out date))
                return false;

            return date.Date >= DateTime.Today && date.Date <= DateTime.Today.AddDays(7);
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cbFilter.SelectedIndex = 0;
            RefreshList();
        }

        private void LbPlans_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbPlans.SelectedItem == null)
                return;

            StudyPlanItem item = (StudyPlanItem)lbPlans.SelectedItem;

            selectedPlanID = item.ID;
            txtSubject.Text = item.Subject;
            txtTopic.Text = item.Topic;
            cbPriority.Text = item.Priority;
            cbStatus.Text = item.Status;

            DateTime date;

            if (DateTime.TryParse(item.ExamDate, out date))
                dpExamDate.Value = date;

            decimal hours;

            if (decimal.TryParse(item.StudyHours, out hours))
            {
                if (hours >= numHours.Minimum && hours <= numHours.Maximum)
                    numHours.Value = hours;
            }

            txtNotes.Text = item.Notes;
            lblStatus.Text = "Selected ID: " + item.ID;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (selectedPlanID == -1)
            {
                MessageBox.Show("Please select a study plan to export.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StudyPlanItem item = FindPlanByID(selectedPlanID);

            if (item == null)
                return;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export Study Plan";
            dialog.Filter = "Text File|*.txt";
            dialog.FileName = SafeFileName(item.Subject + " - " + item.Topic) + ".txt";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string output = "";
                output += "Subject: " + item.Subject + Environment.NewLine;
                output += "Topic: " + item.Topic + Environment.NewLine;
                output += "Priority: " + item.Priority + Environment.NewLine;
                output += "Status: " + item.Status + Environment.NewLine;
                output += "Exam Date: " + item.ExamDate + Environment.NewLine;
                output += "Study Hours: " + item.StudyHours + Environment.NewLine;
                output += "Updated: " + item.UpdatedDate + Environment.NewLine;
                output += Environment.NewLine;
                output += "Notes:" + Environment.NewLine;
                output += item.Notes;

                File.WriteAllText(dialog.FileName, output);
                MessageBox.Show("Study plan exported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        string SafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "");

            if (name.Trim() == "")
                name = "study_plan";

            return name;
        }

        StudyPlanItem FindPlanByID(int id)
        {
            foreach (StudyPlanItem item in plans)
            {
                if (item.ID == id)
                    return item;
            }

            return null;
        }

        void ClearFields()
        {
            selectedPlanID = -1;
            txtSubject.Clear();
            txtTopic.Clear();
            txtNotes.Clear();
            txtSearch.Clear();
            cbPriority.SelectedIndex = 1;
            cbStatus.SelectedIndex = 0;
            cbFilter.SelectedIndex = 0;
            dpExamDate.Value = DateTime.Today;
            numHours.Value = 2;
            lbPlans.ClearSelected();
            lblStatus.Text = "Ready";
        }

        private void BtnHowToUse_Click(object sender, EventArgs e)
        {
            string msg = "";

            msg += "Study Planner - How To Use" + Environment.NewLine;
            msg += "--------------------------" + Environment.NewLine + Environment.NewLine;
            msg += "Add Plan:" + Environment.NewLine;
            msg += "- Write the subject and topic." + Environment.NewLine;
            msg += "- Choose priority, status, exam date, and study hours." + Environment.NewLine;
            msg += "- Add notes if needed, then click Add." + Environment.NewLine + Environment.NewLine;
            msg += "Dashboard:" + Environment.NewLine;
            msg += "- Total Plans shows all saved plans." + Environment.NewLine;
            msg += "- Pending and Completed show your study progress." + Environment.NewLine;
            msg += "- Upcoming Exams shows exams within 7 days." + Environment.NewLine;
            msg += "- Progress bar shows completed percentage." + Environment.NewLine + Environment.NewLine;
            msg += "Search and Filter:" + Environment.NewLine;
            msg += "- Search by subject, topic, priority, status, or notes." + Environment.NewLine;
            msg += "- Filter by All, Pending, Completed, High, or Upcoming." + Environment.NewLine + Environment.NewLine;
            msg += "Data Storage:" + Environment.NewLine;
            msg += "- Data is saved automatically in AppData." + Environment.NewLine;
            msg += "- No Access database is required." + Environment.NewLine;
            msg += "- No SQLite files are required." + Environment.NewLine + Environment.NewLine;
            msg += "Data path:" + Environment.NewLine;
            msg += dataFile;

            MessageBox.Show(msg, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            DialogResult firstConfirm = MessageBox.Show(
                "Warning: This will delete all saved study plans." + Environment.NewLine + Environment.NewLine +
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

            plans.Clear();
            SavePlans();
            RefreshList();
            ClearFields();

            MessageBox.Show("All study plans were deleted successfully.", "Reset Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            lbl.Text = "Final confirmation:" + Environment.NewLine + "Type RESET exactly to delete all study plans.";
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

        public class StudyPlanItem
        {
            public int ID;
            public string Subject;
            public string Topic;
            public string Priority;
            public string Status;
            public string ExamDate;
            public string StudyHours;
            public string Notes;
            public string UpdatedDate;

            public override string ToString()
            {
                string flag = "";

                if (Status == "Completed")
                    flag = "[DONE] ";
                else if (Priority == "High")
                    flag = "[HIGH] ";

                return flag + ID + ": " + Subject + " - " + Topic + " | " + Priority + " | " + Status + " | Exam: " + ExamDate;
            }
        }
    }
}