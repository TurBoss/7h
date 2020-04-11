using SeventhHeaven.Classes;
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
    /// Interaction logic for ControlMappingWindow.xaml
    /// </summary>
    public partial class ControlMappingWindow : Window
    {
        public ControllerMappingViewModel ViewModel { get; set; }

        public ControlMappingWindow()
        {
            InitializeComponent();

            ViewModel = new ControllerMappingViewModel();
            this.DataContext = ViewModel;
        }

        private void btnOkKeyboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartCapturingInput(GameControl.Ok);
        }

        private void btnOkController_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartCapturingInput(GameControl.Ok);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System)
            {
                ViewModel.SetControlIfCapturing(e.SystemKey);

            }
            else
            {
                ViewModel.SetControlIfCapturing(e.Key);
            }
        }

        private void btnCancelKeyboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartCapturingInput(GameControl.Cancel);
        }

        private void btnMenuKeyboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartCapturingInput(GameControl.Menu);
        }

        private void btnSwitchKeyboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartCapturingInput(GameControl.Switch);
        }

        private void btnPageUpKeyboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartCapturingInput(GameControl.Pageup);
        }

        private void btnPageDownKeyboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartCapturingInput(GameControl.Pagedown);
        }
    }
}
