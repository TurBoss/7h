using SeventhHeaven.ViewModels;
using SeventhHeavenUI;
using System.Windows;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for InputTextWindow.xaml
    /// </summary>
    public partial class MessageDialogWindow : Window
    {
        public MessageDialogViewModel ViewModel { get; set; }

        public MessageDialogWindow()
        {
            InitializeComponent();

            ViewModel = new MessageDialogViewModel();
            this.DataContext = ViewModel;
            btnOkay.Focus();
        }

        public MessageDialogWindow(string windowTitle, string prompt, MessageBoxButton buttons, MessageBoxImage image)
        {
            InitializeComponent();

            ViewModel = new MessageDialogViewModel()
            {
                WindowTitle = windowTitle,
                Message = prompt,
                ImageToDisplay = image,
                CheckboxVisibility = Visibility.Collapsed,
            };

            if (buttons == MessageBoxButton.YesNo)
            {
                btnOkay.Visibility = Visibility.Hidden;
                btnOkay.Focus();
            }
            else
            {
                btnYes.Visibility = Visibility.Hidden;
                btnNo.Visibility = Visibility.Hidden;
                btnYes.Focus();
            }

            this.DataContext = ViewModel;
        }

        public MessageDialogWindow(string windowTitle, string prompt, MessageBoxButton buttons, string checkText = "Don't ask me again", bool isChecked = false)
        {
            InitializeComponent();

            ViewModel = new MessageDialogViewModel()
            {
                WindowTitle = windowTitle,
                Message = prompt,
                CheckboxText = checkText,
                CheckboxVisibility = Visibility.Visible,
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

        public static MessageDialogViewModel Show(string prompt, string windowTitle, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            return App.Current.Dispatcher.Invoke(() =>
            {
                MessageDialogWindow window = new MessageDialogWindow(windowTitle, prompt, buttons, image);
                window.ShowDialog();

                return window.ViewModel;
            });
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

        /// <summary>
        /// Increase the height of the Window if message is long
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double padding = 75; // this is to account for margins between controls

            double newHeight = txtMessage.ActualHeight + btnOkay.ActualHeight + padding;

            if (ViewModel.CheckboxVisibility == Visibility.Visible)
            {
                newHeight += chkOption.ActualHeight;
            }

            if (newHeight > this.Height)
            {
                if (newHeight >= this.MaxHeight)
                {
                    newHeight = this.MaxHeight;
                }

                this.Height = newHeight;
            }
        }
    }
}
