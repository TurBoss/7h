using SeventhHeaven.ViewModels;
using System.Windows;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for InputTextWindow.xaml
    /// </summary>
    public partial class AllowModToRunWindow : Window
    {
        public AllowModToRunViewModel ViewModel { get; set; }

        public AllowModToRunWindow(string message)
        {
            InitializeComponent();

            ViewModel = new AllowModToRunViewModel()
            {
                Message = message,
                WindowTitle = "Allow Mod To Run?",
                CheckboxVisibility = Visibility.Collapsed,
            };
            this.DataContext = ViewModel;
            btnOkay.Focus();
        }

        private void btnOkay_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Result = MessageBoxResult.OK;
            this.DialogResult = true;
            this.Close();
        }

        private void gridMain_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double padding = 10;
            this.Height = gridMain.ActualHeight + padding;
        }
    }
}
