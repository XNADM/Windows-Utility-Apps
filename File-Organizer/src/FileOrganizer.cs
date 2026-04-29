using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

class FileOrganizerForm : Form
{
    TextBox txtFolder;
    Button btnBrowse;
    Button btnOrganize;
    Button btnHowToUse;

    CheckBox chkAll;
    CheckBox chkPhotos;
    CheckBox chkVideos;
    CheckBox chkAudio;
    CheckBox chkPdf;
    CheckBox chkWord;
    CheckBox chkExcel;
    CheckBox chkPowerPoint;
    CheckBox chkCompressed;
    CheckBox chkText;
    CheckBox chkApplications;
    CheckBox chkCode;
    CheckBox chkDesign;
    CheckBox chkDatabase;
    CheckBox chkFonts;
    CheckBox chkOther;

    Dictionary<string, string[]> categories = new Dictionary<string, string[]>
    {
        { "Photos", new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".heic", ".tif", ".tiff", ".svg", ".ico", ".raw", ".cr2", ".nef", ".arw" } },
        { "Videos", new string[] { ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".mpeg", ".mpg", ".3gp", ".m4v", ".ts", ".mts" } },
        { "Audio Files", new string[] { ".mp3", ".wav", ".aac", ".flac", ".ogg", ".m4a", ".wma", ".amr", ".mid", ".midi" } },

        { "PDF Files", new string[] { ".pdf" } },
        { "Word Documents", new string[] { ".doc", ".docx", ".dot", ".dotx", ".rtf", ".odt" } },
        { "Excel Files", new string[] { ".xls", ".xlsx", ".xlsm", ".csv", ".ods" } },
        { "PowerPoint Files", new string[] { ".ppt", ".pptx", ".pptm", ".pps", ".ppsx", ".odp" } },

