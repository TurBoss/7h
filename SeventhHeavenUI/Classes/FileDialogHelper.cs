using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeventhHeaven.Classes
{
    public static class FileDialogHelper
    {
        public static string BrowseForFile(string filter, string title = "Select File")
        {
            using (OpenFileDialog fileBrowserDialog = new OpenFileDialog())
            {
                fileBrowserDialog.Filter = filter;
                fileBrowserDialog.Title = title;

                DialogResult result = fileBrowserDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    return fileBrowserDialog.FileName;
                }
            }

            return "";
        }

        public static string BrowseForFolder(string description = "", bool multiselect = false)
        {
            OpenFolderDialog folder = new OpenFolderDialog()
            {
                Title = description,
                AutoUpgradeEnabled = true,
                CheckPathExists = true,
                Multiselect = multiselect,
                RestoreDirectory = true
            };
            DialogResult result = folder.ShowDialog(IntPtr.Zero);

            if (result.Equals(DialogResult.OK))
                return folder.SelectedPath;

            return "";
        }

        public static string OpenSaveDialog(string filter, string title = "Save New File")
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = filter;
                saveDialog.Title = title;

                DialogResult result = saveDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    return saveDialog.FileName;
                }
            }

            return "";
        }
    }
}
