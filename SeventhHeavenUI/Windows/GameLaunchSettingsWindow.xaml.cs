using SeventhHeaven.Classes;
using SeventhHeaven.ViewModels;
using SeventhHeavenUI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for GameLaunchSettingsWindow.xaml
    /// </summary>
    public partial class GameLaunchSettingsWindow : Window
    {
        GameLaunchSettingsViewModel ViewModel { get; set; }

        public GameLaunchSettingsWindow()
        {
            InitializeComponent();

            ViewModel = new GameLaunchSettingsViewModel();
            this.DataContext = ViewModel;
        }

        private void btnTestAudio_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TestAudio(AudioChannel.Center);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SaveSettings())
            {
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Control_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((sender as FrameworkElement) != null)
            {
                ViewModel.StatusMessage = (sender as FrameworkElement).ToolTip as string;
            }
        }

        private void menuItemTestLeft_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TestAudio(AudioChannel.Left);
        }

        private void menuItemTestRight_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TestAudio(AudioChannel.Right);
        }

        private void menuAudioTest_Closed(object sender, RoutedEventArgs e)
        {
            btnAudioOptions.IsEnabled = true; // ensure button is re-enabled after context menu closes
        }

        private void btnAudioOptions_Click(object sender, RoutedEventArgs e)
        {
            if (!menuAudioTest.IsOpen)
            {
                menuAudioTest.PlacementTarget = btnAudioOptions;
                menuAudioTest.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menuAudioTest.IsOpen = true;
                btnAudioOptions.IsEnabled = false; // disable the button while the menu is open until context menu is closed
            }
        }

        private void btnAddProgram_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsProgramPopupOpen)
            {
                return; // dont do anything if popup opened already
            }

            ViewModel.AddNewProgram();
        }

        private void btnRemoveProgram_Click(object sender, RoutedEventArgs e)
        {
            if (lstPrograms.SelectedItem == null)
            {
                ViewModel.StatusMessage = "Selet a program to remove first.";
                return;
            }

            ViewModel.RemoveSelectedProgram((lstPrograms.SelectedItem as ProgramToRunViewModel));
        }

        private void btnEditProgram_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsProgramPopupOpen)
            {
                return; // dont do anything if popup opened already
            }

            if (lstPrograms.SelectedItem == null)
            {
                ViewModel.StatusMessage = "Select a program to edit first.";
                return;
            }

            ViewModel.EditSelectedProgram((lstPrograms.SelectedItem as ProgramToRunViewModel));
        }

        private void btnCancelProgramAction_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CloseProgramPopup();
        }

        private void btnSaveProgram_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveProgramToRun();
        }

        private void btnBrowseProgram_Click(object sender, RoutedEventArgs e)
        {
            string pathToProgram = FileDialogHelper.BrowseForFile("All files|*.*", "Select a program to run such as an .exe or script", ViewModel.NewProgramPathText);

            ViewModel.IsProgramPopupOpen = true; // opening file dialog closes popup so re-open it

            if (!string.IsNullOrWhiteSpace(pathToProgram))
            {
                ViewModel.NewProgramPathText = pathToProgram;
            }
        }

        private void Window_LocationChanged(object sender, System.EventArgs e)
        {
            ViewModel.IsProgramPopupOpen = false;
        }

        private void Window_Deactivated(object sender, System.EventArgs e)
        {
            ViewModel.IsProgramPopupOpen = false;
        }

        private void btnRefreshKeyboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyInputCfgToCustomCfg(forceCopy: true);
        }

        private void btnMoveProgramDown_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsProgramPopupOpen)
            {
                return; // dont do anything if popup opened already
            }

            if (lstPrograms.SelectedItem == null)
            {
                ViewModel.StatusMessage = "Select a program to move first.";
                return;
            }

            ViewModel.ChangeProgramOrder((lstPrograms.SelectedItem as ProgramToRunViewModel), +1);
        }

        private void btnMoveProgramUp_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsProgramPopupOpen)
            {
                return; // dont do anything if popup opened already
            }

            if (lstPrograms.SelectedItem == null)
            {
                ViewModel.StatusMessage = "Select a program to move first.";
                return;
            }

            ViewModel.ChangeProgramOrder((lstPrograms.SelectedItem as ProgramToRunViewModel), -1);
        }

        private void btnImportMovies_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ImportMissingMovies();
        }

        private void sliderSfxVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ShowOrHideToolTip(sliderSfxVolume.ToolTip as ToolTip, true);
        }

        private void sliderSfxVolume_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowOrHideToolTip(sliderSfxVolume.ToolTip as ToolTip, false);
        }

        private void ShowOrHideToolTip(ToolTip controlToolTip, bool isOpen)
        {
            if (controlToolTip != null && controlToolTip.Content != null)
            {
                controlToolTip.IsOpen = isOpen;
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ShowOrHideToolTip(sliderVolume.ToolTip as ToolTip, true);
        }

        private void sliderVolume_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowOrHideToolTip(sliderVolume.ToolTip as ToolTip, false);
        }

        private void btnMoveProgramUp_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel.IsProgramPopupOpen)
            {
                return; // dont do anything if popup opened already
            }

            if (lstPrograms.SelectedItem == null)
            {
                ViewModel.StatusMessage = "Select a program to move first.";
                return;
            }

            ViewModel.ChangeProgramOrder((lstPrograms.SelectedItem as ProgramToRunViewModel), 0 - lstPrograms.SelectedIndex);
        }

        private void btnMoveProgramDown_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel.IsProgramPopupOpen)
            {
                return; // dont do anything if popup opened already
            }

            if (lstPrograms.SelectedItem == null)
            {
                ViewModel.StatusMessage = "Select a program to move first.";
                return;
            }

            ViewModel.ChangeProgramOrder((lstPrograms.SelectedItem as ProgramToRunViewModel), (lstPrograms.Items.Count - 1) - lstPrograms.SelectedIndex);
        }
    }
}