        { "Compressed Files", new string[] { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".iso", ".cab" } },
        { "Text Files", new string[] { ".txt", ".log", ".md", ".ini", ".cfg", ".conf", ".json", ".xml", ".yaml", ".yml" } },
        { "Applications", new string[] { ".exe", ".msi", ".bat", ".cmd", ".ps1", ".apk", ".appx", ".deb", ".rpm", ".jar" } },
        { "Code Files", new string[] { ".cs", ".java", ".py", ".cpp", ".c", ".h", ".html", ".css", ".js", ".ts", ".php", ".sql", ".rb", ".go", ".swift", ".kt", ".vb", ".xaml" } },
        { "Design Files", new string[] { ".psd", ".ai", ".fig", ".xd", ".sketch", ".indd", ".eps", ".cdr", ".dwg", ".dxf" } },
        { "Database Files", new string[] { ".db", ".sqlite", ".sqlite3", ".mdb", ".accdb", ".bak", ".mdf", ".ldf" } },
        { "Font Files", new string[] { ".ttf", ".otf", ".woff", ".woff2", ".eot" } }
    };

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new FileOrganizerForm());
    }

    public FileOrganizerForm()
    {
        BuildUI();
    }

    void BuildUI()
    {
        this.Text = "Smart File Organizer";
        this.Width = 620;
        this.Height = 690;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        Label title = new Label();
        title.Text = "Smart File Organizer";
        title.Left = 30;
        title.Top = 20;
        title.Width = 520;
        title.Height = 40;
        title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        this.Controls.Add(title);

        Label lblFolder = new Label();
        lblFolder.Text = "Choose Folder:";
        lblFolder.Left = 30;
        lblFolder.Top = 80;
        lblFolder.Width = 150;
        this.Controls.Add(lblFolder);

        txtFolder = new TextBox();
        txtFolder.Left = 30;
        txtFolder.Top = 105;
        txtFolder.Width = 430;
        txtFolder.ReadOnly = true;
        this.Controls.Add(txtFolder);

        btnBrowse = new Button();
        btnBrowse.Text = "Browse";
        btnBrowse.Left = 470;
        btnBrowse.Top = 103;
        btnBrowse.Width = 100;
        btnBrowse.Click += BtnBrowse_Click;
        this.Controls.Add(btnBrowse);

        Label lblOptions = new Label();
        lblOptions.Text = "Choose what to organize:";
        lblOptions.Left = 30;
        lblOptions.Top = 150;
        lblOptions.Width = 250;
        this.Controls.Add(lblOptions);

        chkAll = CreateCheckBox("All Files", 30, 180);

        chkPhotos = CreateCheckBox("Photos", 30, 220);
        chkVideos = CreateCheckBox("Videos", 30, 250);
        chkAudio = CreateCheckBox("Audio Files", 30, 280);
        chkPdf = CreateCheckBox("PDF Files", 30, 310);
        chkWord = CreateCheckBox("Word Documents", 30, 340);
        chkExcel = CreateCheckBox("Excel Files", 30, 370);
        chkPowerPoint = CreateCheckBox("PowerPoint Files", 30, 400);
        chkCompressed = CreateCheckBox("Compressed Files", 30, 430);

        chkText = CreateCheckBox("Text Files", 310, 220);
        chkApplications = CreateCheckBox("Applications", 310, 250);
        chkCode = CreateCheckBox("Code Files", 310, 280);
        chkDesign = CreateCheckBox("Design Files", 310, 310);
        chkDatabase = CreateCheckBox("Database Files", 310, 340);
        chkFonts = CreateCheckBox("Font Files", 310, 370);
        chkOther = CreateCheckBox("Other Files", 310, 400);

        chkAll.CheckedChanged += ChkAll_CheckedChanged;

        this.Controls.Add(chkAll);
        this.Controls.Add(chkPhotos);
        this.Controls.Add(chkVideos);
        this.Controls.Add(chkAudio);
        this.Controls.Add(chkPdf);
        this.Controls.Add(chkWord);
        this.Controls.Add(chkExcel);
        this.Controls.Add(chkPowerPoint);
        this.Controls.Add(chkCompressed);
        this.Controls.Add(chkText);
        this.Controls.Add(chkApplications);
        this.Controls.Add(chkCode);
        this.Controls.Add(chkDesign);
        this.Controls.Add(chkDatabase);
        this.Controls.Add(chkFonts);
        this.Controls.Add(chkOther);

        Label madeBy = new Label();
        madeBy.Text = "Developed by Mohammed Ahmed";
        madeBy.Left = 30;
        madeBy.Top = 485;
        madeBy.Width = 540;
        madeBy.Height = 22;
        madeBy.TextAlign = ContentAlignment.MiddleCenter;
        madeBy.Font = new Font("Segoe UI", 9, FontStyle.Italic);
        this.Controls.Add(madeBy);

        btnHowToUse = new Button();
        btnHowToUse.Text = "How To Use";
        btnHowToUse.Left = 30;
        btnHowToUse.Top = 525;
        btnHowToUse.Width = 540;
        btnHowToUse.Height = 35;
        btnHowToUse.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        btnHowToUse.Click += BtnHowToUse_Click;
        this.Controls.Add(btnHowToUse);

        btnOrganize = new Button();
        btnOrganize.Text = "Organize Files";
        btnOrganize.Left = 30;
        btnOrganize.Top = 575;
        btnOrganize.Width = 540;
        btnOrganize.Height = 45;
        btnOrganize.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        btnOrganize.Click += BtnOrganize_Click;
        this.Controls.Add(btnOrganize);
    }

    CheckBox CreateCheckBox(string text, int left, int top)
    {
        CheckBox chk = new CheckBox();
        chk.Text = text;
        chk.Left = left;
        chk.Top = top;
        chk.Width = 220;
        return chk;
    }

    void BtnBrowse_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtFolder.Text = dialog.SelectedPath;
        }
    }

    void BtnHowToUse_Click(object sender, EventArgs e)
    {
        string message = "";

        message += "How To Use Smart File Organizer" + Environment.NewLine;
        message += "--------------------------------" + Environment.NewLine + Environment.NewLine;

        message += "1. Click Browse." + Environment.NewLine;
        message += "2. Choose the folder you want to organize." + Environment.NewLine;
        message += "   Example: Desktop or Downloads." + Environment.NewLine + Environment.NewLine;

        message += "3. Choose the file type you want to organize." + Environment.NewLine;
        message += "   Example: Photos, Videos, PDF Files, Word Documents." + Environment.NewLine + Environment.NewLine;

        message += "4. If you choose All Files, the program will organize all files it finds." + Environment.NewLine;
        message += "   Unknown file types will go to Other Files." + Environment.NewLine + Environment.NewLine;

        message += "5. Click Organize Files." + Environment.NewLine + Environment.NewLine;

        message += "Example before organizing:" + Environment.NewLine;
        message += "photo.jpg" + Environment.NewLine;
        message += "video.mp4" + Environment.NewLine;
        message += "report.docx" + Environment.NewLine;
        message += "file.abc" + Environment.NewLine + Environment.NewLine;

        message += "Example after organizing:" + Environment.NewLine;
        message += "Photos folder -> photo.jpg" + Environment.NewLine;
        message += "Videos folder -> video.mp4" + Environment.NewLine;
        message += "Word Documents folder -> report.docx" + Environment.NewLine;
        message += "Other Files folder -> file.abc" + Environment.NewLine + Environment.NewLine;

        message += "Important Notes:" + Environment.NewLine;
        message += "- The program organizes only the selected folder." + Environment.NewLine;
        message += "- It does not organize the whole computer." + Environment.NewLine;
        message += "- It does not create empty folders." + Environment.NewLine;
        message += "- If a file has the same name, it will rename it automatically." + Environment.NewLine;
        message += "  Example: photo.jpg -> photo (1).jpg";

        MessageBox.Show(message, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void ChkAll_CheckedChanged(object sender, EventArgs e)
    {
        bool status = chkAll.Checked;

        chkPhotos.Checked = status;
        chkVideos.Checked = status;
        chkAudio.Checked = status;
        chkPdf.Checked = status;
        chkWord.Checked = status;
        chkExcel.Checked = status;
        chkPowerPoint.Checked = status;
        chkCompressed.Checked = status;
        chkText.Checked = status;
        chkApplications.Checked = status;
        chkCode.Checked = status;
        chkDesign.Checked = status;
        chkDatabase.Checked = status;
        chkFonts.Checked = status;
        chkOther.Checked = status;
    }

    void BtnOrganize_Click(object sender, EventArgs e)
    {
        string selectedFolder = txtFolder.Text;

        if (string.IsNullOrWhiteSpace(selectedFolder) || !Directory.Exists(selectedFolder))
        {
            MessageBox.Show("Choose a folder first.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        List<string> selectedCategories = GetSelectedCategories();

        if (selectedCategories.Count == 0)
        {
            MessageBox.Show("Choose at least one file type.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Dictionary<string, int> report = new Dictionary<string, int>();
        int movedFiles = OrganizeFiles(selectedFolder, selectedCategories, chkAll.Checked, report);

        if (chkAll.Checked)
        {
            if (movedFiles == 0)
            {
                MessageBox.Show("No files were found in this folder.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(BuildReportMessage(movedFiles, report), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else
        {
            if (movedFiles == 0)
            {
                MessageBox.Show("No matching files were found for the selected type.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(BuildReportMessage(movedFiles, report), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    List<string> GetSelectedCategories()
    {
        List<string> selected = new List<string>();

        if (chkPhotos.Checked)
            selected.Add("Photos");

        if (chkVideos.Checked)
            selected.Add("Videos");

        if (chkAudio.Checked)
            selected.Add("Audio Files");

        if (chkPdf.Checked)
            selected.Add("PDF Files");

        if (chkWord.Checked)
            selected.Add("Word Documents");

        if (chkExcel.Checked)
            selected.Add("Excel Files");

        if (chkPowerPoint.Checked)
            selected.Add("PowerPoint Files");

        if (chkCompressed.Checked)
            selected.Add("Compressed Files");

        if (chkText.Checked)
            selected.Add("Text Files");

        if (chkApplications.Checked)
            selected.Add("Applications");

        if (chkCode.Checked)
            selected.Add("Code Files");

        if (chkDesign.Checked)
            selected.Add("Design Files");

        if (chkDatabase.Checked)
            selected.Add("Database Files");

        if (chkFonts.Checked)
            selected.Add("Font Files");

        if (chkOther.Checked)
            selected.Add("Other Files");

        return selected;
    }

    int OrganizeFiles(string selectedFolder, List<string> selectedCategories, bool allSelected, Dictionary<string, int> report)
    {
        int movedCount = 0;
        string[] files = Directory.GetFiles(selectedFolder);

        foreach (string file in files)
        {
            string extension = Path.GetExtension(file).ToLower();
            string matchedCategory = GetCategoryByExtension(extension);

            if (matchedCategory == "")
            {
                matchedCategory = "Other Files";
            }

            if (allSelected || ListHasValue(selectedCategories, matchedCategory))
            {
                string targetFolder = Path.Combine(selectedFolder, matchedCategory);

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                string fileName = Path.GetFileName(file);
                string targetPath = Path.Combine(targetFolder, fileName);
                targetPath = GetUniqueFilePath(targetPath);

                File.Move(file, targetPath);

                movedCount++;

                if (!report.ContainsKey(matchedCategory))
                {
                    report.Add(matchedCategory, 0);
                }

                report[matchedCategory] = report[matchedCategory] + 1;
            }
        }

        return movedCount;
    }

    string GetCategoryByExtension(string extension)
    {
        foreach (string category in categories.Keys)
        {
            if (ExtensionExists(categories[category], extension))
            {
                return category;
            }
        }

        return "";
    }

    bool ExtensionExists(string[] extensions, string extension)
    {
        foreach (string item in extensions)
        {
            if (item == extension)
            {
                return true;
            }
        }

        return false;
    }

    bool ListHasValue(List<string> list, string value)
    {
        foreach (string item in list)
        {
            if (item == value)
            {
                return true;
            }
        }

        return false;
    }

    string GetUniqueFilePath(string path)
    {
        if (!File.Exists(path))
        {
            return path;
        }

        string folder = Path.GetDirectoryName(path);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path);

        int counter = 1;
        string newPath = "";

        do
        {
            string newFileName = fileNameWithoutExtension + " (" + counter + ")" + extension;
            newPath = Path.Combine(folder, newFileName);
            counter++;
        }
        while (File.Exists(newPath));

        return newPath;
    }

    string BuildReportMessage(int movedFiles, Dictionary<string, int> report)
    {
        string message = "Done. " + movedFiles + " file(s) organized successfully." + Environment.NewLine + Environment.NewLine;
        message = message + "Organized files:" + Environment.NewLine;

        string[] order = new string[]
        {
            "Photos",
            "Videos",
            "Audio Files",
            "PDF Files",
            "Word Documents",
            "Excel Files",
            "PowerPoint Files",
            "Compressed Files",
            "Text Files",
            "Applications",
            "Code Files",
            "Design Files",
            "Database Files",
            "Font Files",
            "Other Files"
        };

        foreach (string category in order)
        {
            if (report.ContainsKey(category))
            {
                message = message + category + ": " + report[category] + Environment.NewLine;
            }
        }

        return message;
    }
}