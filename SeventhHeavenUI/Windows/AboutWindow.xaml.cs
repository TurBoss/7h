using SeventhHeaven.ViewModels;
using SeventhHeavenUI;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for InputTextWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            this.Title = $"About {App.GetAppName()} v{App.GetAppVersion().ToString()}";
            btnClose.Focus();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }
}
