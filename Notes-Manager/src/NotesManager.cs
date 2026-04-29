using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace NotesManagerApp
{
    public class NotesManagerForm : Form
    {
        TextBox txtTitle;
        TextBox txtContent;
        TextBox txtSearch;
        ComboBox cbCategory;
        ComboBox cbFilter;
        ListBox lbNotes;

        CheckBox chkPinned;
        CheckBox chkImportant;

        Button btnAdd;
        Button btnUpdate;
        Button btnDelete;
        Button btnClear;
        Button btnExport;
        Button btnReset;
        Button btnHowToUse;
        Button btnSearch;
        Button btnShowAll;

        Label lblStatus;

        List<NoteItem> notes = new List<NoteItem>();
        int selectedNoteID = -1;

        static string userName = Environment.UserName;
        static string dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            userName + "_NotesManager_Data"
        );

        static string dataFile = Path.Combine(dataFolder, "notes_data.txt");

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NotesManagerForm());
        }

        public NotesManagerForm()
        {
            PrepareDataFolder();
            BuildUI();
            LoadNotes();
            RefreshList();
        }

        void BuildUI()
        {
            this.Text = "Notes Manager";
            this.Size = new Size(980, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            Label title = new Label();
            title.Text = "Notes Manager";
            title.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(35, 35, 35);
            title.Location = new Point(30, 20);
            title.Size = new Size(400, 45);
            this.Controls.Add(title);

            btnReset = CreateButton("Reset Data", 820, 25, 110, 30, Color.FromArgb(192, 57, 43));
            btnReset.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            Panel inputCard = CreateCard(30, 80, 420, 520);
            this.Controls.Add(inputCard);

            Label lblTitle = CreateLabel("Note Title:", 20, 25, 120, 25, FontStyle.Regular);
            inputCard.Controls.Add(lblTitle);

            txtTitle = new TextBox();
            txtTitle.Location = new Point(140, 25);
            txtTitle.Size = new Size(250, 25);
            inputCard.Controls.Add(txtTitle);

            Label lblCategory = CreateLabel("Category:", 20, 65, 120, 25, FontStyle.Regular);
            inputCard.Controls.Add(lblCategory);

            cbCategory = new ComboBox();
            cbCategory.Location = new Point(140, 65);
            cbCategory.Size = new Size(160, 25);
            cbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCategory.Items.Add("General");
            cbCategory.Items.Add("Study");
            cbCategory.Items.Add("Work");
            cbCategory.Items.Add("Personal");
            cbCategory.Items.Add("Important");
            cbCategory.Items.Add("Ideas");
            cbCategory.SelectedIndex = 0;
            inputCard.Controls.Add(cbCategory);

            chkPinned = new CheckBox();
            chkPinned.Text = "Pinned";
            chkPinned.Location = new Point(140, 105);
            chkPinned.Size = new Size(100, 25);
            inputCard.Controls.Add(chkPinned);

            chkImportant = new CheckBox();
            chkImportant.Text = "Important";
            chkImportant.Location = new Point(250, 105);
            chkImportant.Size = new Size(120, 25);
            inputCard.Controls.Add(chkImportant);

            Label lblContent = CreateLabel("Content:", 20, 145, 120, 25, FontStyle.Regular);
            inputCard.Controls.Add(lblContent);

            txtContent = new TextBox();
            txtContent.Location = new Point(20, 175);
            txtContent.Size = new Size(370, 190);
            txtContent.Multiline = true;
            txtContent.ScrollBars = ScrollBars.Vertical;
            inputCard.Controls.Add(txtContent);

            btnAdd = CreateButton("Add Note", 20, 390, 115, 35, Color.FromArgb(39, 174, 96));
            btnAdd.Click += BtnAdd_Click;
            inputCard.Controls.Add(btnAdd);

            btnUpdate = CreateButton("Update Note", 145, 390, 115, 35, Color.FromArgb(35, 97, 146));
            btnUpdate.Click += BtnUpdate_Click;
            inputCard.Controls.Add(btnUpdate);

            btnDelete = CreateButton("Delete Note", 270, 390, 120, 35, Color.FromArgb(192, 57, 43));
            btnDelete.Click += BtnDelete_Click;
            inputCard.Controls.Add(btnDelete);

            btnClear = CreateButton("Clear Fields", 20, 440, 115, 35, Color.FromArgb(44, 62, 80));
            btnClear.Click += BtnClear_Click;
            inputCard.Controls.Add(btnClear);

            btnExport = CreateButton("Export Note", 145, 440, 115, 35, Color.FromArgb(243, 156, 18));
            btnExport.Click += BtnExport_Click;
            inputCard.Controls.Add(btnExport);

            btnHowToUse = CreateButton("How To Use", 270, 440, 120, 35, Color.FromArgb(127, 140, 141));
            btnHowToUse.Click += BtnHowToUse_Click;
            inputCard.Controls.Add(btnHowToUse);

            Panel listCard = CreateCard(480, 80, 450, 520);
            this.Controls.Add(listCard);

            Label lblSearch = CreateLabel("Search:", 20, 25, 85, 25, FontStyle.Regular);
            listCard.Controls.Add(lblSearch);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(110, 25);
            txtSearch.Size = new Size(185, 25);
            listCard.Controls.Add(txtSearch);

            btnSearch = CreateButton("Search", 310, 22, 100, 30, Color.FromArgb(35, 97, 146));
            btnSearch.Click += BtnSearch_Click;
            listCard.Controls.Add(btnSearch);

            Label lblFilter = CreateLabel("Filter:", 20, 65, 85, 25, FontStyle.Regular);
            listCard.Controls.Add(lblFilter);

            cbFilter = new ComboBox();
            cbFilter.Location = new Point(110, 65);
            cbFilter.Size = new Size(145, 25);
            cbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilter.Items.Add("All");
            cbFilter.Items.Add("Pinned");
            cbFilter.Items.Add("Important");
            cbFilter.Items.Add("General");
            cbFilter.Items.Add("Study");
            cbFilter.Items.Add("Work");
            cbFilter.Items.Add("Personal");
            cbFilter.Items.Add("Ideas");
            cbFilter.SelectedIndex = 0;
            cbFilter.SelectedIndexChanged += CbFilter_SelectedIndexChanged;
            listCard.Controls.Add(cbFilter);

            btnShowAll = CreateButton("Show All", 275, 62, 135, 30, Color.FromArgb(44, 62, 80));
            btnShowAll.Click += BtnShowAll_Click;
            listCard.Controls.Add(btnShowAll);

            Label lblNotes = CreateLabel("Notes List", 20, 110, 200, 25, FontStyle.Bold);
            listCard.Controls.Add(lblNotes);

            lbNotes = new ListBox();
            lbNotes.Location = new Point(20, 145);
            lbNotes.Size = new Size(390, 310);
            lbNotes.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lbNotes.SelectedIndexChanged += LbNotes_SelectedIndexChanged;
            listCard.Controls.Add(lbNotes);

            lblStatus = new Label();
            lblStatus.Text = "Ready";
            lblStatus.Location = new Point(20, 470);
            lblStatus.Size = new Size(390, 30);
            lblStatus.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
            listCard.Controls.Add(lblStatus);

            Label developedBy = new Label();
            developedBy.Text = "Developed by Mohammed Ahmed Alhijab";
            developedBy.Location = new Point(0, 625);
            developedBy.Size = new Size(980, 25);
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
            p.BackColor = Color.White;
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
            lbl.ForeColor = Color.FromArgb(35, 35, 35);
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

        void LoadNotes()
        {
            notes.Clear();

            if (!File.Exists(dataFile))
                return;

            string[] lines = File.ReadAllLines(dataFile);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('|');

                if (parts.Length != 8)
                    continue;

                try
                {
                    NoteItem note = new NoteItem();
                    note.ID = Convert.ToInt32(parts[0]);
                    note.Title = Decode(parts[1]);
                    note.Category = Decode(parts[2]);
                    note.Content = Decode(parts[3]);
                    note.Pinned = parts[4] == "1";
                    note.Important = parts[5] == "1";
                    note.CreatedDate = Decode(parts[6]);
                    note.UpdatedDate = Decode(parts[7]);

                    notes.Add(note);
                }
                catch
                {
                }
            }
        }

        void SaveNotes()
        {
            List<string> lines = new List<string>();

            foreach (NoteItem note in notes)
            {
                string line =
                    note.ID + "|" +
                    Encode(note.Title) + "|" +
                    Encode(note.Category) + "|" +
                    Encode(note.Content) + "|" +
                    (note.Pinned ? "1" : "0") + "|" +
                    (note.Important ? "1" : "0") + "|" +
                    Encode(note.CreatedDate) + "|" +
                    Encode(note.UpdatedDate);

                lines.Add(line);
            }

            File.WriteAllLines(dataFile, lines.ToArray());
        }

        string Encode(string value)
        {
            if (value == null)
                value = "";

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        string Decode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            byte[] bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }

        int GetNextID()
        {
            int max = 0;

            foreach (NoteItem note in notes)
            {
                if (note.ID > max)
                    max = note.ID;
            }

            return max + 1;
        }

        void RefreshList()
        {
            lbNotes.Items.Clear();

            List<NoteItem> displayNotes = new List<NoteItem>();

            foreach (NoteItem note in notes)
            {
                displayNotes.Add(note);
            }

            displayNotes.Sort((a, b) =>
            {
                if (a.Pinned && !b.Pinned)
                    return -1;

                if (!a.Pinned && b.Pinned)
                    return 1;

                return b.ID.CompareTo(a.ID);
            });

            foreach (NoteItem note in displayNotes)
            {
                lbNotes.Items.Add(note);
            }

            lblStatus.Text = "Total notes: " + notes.Count;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string category = cbCategory.Text;
            string content = txtContent.Text.Trim();

            if (title == "")
            {
                MessageBox.Show("Please enter a note title.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (content == "")
            {
                MessageBox.Show("Please enter note content.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NoteItem note = new NoteItem();
            note.ID = GetNextID();
            note.Title = title;
            note.Category = category;
            note.Content = content;
            note.Pinned = chkPinned.Checked;
            note.Important = chkImportant.Checked;
            note.CreatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            note.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            notes.Add(note);
            SaveNotes();
            RefreshList();
            ClearFields();

            MessageBox.Show("Note added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedNoteID == -1)
            {
                MessageBox.Show("Please select a note to update.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NoteItem note = FindNoteByID(selectedNoteID);

            if (note == null)
            {
                MessageBox.Show("Selected note was not found.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string title = txtTitle.Text.Trim();
            string content = txtContent.Text.Trim();

            if (title == "")
            {
                MessageBox.Show("Please enter a note title.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (content == "")
            {
                MessageBox.Show("Please enter note content.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            note.Title = title;
            note.Category = cbCategory.Text;
            note.Content = content;
            note.Pinned = chkPinned.Checked;
            note.Important = chkImportant.Checked;
            note.UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            SaveNotes();
            RefreshList();
            ClearFields();

            MessageBox.Show("Note updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedNoteID == -1)
            {
                MessageBox.Show("Please select a note to delete.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NoteItem note = FindNoteByID(selectedNoteID);

            if (note == null)
            {
                MessageBox.Show("Selected note was not found.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to delete this note?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            notes.Remove(note);
            SaveNotes();
            RefreshList();
            ClearFields();

            MessageBox.Show("Note deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (selectedNoteID == -1)
            {
                MessageBox.Show("Please select a note to export.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NoteItem note = FindNoteByID(selectedNoteID);

            if (note == null)
            {
                MessageBox.Show("Selected note was not found.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export Note";
            dialog.Filter = "Text File|*.txt";
            dialog.FileName = SafeFileName(note.Title) + ".txt";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string output = "";
                output += "Title: " + note.Title + Environment.NewLine;
                output += "Category: " + note.Category + Environment.NewLine;
                output += "Pinned: " + (note.Pinned ? "Yes" : "No") + Environment.NewLine;
                output += "Important: " + (note.Important ? "Yes" : "No") + Environment.NewLine;
                output += "Created: " + note.CreatedDate + Environment.NewLine;
                output += "Updated: " + note.UpdatedDate + Environment.NewLine;
                output += Environment.NewLine;
                output += "Content:" + Environment.NewLine;
                output += note.Content;

                File.WriteAllText(dialog.FileName, output);
                MessageBox.Show("Note exported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        string SafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c.ToString(), "");
            }

            if (name.Trim() == "")
                name = "note";

            return name;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim().ToLower();

            if (search == "")
            {
                RefreshList();
                return;
            }

            lbNotes.Items.Clear();

            foreach (NoteItem note in notes)
            {
                if (note.Title.ToLower().Contains(search) ||
                    note.Category.ToLower().Contains(search) ||
                    note.Content.ToLower().Contains(search))
                {
                    lbNotes.Items.Add(note);
                }
            }

            lblStatus.Text = "Search results: " + lbNotes.Items.Count;
        }

        private void CbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        void ApplyFilter()
        {
            string filter = cbFilter.Text;

            lbNotes.Items.Clear();

            foreach (NoteItem note in notes)
            {
                bool show = false;

                if (filter == "All")
                    show = true;
                else if (filter == "Pinned" && note.Pinned)
                    show = true;
                else if (filter == "Important" && note.Important)
                    show = true;
                else if (note.Category == filter)
                    show = true;

                if (show)
                    lbNotes.Items.Add(note);
            }

            lblStatus.Text = filter + " notes: " + lbNotes.Items.Count;
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cbFilter.SelectedIndex = 0;
            RefreshList();
        }

        private void LbNotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbNotes.SelectedItem == null)
                return;

            NoteItem note = (NoteItem)lbNotes.SelectedItem;

            selectedNoteID = note.ID;
            txtTitle.Text = note.Title;
            cbCategory.Text = note.Category;
            txtContent.Text = note.Content;
            chkPinned.Checked = note.Pinned;
            chkImportant.Checked = note.Important;

            lblStatus.Text = "Selected note ID: " + note.ID + " | Created: " + note.CreatedDate + " | Updated: " + note.UpdatedDate;
        }

        NoteItem FindNoteByID(int id)
        {
            foreach (NoteItem note in notes)
            {
                if (note.ID == id)
                    return note;
            }

            return null;
        }

        void ClearFields()
        {
            selectedNoteID = -1;
            txtTitle.Clear();
            txtContent.Clear();
            txtSearch.Clear();
            cbCategory.SelectedIndex = 0;
            cbFilter.SelectedIndex = 0;
            chkPinned.Checked = false;
            chkImportant.Checked = false;
            lbNotes.ClearSelected();
            lblStatus.Text = "Ready";
        }

        private void BtnHowToUse_Click(object sender, EventArgs e)
        {
            string msg = "";

            msg += "Notes Manager - How To Use" + Environment.NewLine;
            msg += "--------------------------" + Environment.NewLine + Environment.NewLine;

            msg += "Add Note:" + Environment.NewLine;
            msg += "- Write a title." + Environment.NewLine;
            msg += "- Choose a category." + Environment.NewLine;
            msg += "- Write the note content." + Environment.NewLine;
            msg += "- Optional: mark it as Pinned or Important." + Environment.NewLine;
            msg += "- Click Add Note." + Environment.NewLine + Environment.NewLine;

            msg += "Update Note:" + Environment.NewLine;
            msg += "- Select a note from the list." + Environment.NewLine;
            msg += "- Edit the title, category, content, pinned, or important options." + Environment.NewLine;
            msg += "- Click Update Note." + Environment.NewLine + Environment.NewLine;

            msg += "Delete Note:" + Environment.NewLine;
            msg += "- Select a note from the list." + Environment.NewLine;
            msg += "- Click Delete Note." + Environment.NewLine;
            msg += "- Confirm the delete message." + Environment.NewLine + Environment.NewLine;

            msg += "Search and Filter:" + Environment.NewLine;
            msg += "- Use Search to find notes by title, category, or content." + Environment.NewLine;
            msg += "- Use Filter to show All, Pinned, Important, or one category only." + Environment.NewLine + Environment.NewLine;

            msg += "Export Note:" + Environment.NewLine;
            msg += "- Select a note." + Environment.NewLine;
            msg += "- Click Export Note." + Environment.NewLine;
            msg += "- Choose where to save it as a text file." + Environment.NewLine + Environment.NewLine;

            msg += "Reset Data:" + Environment.NewLine;
            msg += "- Reset Data deletes all notes." + Environment.NewLine;
            msg += "- It requires two confirmations." + Environment.NewLine + Environment.NewLine;

            msg += "Data Storage:" + Environment.NewLine;
            msg += "- Notes are saved automatically in AppData." + Environment.NewLine;
            msg += "- No Access database is required." + Environment.NewLine;
            msg += "- No SQLite files are required." + Environment.NewLine + Environment.NewLine;

            msg += "Data path:" + Environment.NewLine;
            msg += dataFile;

            MessageBox.Show(msg, "How To Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            DialogResult firstConfirm = MessageBox.Show(
                "Warning: This will delete all saved notes." + Environment.NewLine + Environment.NewLine +
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

            notes.Clear();
            SaveNotes();
            RefreshList();
            ClearFields();

            MessageBox.Show("All notes were deleted successfully.", "Reset Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        string AskResetConfirmation()
        {
            Form f = new Form();
            f.Text = "Final Reset Confirmation";
            f.Size = new Size(470, 230);
            f.StartPosition = FormStartPosition.CenterParent;
            f.BackColor = Color.FromArgb(245, 247, 250);
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;

            Label lbl = new Label();
            lbl.Text = "Final confirmation:" + Environment.NewLine + "Type RESET exactly to delete all notes.";
            lbl.Location = new Point(25, 25);
            lbl.Size = new Size(400, 55);
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            f.Controls.Add(lbl);

            TextBox tb = new TextBox();
            tb.Location = new Point(25, 90);
            tb.Size = new Size(400, 25);
            f.Controls.Add(tb);

            Button ok = CreateDialogButton("Confirm Reset", 75, 140, 140, 35, Color.FromArgb(192, 57, 43));
            Button cancel = CreateDialogButton("Cancel", 230, 140, 120, 35, Color.FromArgb(44, 62, 80));

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

        Button CreateDialogButton(string text, int x, int y, int w, int h, Color color)
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

        public class NoteItem
        {
            public int ID;
            public string Title;
            public string Category;
            public string Content;
            public bool Pinned;
            public bool Important;
            public string CreatedDate;
            public string UpdatedDate;

            public override string ToString()
            {
                string flags = "";

                if (Pinned)
                    flags += "[Pinned] ";

                if (Important)
                    flags += "[Important] ";

                return flags + ID + ": " + Title + " | " + Category + " | Updated: " + UpdatedDate;
            }
        }
    }
}