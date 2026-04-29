using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace TaskManagerApp
{
    public class ReportsForm : Form
    {
        Button btnDailyReport;
        Button btnWeeklyReport;
        Button btnAllReports;
        Button btnMain;
        Button btnHelp;

        Label lblTitle;
        Panel pnlReport;
        Label lblTaskCount;

        Form previousPage;

        public ReportsForm(Form previous)
        {
            previousPage = previous;
            AppStyle.ApplyForm(this, "General Report", 980, 780);
            AppTray.Register(this);
            BuildUI();
        }

        void BuildUI()
        {
            lblTitle = AppStyle.Label("General Task Report", 30, 20, 500, 40, AppStyle.TitleFont);
            Controls.Add(lblTitle);

            btnDailyReport = AppStyle.Button("Daily Report", 30, 80, 150, 40, AppStyle.Primary);
            btnDailyReport.Click += BtnDailyReport_Click;
            Controls.Add(btnDailyReport);

            btnWeeklyReport = AppStyle.Button("Weekly Report", 200, 80, 150, 40, AppStyle.Primary);
            btnWeeklyReport.Click += BtnWeeklyReport_Click;
            Controls.Add(btnWeeklyReport);

            btnAllReports = AppStyle.Button("All Reports", 370, 80, 150, 40, AppStyle.Success);
            btnAllReports.Click += BtnAllReports_Click;
            Controls.Add(btnAllReports);

            btnMain = AppStyle.Button("Main Page", 540, 80, 150, 40, AppStyle.Secondary);
            btnMain.Click += BtnMain_Click;
            Controls.Add(btnMain);

            btnHelp = AppStyle.Button("How To Use", 710, 80, 150, 40, AppStyle.Warning);
            btnHelp.Click += BtnHelp_Click;
            Controls.Add(btnHelp);

            pnlReport = AppStyle.Card(30, 145, 900, 490);
            pnlReport.AutoScroll = true;
            Controls.Add(pnlReport);

            lblTaskCount = AppStyle.Label("", 30, 655, 900, 45, AppStyle.HeaderFont);
            Controls.Add(lblTaskCount);
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            string msg = "";
            msg += "General Report - How To Use" + Environment.NewLine;
            msg += "---------------------------" + Environment.NewLine + Environment.NewLine;
            msg += "Daily Report shows daily tasks only." + Environment.NewLine;
            msg += "Weekly Report shows weekly tasks only." + Environment.NewLine;
            msg += "All Reports shows both daily and weekly tasks." + Environment.NewLine;
            msg += "Deleted tasks do not appear here." + Environment.NewLine + Environment.NewLine;
            msg += "Database path:" + Environment.NewLine;
            msg += AppData.DbPath;

            MessageBox.Show(msg, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnMain_Click(object sender, EventArgs e)
        {
            previousPage.Show();
            this.Hide();
        }

        private void BtnDailyReport_Click(object sender, EventArgs e)
        {
            pnlReport.Controls.Clear();

            ReportSummary daily = LoadReportSection("DTasks", "Daily Tasks Report", 15, 10);

            lblTaskCount.Text = "Daily Tasks | Total: " + daily.Total +
                                "    |    Completed: " + daily.Completed +
                                "    |    Pending: " + daily.Pending;
        }

        private void BtnWeeklyReport_Click(object sender, EventArgs e)
        {
            pnlReport.Controls.Clear();

            ReportSummary weekly = LoadReportSection("Tasks", "Weekly Tasks Report", 15, 10);

            lblTaskCount.Text = "Weekly Tasks | Total: " + weekly.Total +
                                "    |    Completed: " + weekly.Completed +
                                "    |    Pending: " + weekly.Pending;
        }

        private void BtnAllReports_Click(object sender, EventArgs e)
        {
            pnlReport.Controls.Clear();

            ReportSummary daily = LoadReportSection("DTasks", "Daily Tasks Report", 15, 10);

            int nextY = daily.NextY + 35;

            ReportSummary weekly = LoadReportSection("Tasks", "Weekly Tasks Report", 15, nextY);

            int total = daily.Total + weekly.Total;
            int completed = daily.Completed + weekly.Completed;
            int pending = daily.Pending + weekly.Pending;

            lblTaskCount.Text = "All Reports | Total: " + total +
                                "    |    Completed: " + completed +
                                "    |    Pending: " + pending;
        }

        ReportSummary LoadReportSection(string tableName, string reportTitle, int sectionX, int sectionY)
        {
            ReportSummary summary = new ReportSummary();

            Label reportHeader = AppStyle.Label(reportTitle, sectionX, sectionY, 500, 30, AppStyle.HeaderFont);
            pnlReport.Controls.Add(reportHeader);

            string[] headers = { "Name", "Priority", "Date", "Notes", "Status" };

            int startX = sectionX;
            int startY = sectionY + 45;

            int[] widths = { 190, 110, 130, 310, 130 };

            for (int i = 0; i < headers.Length; i++)
            {
                Label lblHeader = new Label();
                lblHeader.Text = headers[i];
                lblHeader.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                lblHeader.BackColor = AppStyle.Secondary;
                lblHeader.ForeColor = Color.White;
                lblHeader.TextAlign = ContentAlignment.MiddleLeft;
                lblHeader.Location = new Point(startX, startY);
                lblHeader.Size = new Size(widths[i], 28);
                pnlReport.Controls.Add(lblHeader);

                startX += widths[i];
            }

            startY += 32;

            using (SQLiteConnection cn = new SQLiteConnection(AppData.ConnString))
            {
                try
                {
                    cn.Open();

                    string query = "SELECT * FROM " + tableName + " WHERE Status<>'Deleted'";

                    SQLiteCommand cmd = new SQLiteCommand(query, cn);
                    SQLiteDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string name = reader["Name"].ToString();
                        string priority = reader["Priority"].ToString();
                        string date = reader["Date"].ToString();
                        string notes = reader["Notes"].ToString();
                        string status = reader["Status"].ToString().Trim();

                        string[] row = { name, priority, date, notes, status };

                        startX = sectionX;

                        for (int i = 0; i < row.Length; i++)
                        {
                            Label lblRow = new Label();
                            lblRow.Text = row[i];
                            lblRow.Font = AppStyle.NormalFont;
                            lblRow.BackColor = Color.White;
                            lblRow.ForeColor = AppStyle.TextColor;
                            lblRow.BorderStyle = BorderStyle.FixedSingle;
                            lblRow.TextAlign = ContentAlignment.MiddleLeft;
                            lblRow.Location = new Point(startX, startY);
                            lblRow.Size = new Size(widths[i], 30);
                            pnlReport.Controls.Add(lblRow);

                            startX += widths[i];
                        }

                        startY += 32;

                        summary.Total++;

                        if (status.ToLower() == "completed")
                            summary.Completed++;
                    }

                    summary.Pending = summary.Total - summary.Completed;

                    if (summary.Total == 0)
                    {
                        Label emptyLabel = AppStyle.Label("No tasks found.", sectionX, startY, 300, 25, AppStyle.NormalFont);
                        pnlReport.Controls.Add(emptyLabel);
                        startY += 32;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading report: " + ex.Message);
                }
            }

            summary.NextY = startY;

            return summary;
        }

        public class ReportSummary
        {
            public int Total;
            public int Completed;
            public int Pending;
            public int NextY;
        }
    }
}