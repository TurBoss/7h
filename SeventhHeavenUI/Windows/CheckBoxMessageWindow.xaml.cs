using SeventhHeaven.ViewModels;
using System.Windows;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for InputTextWindow.xaml
    /// </summary>
    public partial class CheckBoxMessageWindow : Window
    {
        public CheckboxMessageViewModel ViewModel { get; set; }

        public CheckBoxMessageWindow()
        {
            InitializeComponent();

            ViewModel = new CheckboxMessageViewModel();
            this.DataContext = ViewModel;
            btnOkay.Focus();
        }

        public CheckBoxMessageWindow(string windowTitle, string prompt, MessageBoxButton buttons, string checkText = "Don't ask me again", bool isChecked = false)
        {
            InitializeComponent();

            ViewModel = new CheckboxMessageViewModel()
            {
                WindowTitle = windowTitle,
                Message = prompt,
                CheckboxText = checkText,
                IsChecked = isChecked
            };

            if (buttons == MessageBoxButton.YesNo)
            {
                btnOkay.Visibility = Visibility.Hidden;
            }
            else
            {
                btnYes.Visibility = Visibility.Hidden;
                btnNo.Visibility = Visibility.Hidden;
            }

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

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Result = MessageBoxResult.Yes;
            this.DialogResult = true;
            this.Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Result = MessageBoxResult.No;
            this.DialogResult = false;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double padding = 10;
            this.Height = gridMain.ActualHeight + padding;
        }
    }
}
