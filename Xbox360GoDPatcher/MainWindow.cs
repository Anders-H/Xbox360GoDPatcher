using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Xbox360GoDPatcher
{
    public partial class MainWindow : Form
    {
        private const int StartAddress = 556; // 0x22C
        private const int FFCount = 8;
        private const int ZeroCount = 7;
        private const int LengthRequirement = 580;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void listView1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            
            listView1.BeginUpdate();
            listView1.Items.Clear();
            Cursor = Cursors.WaitCursor;

            foreach (string file in files)
            {
                try
                {
                    var fi = new FileInfo(file);
                    var li = listView1.Items.Add(fi.Name);
                    li.Tag = fi;
                    var status = CheckFilename(fi.Name) ? PatchStatus.NotPatched : PatchStatus.UnexpectedFilename;
                    var subItem = li.SubItems.Add(PatchStatusAsString(status));
                    subItem.Tag = status;
                }
                catch
                {
                    // ignored
                }
            }

            Cursor = Cursors.Default;
            listView1.EndUpdate();
        }

        private static string PatchStatusAsString(PatchStatus status)
        {
            switch (status)
            {
                case PatchStatus.NotPatched:
                    return "Not patched";
                case PatchStatus.Patched:
                    return "Patched";
                case PatchStatus.FailedFileLocked:
                    return "File is locked";
                case PatchStatus.FailedFileTooSmall:
                    return "File too small";
                case PatchStatus.UnexpectedFilename:
                    return "Unexpected filename";
                case PatchStatus.UnknownFail:
                    return "Unknown fail";
                default:
                    return "";
            }
        }

        private static bool CheckFilename(string filename) =>
            Regex.IsMatch(filename, "^[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]$");

        private void selectNotPatchedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.SelectedItems.Clear();
            foreach (ListViewItem i in listView1.Items)
            {
                var status = (PatchStatus)i.SubItems[1].Tag;

                if (status == PatchStatus.NotPatched)
                    i.Selected = true;
            }
        }

        private void patchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var success = 0;
            var fail = 0;
            Cursor = Cursors.WaitCursor;

            foreach (ListViewItem i in listView1.SelectedItems)
            {
                var fi = (FileInfo)i.Tag;
                i.EnsureVisible();

                var result = PatchFile(fi);

                if (result == PatchStatus.Patched)
                    success++;
                else
                    fail++;

                i.SubItems[1].Tag = result;
                i.SubItems[1].Text = PatchStatusAsString(result);

                listView1.Refresh();
            }

            Cursor = Cursors.Default;

            MessageBox.Show($"Success: {success}\nFail: {fail}", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static PatchStatus PatchFile(FileInfo fi)
        {
            try
            {
                if (fi.Length < LengthRequirement)
                    return PatchStatus.FailedFileTooSmall;

                var bytes = File.ReadAllBytes(fi.FullName);

                var pointer = StartAddress;

                for (var i = pointer; i < pointer + FFCount; i++)
                    bytes[i] = 255;

                pointer += FFCount;

                for (var i = pointer; i < pointer + ZeroCount; i++)
                    bytes[i] = 0;

                File.WriteAllBytes(fi.FullName, bytes);

                return PatchStatus.Patched;
            }
            catch
            {
                return PatchStatus.UnknownFail;
            }
        }
    }
}