using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PCHealthToolsApp
{
    public class PCHealthToolsForm : Form
    {
        Button btnTemp;
        Button btnDownloads;
        Button btnRecycleBin;
        Button btnStartupApps;
        Button btnTaskManager;
        Button btnDeviceManager;
        Button btnDiskCleanup;
        Button btnWindowsSecurity;
        Button btnSystemInfo;
        Button btnDiskSpace;
        Button btnControlPanel;
        Button btnNetworkSettings;
        Button btnPowerOptions;
        Button btnWindowsUpdate;
        Button btnCommandPrompt;
        Button btnHowToUse;
        Button btnReset;

        Label lblStatus;
        Label lblDiskInfo;
        ProgressBar diskProgress;

        static string userName = Environment.UserName;
        static string dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            userName + "_PCHealthTools_Data"
        );

        static string settingsFile = Path.Combine(dataFolder, "settings.txt");

        Color bg = Color.FromArgb(236, 240, 245);
        Color dark = Color.FromArgb(31, 41, 55);
        Color blue = Color.FromArgb(35, 97, 146);
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
            Application.Run(new PCHealthToolsForm());
        }

        public PCHealthToolsForm()
        {
            PrepareDataFolder();
            BuildUI();
            LoadDiskInfo();
        }

        void BuildUI()
        {
            this.Text = "PC Health Tools";
            this.Size = new Size(1060, 730);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bg;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            Label title = new Label();
            title.Text = "PC Health Tools";
            title.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            title.ForeColor = dark;
            title.Location = new Point(30, 18);
            title.Size = new Size(430, 45);
            this.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Quick access to useful Windows tools, folders, and system checks.";
            subtitle.Font = new Font("Segoe UI", 10, FontStyle.Italic);
            subtitle.ForeColor = Color.FromArgb(90, 90, 90);
            subtitle.Location = new Point(35, 60);
            subtitle.Size = new Size(700, 25);
            this.Controls.Add(subtitle);

            btnReset = CreateButton("Reset Data", 895, 25, 110, 30, red);
            btnReset.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            Panel statusCard = CreateCard(30, 100, 975, 115);
            this.Controls.Add(statusCard);

            Label diskTitle = CreateLabel("Main Drive Status", 20, 15, 250, 28, FontStyle.Bold);
            diskTitle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            statusCard.Controls.Add(diskTitle);

            lblDiskInfo = new Label();
            lblDiskInfo.Text = "Loading disk information...";
            lblDiskInfo.Location = new Point(20, 48);
            lblDiskInfo.Size = new Size(650, 25);
            lblDiskInfo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblDiskInfo.ForeColor = purple;
            statusCard.Controls.Add(lblDiskInfo);

            diskProgress = new ProgressBar();
            diskProgress.Location = new Point(20, 78);
            diskProgress.Size = new Size(650, 20);
            diskProgress.Minimum = 0;
            diskProgress.Maximum = 100;
            statusCard.Controls.Add(diskProgress);

            btnDiskSpace = CreateButton("Refresh Disk Space", 720, 43, 200, 38, blue);
            btnDiskSpace.Click += BtnDiskSpace_Click;
            statusCard.Controls.Add(btnDiskSpace);

            Panel toolsCard = CreateCard(30, 235, 975, 350);
            this.Controls.Add(toolsCard);

            Label toolsTitle = CreateLabel("Quick Tools", 20, 15, 250, 30, FontStyle.Bold);
            toolsTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            toolsCard.Controls.Add(toolsTitle);

            btnTemp = CreateButton("Open Temp Folder", 30, 65, 200, 42, blue);
            btnTemp.Click += BtnTemp_Click;
            toolsCard.Controls.Add(btnTemp);

            btnDownloads = CreateButton("Open Downloads", 250, 65, 200, 42, blue);
            btnDownloads.Click += BtnDownloads_Click;
            toolsCard.Controls.Add(btnDownloads);

            btnRecycleBin = CreateButton("Open Recycle Bin", 470, 65, 200, 42, green);
            btnRecycleBin.Click += BtnRecycleBin_Click;
            toolsCard.Controls.Add(btnRecycleBin);

            btnStartupApps = CreateButton("Startup Apps", 690, 65, 200, 42, orange);
            btnStartupApps.Click += BtnStartupApps_Click;
            toolsCard.Controls.Add(btnStartupApps);

            btnTaskManager = CreateButton("Task Manager", 30, 125, 200, 42, purple);
            btnTaskManager.Click += BtnTaskManager_Click;
            toolsCard.Controls.Add(btnTaskManager);

            btnDeviceManager = CreateButton("Device Manager", 250, 125, 200, 42, purple);
            btnDeviceManager.Click += BtnDeviceManager_Click;
            toolsCard.Controls.Add(btnDeviceManager);

            btnDiskCleanup = CreateButton("Disk Cleanup", 470, 125, 200, 42, green);
            btnDiskCleanup.Click += BtnDiskCleanup_Click;
            toolsCard.Controls.Add(btnDiskCleanup);

            btnWindowsSecurity = CreateButton("Windows Security", 690, 125, 200, 42, red);
            btnWindowsSecurity.Click += BtnWindowsSecurity_Click;
            toolsCard.Controls.Add(btnWindowsSecurity);

            btnSystemInfo = CreateButton("System Information", 30, 185, 200, 42, dark);
            btnSystemInfo.Click += BtnSystemInfo_Click;
            toolsCard.Controls.Add(btnSystemInfo);

            btnControlPanel = CreateButton("Control Panel", 250, 185, 200, 42, dark);
            btnControlPanel.Click += BtnControlPanel_Click;
            toolsCard.Controls.Add(btnControlPanel);

            btnNetworkSettings = CreateButton("Network Settings", 470, 185, 200, 42, blue);
            btnNetworkSettings.Click += BtnNetworkSettings_Click;
            toolsCard.Controls.Add(btnNetworkSettings);

            btnPowerOptions = CreateButton("Power Options", 690, 185, 200, 42, orange);
            btnPowerOptions.Click += BtnPowerOptions_Click;
            toolsCard.Controls.Add(btnPowerOptions);

            btnWindowsUpdate = CreateButton("Windows Update", 30, 245, 200, 42, green);
            btnWindowsUpdate.Click += BtnWindowsUpdate_Click;
            toolsCard.Controls.Add(btnWindowsUpdate);

            btnCommandPrompt = CreateButton("Command Prompt", 250, 245, 200, 42, dark);
            btnCommandPrompt.Click += BtnCommandPrompt_Click;
            toolsCard.Controls.Add(btnCommandPrompt);

            btnHowToUse = CreateButton("How To Use", 470, 245, 200, 42, gray);
            btnHowToUse.Click += BtnHowToUse_Click;
            toolsCard.Controls.Add(btnHowToUse);

            lblStatus = new Label();
            lblStatus.Text = "Ready";
            lblStatus.Location = new Point(690, 255);
            lblStatus.Size = new Size(220, 25);
            lblStatus.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
            toolsCard.Controls.Add(lblStatus);

            Panel safetyCard = CreateCard(30, 605, 975, 45);
            this.Controls.Add(safetyCard);

            Label safety = new Label();
            safety.Text = "Safety Note: This app opens official Windows tools only. It does not delete files, change settings, or run dangerous commands automatically.";
            safety.Location = new Point(20, 12);
            safety.Size = new Size(930, 25);
            safety.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            safety.ForeColor = Color.FromArgb(90, 90, 90);
            safetyCard.Controls.Add(safety);

            Label developedBy = new Label();
            developedBy.Text = "Developed by Mohammed Ahmed Alhijab";
            developedBy.Location = new Point(0, 660);
            developedBy.Size = new Size(1060, 25);
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
                File.WriteAllText(settingsFile, "PC Health Tools settings file");
        }

        void OpenProcess(string fileName)
        {
            try
            {
                Process.Start(fileName);
                lblStatus.Text = "Opened";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error";
            }
        }

        void OpenProcessWithArgs(string fileName, string args)
        {
            try
            {
                Process.Start(fileName, args);
                lblStatus.Text = "Opened";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error";
            }
        }

        void LoadDiskInfo()
        {
            try
            {
                string root = Path.GetPathRoot(Environment.SystemDirectory);
                DriveInfo drive = new DriveInfo(root);

                long total = drive.TotalSize;
                long free = drive.AvailableFreeSpace;
                long used = total - free;

                int usedPercent = 0;

                if (total > 0)
                    usedPercent = Convert.ToInt32((used * 100) / total);

                if (usedPercent < 0)
                    usedPercent = 0;

                if (usedPercent > 100)
                    usedPercent = 100;

                diskProgress.Value = usedPercent;

                lblDiskInfo.Text =
                    root +
                    " Used: " + FormatSize(used) +
                    " / " + FormatSize(total) +
                    "   Free: " + FormatSize(free) +
                    "   (" + usedPercent + "% used)";

                lblStatus.Text = "Disk checked";
            }
            catch (Exception ex)
            {
                lblDiskInfo.Text = "Could not read disk information.";
                MessageBox.Show("Disk check error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void BtnDiskSpace_Click(object sender, EventArgs e)
        {
            LoadDiskInfo();
        }

        private void BtnTemp_Click(object sender, EventArgs e)
        {
            string temp = Path.GetTempPath();

            if (Directory.Exists(temp))
                OpenProcess(temp);
            else
                MessageBox.Show("Temp folder was not found.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnDownloads_Click(object sender, EventArgs e)
        {
            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            if (Directory.Exists(downloads))
                OpenProcess(downloads);
            else
                MessageBox.Show("Downloads folder was not found.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnRecycleBin_Click(object sender, EventArgs e)
        {
            OpenProcess("shell:RecycleBinFolder");
        }

        private void BtnStartupApps_Click(object sender, EventArgs e)
        {
            OpenProcessWithArgs("cmd.exe", "/c start ms-settings:startupapps");
        }

        private void BtnTaskManager_Click(object sender, EventArgs e)
        {
            OpenProcess("taskmgr.exe");
        }

        private void BtnDeviceManager_Click(object sender, EventArgs e)
        {
            OpenProcess("devmgmt.msc");
        }

        private void BtnDiskCleanup_Click(object sender, EventArgs e)
        {
            OpenProcess("cleanmgr.exe");
        }

        private void BtnWindowsSecurity_Click(object sender, EventArgs e)
        {
            OpenProcessWithArgs("cmd.exe", "/c start windowsdefender:");
        }

        private void BtnSystemInfo_Click(object sender, EventArgs e)
        {
            OpenProcess("msinfo32.exe");
        }

        private void BtnControlPanel_Click(object sender, EventArgs e)
        {
            OpenProcess("control.exe");
        }

        private void BtnNetworkSettings_Click(object sender, EventArgs e)
        {
            OpenProcessWithArgs("cmd.exe", "/c start ms-settings:network");
        }

        private void BtnPowerOptions_Click(object sender, EventArgs e)
        {
            OpenProcess("powercfg.cpl");
        }

        private void BtnWindowsUpdate_Click(object sender, EventArgs e)
        {
            OpenProcessWithArgs("cmd.exe", "/c start ms-settings:windowsupdate");
        }

        private void BtnCommandPrompt_Click(object sender, EventArgs e)
        {
            OpenProcess("cmd.exe");
        }

        private void BtnHowToUse_Click(object sender, EventArgs e)
        {
            string msg = "";

            msg += "PC Health Tools - How To Use" + Environment.NewLine;
            msg += "----------------------------" + Environment.NewLine + Environment.NewLine;

            msg += "Main Drive Status:" + Environment.NewLine;
            msg += "- Shows used space, free space, and storage percentage for the main Windows drive." + Environment.NewLine;
            msg += "- Click Refresh Disk Space to update the numbers." + Environment.NewLine + Environment.NewLine;

            msg += "Quick Tools:" + Environment.NewLine;
            msg += "- Open Temp Folder opens the Windows temporary folder." + Environment.NewLine;
            msg += "- Open Downloads opens your Downloads folder." + Environment.NewLine;
            msg += "- Open Recycle Bin opens deleted items." + Environment.NewLine;
            msg += "- Startup Apps opens Windows startup app settings." + Environment.NewLine;
            msg += "- Task Manager opens Windows Task Manager." + Environment.NewLine;
            msg += "- Device Manager opens hardware device settings." + Environment.NewLine;
            msg += "- Disk Cleanup opens the official Windows cleanup tool." + Environment.NewLine;
            msg += "- Windows Security opens Microsoft Defender / Security app." + Environment.NewLine;
            msg += "- System Information opens detailed PC information." + Environment.NewLine;
            msg += "- Control Panel opens classic Windows Control Panel." + Environment.NewLine;
            msg += "- Network Settings opens Windows network settings." + Environment.NewLine;
            msg += "- Power Options opens battery and power plan settings." + Environment.NewLine;
            msg += "- Windows Update opens update settings." + Environment.NewLine;
            msg += "- Command Prompt opens CMD." + Environment.NewLine + Environment.NewLine;

            msg += "Safety:" + Environment.NewLine;
            msg += "- This app opens Windows tools only." + Environment.NewLine;
            msg += "- It does not delete files automatically." + Environment.NewLine;
            msg += "- It does not change system settings automatically." + Environment.NewLine + Environment.NewLine;

            msg += "Data Storage:" + Environment.NewLine;
            msg += "- Only a small settings file is saved in AppData." + Environment.NewLine;
            msg += "- No Access database is required." + Environment.NewLine;
            msg += "- No SQLite files are required." + Environment.NewLine + Environment.NewLine;

            msg += "Settings path:" + Environment.NewLine;
            msg += settingsFile;

            MessageBox.Show(msg, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            DialogResult firstConfirm = MessageBox.Show(
                "Reset Data will clear the small saved settings file only." + Environment.NewLine +
                "It will not delete your files and will not change Windows settings." + Environment.NewLine + Environment.NewLine +
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
                File.WriteAllText(settingsFile, "PC Health Tools settings file");
                LoadDiskInfo();
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
    }
}