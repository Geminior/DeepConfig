namespace DeepConfigEditor.FileExplorer
{
    using System;
    using System.Windows.Forms;

    public partial class FileSelection : UserControl
    {
        public FileSelection()
        {
            InitializeComponent();
        }

        void OnFileSelected(string filename)
        {
            MessageBox.Show(filename);
        }

        void UpdateOpenButtonState()
        {
            //openButton.Enabled = (shellView.SelectedItems.Length > 0) ||
            //                     (fileNameCombo.Text.Length > 0);
        }

        void fileNameCombo_TextChanged(object sender, EventArgs e)
        {
            UpdateOpenButtonState();
        }

        void shellView_DoubleClick(object sender, EventArgs e)
        {
            OnFileSelected(shellView.SelectedItems[0].FileSystemPath);
        }

        void shellView_SelectionChanged(object sender, EventArgs e)
        {
            UpdateOpenButtonState();
        }

        void fileNameCombo_FilenameEntered(object sender, EventArgs e)
        {
            OnFileSelected(fileNameCombo.Text);
        }

        //void openButton_Click(object sender, EventArgs e)
        //{
        //    if (!shellView.NavigateSelectedFolder())
        //    {
        //        ShellItem[] selected = shellView.SelectedItems;

        //        if (selected.Length > 0)
        //        {
        //            OnFileSelected(selected[0].FileSystemPath);
        //        }
        //        else if (File.Exists(fileNameCombo.Text))
        //        {
        //            OnFileSelected(fileNameCombo.Text);
        //        }
        //    }
        //}
    }
}
