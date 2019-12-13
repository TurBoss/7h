using System;
using System.Reflection;
using System.Windows.Forms;

/// <summary>
/// Dialog to Select a Folder which resembles the UI of the <see cref="OpenFileDialog"/>
/// </summary>
/// <remarks>
/// code found from: https://stackoverflow.com/questions/11767/browse-for-a-directory-in-c-sharp
/// </remarks>
class OpenFolderDialog
{
    //public bool AddExtension { get; set; }
    public bool AutoUpgradeEnabled { get; set; }
    //public bool CheckFileExists { get; set; }
    public bool CheckPathExists { get; set; }
    //public bool DereferenceLinks { get; set; }
    public string Title { get; set; }
    //public string DefaultExt { get; set; }
    public string InitialDirectory { get; set; }
    //public bool ValidateNames { get; set; }
    //public bool SupportMultiDottedExtensions { get; set; }
    //public bool ShowHelp { get; set; }
    public bool Multiselect { get; set; }
    public bool RestoreDirectory { get; set; }
    //public string Filter { get; set; }
    public string SelectedPath { get; private set; }
    public string[] SelectedPaths { get; private set; }

    private FolderBrowserDialog FolderBrowser
    {
        get
        {
            return new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                Description = Title,
                SelectedPath = InitialDirectory
            };
        }
    }

    private OpenFileDialog Dialog
    {
        get
        {
            return new OpenFileDialog()
            {
                Title = Title,
                AddExtension = false,
                AutoUpgradeEnabled = AutoUpgradeEnabled,
                CheckFileExists = true,
                CheckPathExists = CheckPathExists,
                DefaultExt = string.Empty,
                DereferenceLinks = false,
                InitialDirectory = InitialDirectory,
                ValidateNames = false,
                SupportMultiDottedExtensions = false,
                ShowHelp = false,
                Multiselect = Multiselect,
                RestoreDirectory = RestoreDirectory,
                Filter = string.Empty,
                FileName = SelectedPath
            };
        }
    }

    /// <param name="hWndOwner">Handle of the control or window to be the parent of the file dialog</param>
    /// <returns>true if the user clicks OK</returns>
    public DialogResult ShowDialog(IntPtr hWndOwner)
    {
        if (Environment.OSVersion.Version.Major >= 6)
        {
            OpenFileDialog dialog = Dialog;
            DialogResult result = VistaDialog.Show(hWndOwner, dialog) != 0 ? DialogResult.Cancel : DialogResult.OK;
            SelectedPath = dialog.FileName;
            SelectedPaths = dialog.FileNames;
            return result;
        }
        else
        {
            FolderBrowserDialog XPDialog = FolderBrowser;
            DialogResult result = XPDialog.ShowDialog();
            SelectedPath = XPDialog.SelectedPath;
            return result;
        }
    }

    private static class VistaDialog
    {
        private const BindingFlags c_flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly static Assembly s_windowsFormsAssembly = typeof(FileDialog).Assembly;
        private readonly static Type s_iFileDialogType = s_windowsFormsAssembly.GetType("System.Windows.Forms.FileDialogNative+IFileDialog");
        private readonly static MethodInfo s_createVistaDialogMethodInfo = typeof(OpenFileDialog).GetMethod("CreateVistaDialog", c_flags);
        private readonly static MethodInfo s_onBeforeVistaDialogMethodInfo = typeof(OpenFileDialog).GetMethod("OnBeforeVistaDialog", c_flags);
        private readonly static MethodInfo s_getOptionsMethodInfo = typeof(FileDialog).GetMethod("GetOptions", c_flags);
        private readonly static MethodInfo s_setOptionsMethodInfo = s_iFileDialogType.GetMethod("SetOptions", c_flags);
        private readonly static uint s_fosPickFoldersBitFlag = (uint)s_windowsFormsAssembly
            .GetType("System.Windows.Forms.FileDialogNative+FOS")
            .GetField("FOS_PICKFOLDERS")
            .GetValue(null);
        private readonly static ConstructorInfo s_vistaDialogEventsConstructorInfo = s_windowsFormsAssembly
            .GetType("System.Windows.Forms.FileDialog+VistaDialogEvents")
            .GetConstructor(c_flags, null, new[] { typeof(FileDialog) }, null);
        private readonly static MethodInfo s_adviseMethodInfo = s_iFileDialogType.GetMethod("Advise");
        private readonly static MethodInfo s_unAdviseMethodInfo = s_iFileDialogType.GetMethod("Unadvise");
        private readonly static MethodInfo s_showMethodInfo = s_iFileDialogType.GetMethod("Show");

        public static int Show(IntPtr ownerHandle, OpenFileDialog dialog)
        {
            var iFileDialog = s_createVistaDialogMethodInfo.Invoke(dialog, new object[] { });
            s_onBeforeVistaDialogMethodInfo.Invoke(dialog, new[] { iFileDialog });
            s_setOptionsMethodInfo.Invoke(iFileDialog, new object[] { (uint)s_getOptionsMethodInfo.Invoke(dialog, new object[] { }) | s_fosPickFoldersBitFlag });
            var adviseParametersWithOutputConnectionToken = new[] { s_vistaDialogEventsConstructorInfo.Invoke(new object[] { dialog }), 0U };
            s_adviseMethodInfo.Invoke(iFileDialog, adviseParametersWithOutputConnectionToken);

            try
            {
                return (int)s_showMethodInfo.Invoke(iFileDialog, new object[] { ownerHandle });

            }
            finally
            {
                s_unAdviseMethodInfo.Invoke(iFileDialog, new[] { adviseParametersWithOutputConnectionToken[1] });
            }
        }
    }
}