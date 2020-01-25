using Iros._7th;
using Iros._7th.Workshop;
using SeventhHeaven.Classes;
using SeventhHeaven.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for ConfigureGLWindow.xaml
    /// </summary>
    public partial class ConfigureGLWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private Iros._7th.Workshop.ConfigSettings.Settings _settings;
        private Iros._7th.Workshop.ConfigSettings.ConfigSpec _spec;
        private string _file;

        private List<GLSettingViewModel> ViewModels { get; set; }

        private Button btnClearTextureCache;

        public ConfigureGLWindow()
        {
            InitializeComponent();

            ViewModels = new List<GLSettingViewModel>();
        }

        public void SetStatusMessage(string message)
        {
            txtStatusMessage.Text = message;
        }


        public void Init(string cfgSpec, string cfgFile)
        {
            _file = cfgFile;

            try
            {
                _spec = Util.Deserialize<Iros._7th.Workshop.ConfigSettings.ConfigSpec>(cfgSpec);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                MessageDialogWindow.Show("Failed to read the required spec xml file to display settings. The window mus close.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            if (!File.Exists(_file))
            {
                // copy default .cfg file if missing
                File.Copy(Path.Combine(Sys.PathToGameDriverFolder, "7H_GameDriver.cfg"), _file, true);
            }

            _settings = new Iros._7th.Workshop.ConfigSettings.Settings(File.ReadAllLines(_file));
            _settings.SetMissingDefaults(_spec.Settings);

            Dictionary<string, int> tabOrders = new Dictionary<string, int>()
            {
                {"Graphics", 0},
                {"Rendering", 1},
                {"Advanced", 3}
            };

            foreach (var items in _spec.Settings.GroupBy(s => s.Group)
                                                .Select(g => new { settingGroup = g, SortOrder = tabOrders[g.Key] })
                                                .OrderBy(g => g.SortOrder)
                                                .Select(g => g.settingGroup))
            {
                TabItem tab = new TabItem()
                {
                    Header = items.Key,
                };

                StackPanel stackPanel = new StackPanel()
                {
                    Margin = new Thickness(0, 5, 0, 0)
                };

                ScrollViewer scrollViewer = new ScrollViewer()
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };

                foreach (Iros._7th.Workshop.ConfigSettings.Setting setting in items)
                {
                    GLSettingViewModel settingViewModel = new GLSettingViewModel(setting, _settings);

                    ContentControl settingControl = new ContentControl();
                    settingControl.DataContext = settingViewModel;
                    settingControl.MouseEnter += SettingControl_MouseEnter;

                    ViewModels.Add(settingViewModel);
                    stackPanel.Children.Add(settingControl);
                }

                if (items.Key == "Advanced")
                {
                    // add clear texture cache button
                    btnClearTextureCache = new Button()
                    {
                        Content = "Clear Texture Cache",
                        ToolTip = $"Will delete everything under {Path.Combine(Sys.Settings.AaliFolder, "cache")}",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5, 0, 0, 0)
                    };
                    btnClearTextureCache.Click += BtnClearTextureCache_Click;

                    stackPanel.Children.Add(btnClearTextureCache);
                }


                scrollViewer.Content = stackPanel;
                tab.Content = scrollViewer;
                tabCtrlMain.Items.Add(tab);
            }
        }

        private void BtnClearTextureCache_Click(object sender, RoutedEventArgs e)
        {
            ClearTextureCache();
        }

        private void ClearTextureCache()
        {
            string pathToCache = Path.Combine(Sys.Settings.AaliFolder, "cache");

            if (!Directory.Exists(pathToCache))
            {
                SetStatusMessage("Path to cache folder does not exist.");
                return;
            }

            try
            {
                foreach (string file in Directory.GetFiles(pathToCache, "*.ctx", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException ex)
                    {
                        Logger.Warn(ex); // add warning to log if fail to delete file but continue deleting other files
                    }
                }

                // delete empty folders recursively
                FileUtils.DeleteEmptyFolders(pathToCache);

                SetStatusMessage("Texture cache cleared!");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                SetStatusMessage($"Failed to clear texture cache: {ex.Message}");
            }
        }

        private void SettingControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            GLSettingViewModel viewModel = ((ContentControl)sender)?.DataContext as GLSettingViewModel;

            if (viewModel == null)
            {
                return;
            }

            SetStatusMessage(viewModel.Description);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (GLSettingViewModel item in ViewModels)
                {
                    item.Save(_settings);
                }

                File.WriteAllLines(_file, _settings.GetOutput());

                Sys.Message(new WMessage("Game Driver settings saved!"));
                this.Close();
            }
            catch (UnauthorizedAccessException)
            {
                MessageDialogWindow.Show("Could not write to 7H_GameDriver.cfg file. Check that it is not set to read only, and that FF7 is installed in a folder you have full write access to.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                SetStatusMessage("Unknown error while saving. error has been logged.");
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ViewModels)
            {
                item.ResetToDefault(_settings);
            }

            SetStatusMessage("Game Driver settings reset to defaults. Click 'Save' to save changes.");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (btnClearTextureCache != null)
            {
                btnClearTextureCache.Click -= BtnClearTextureCache_Click;
            }
        }
    }
}
