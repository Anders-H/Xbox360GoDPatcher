using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Xbox360GoDPatcher
{
    public partial class MainWindow : Form
    {
        private const int StartAddress = 556;
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

        private void selectNotPatchedToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            listView1.SelectedItems.Clear();
            foreach (ListViewItem i in listView1.Items)
            {
                var status = (PatchStatus)i.SubItems[1].Tag;

                if (status == PatchStatus.NotPatched)
                    i.Selected = true;
            }
        }

        private void patchToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var success = 0;
            var fail = 0;
            Cursor = Cursors.WaitCursor;

            foreach (ListViewItem i in listView1.SelectedItems)
            {
                var fi = (FileInfo)i.Tag;
                i.EnsureVisible();

                var result = PatchFile(fi);

                i.SubItems[1].Tag = result;
                i.SubItems[1].Text = PatchStatusAsString(result);

                listView1.Refresh();
            }

            Cursor = Cursors.Default;
        }
    }
}