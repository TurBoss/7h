using Iros._7th;
using Iros._7th.Workshop;
using SeventhHeaven.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        public ConfigureGLWindow()
        {
            InitializeComponent();

            ViewModels = new List<GLSettingViewModel>();
        }


        public void Init(string cfgSpec, string cfgFile)
        {
            _settings = new Iros._7th.Workshop.ConfigSettings.Settings(System.IO.File.ReadAllLines(cfgFile));
            _spec = Util.Deserialize<Iros._7th.Workshop.ConfigSettings.ConfigSpec>(cfgSpec);
            _file = cfgFile;

            foreach (var items in _spec.Settings.GroupBy(s => s.Group))
            {
                TabItem tab = new TabItem()
                {
                    Header = items.Key,
                };

                StackPanel stackPanel = new StackPanel()
                {
                    Margin = new Thickness(0, 5, 0, 0)
                };

                foreach (Iros._7th.Workshop.ConfigSettings.Setting setting in items)
                {
                    GLSettingViewModel settingViewModel = new GLSettingViewModel(setting, _settings);

                    ContentControl settingControl = new ContentControl();
                    settingControl.DataContext = settingViewModel;

                    ViewModels.Add(settingViewModel);
                    stackPanel.Children.Add(settingControl);
                }

                tab.Content = stackPanel;
                tabCtrlMain.Items.Add(tab);
            }
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

                System.IO.File.WriteAllLines(_file, _settings.GetOutput());
                Sys.Message(new WMessage("OpenGL settings saved!"));
                this.Close();
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("Could not write to ff7_opengl.cfg file. Check that it is not set to read only, and that FF7 is installed in a folder you have full write access to.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Sys.Message(new WMessage("Unknown error while saving. error has been logged."));
            }
        }
    }
}
