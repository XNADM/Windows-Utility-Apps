\# Disk Space Analyzer



Disk Space Analyzer is a simple Windows desktop application built with C# Windows Forms.  

It helps users scan folders, understand what is taking storage space, find large files, review large folders, and export a clear storage report.



The app is designed to be safe and easy to use. It blocks protected system locations such as Windows, Program Files, AppData, System32, and other important system folders.



\## Features



\- Choose any safe folder to scan

\- Quick folder buttons for Downloads, Desktop, and Documents

\- Full Scan, Quick Scan, Large Files Scan, and Old Files Scan

\- Shows total scanned size

\- Shows total files count

\- Shows total folders count

\- Shows the largest file

\- Shows the most used file type

\- Top 20 largest files list

\- Largest folders list

\- File type summary

\- Extension summary

\- File age summary

\- Search by file name, extension, category, or path

\- Press Enter to search

\- Filter by file type

\- Filter by file size

\- Sort by size, date, or name

\- Select multiple files or folders

\- Select All button for files and folders

\- Open selected file or folder

\- Open file or folder location

\- Copy selected paths

\- Move selected files or folders

\- Delete selected files or folders with double confirmation

\- Export scan report as a text file

\- Reset saved settings

\- Built-in How To Use guide



\## Safety



Disk Space Analyzer is built with safety in mind.



The app blocks scanning, moving, and deleting from protected system locations, including:



\- `C:\\Windows`

\- `C:\\Windows\\System32`

\- `C:\\Program Files`

\- `C:\\Program Files (x86)`

\- `AppData`

\- `WindowsApps`

\- Other important Windows system paths



The app does not delete anything automatically.  

Delete actions require two confirmations.



\## Data Storage



The app saves only simple settings locally in the user's AppData folder.



Examples of saved settings:



\- Last selected folder

\- Last scan mode

\- Last type filter

\- Last size filter

\- Last sort option



No external database is required.



This app does not use:



\- Access

\- SQLite

\- SQL Server

\- External database files



\## How To Use



1\. Open the app.

2\. Click Browse or choose Downloads, Desktop, or Documents.

3\. Select a scan mode.

4\. Click Scan Folder.

5\. Review the largest files and folders.

6\. Use filters or search if needed.

7\. Select one or more items.

8\. Use Open Selected, Open Location, Copy Path, Move Selected, or Delete Selected.

9\. Export a report if needed.



\## Built With



\- C#

\- Windows Forms

\- Local AppData file-based settings



\## Developer



Developed by Mohammed Ahmed

