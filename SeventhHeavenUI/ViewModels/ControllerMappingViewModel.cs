using SeventhHeaven.Classes;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeaven.ViewModels
{
    public class ControllerMappingViewModel : ViewModelBase
    {
        private string _okControllerText;
        private bool _isCapturing;
        private GameControl _controlToCapture;
        private string _okKeyboardText;
        private string _cancelKeyboardText;
        private string _menuKeyboardText;
        private string _switchKeyboardText;
        private string _pageUpKeyboardText;
        private string _pageDownKeyboardText;

        private ControlConfiguration LoadedConfiguration { get; set; }

        public string OkControllerText
        {
            get
            {
                return _okControllerText;
            }
            set
            {
                _okControllerText = value;
                NotifyPropertyChanged();
            }
        }

        public string OkKeyboardText
        {
            get
            {
                return _okKeyboardText;
            }
            set
            {
                _okKeyboardText = value;
                NotifyPropertyChanged();
            }
        }


        public string CancelKeyboardText
        {
            get
            {
                return _cancelKeyboardText;
            }
            set
            {
                _cancelKeyboardText = value;
                NotifyPropertyChanged();
            }
        }

        public string MenuKeyboardText
        {
            get
            {
                return _menuKeyboardText;
            }
            set
            {
                _menuKeyboardText = value;
                NotifyPropertyChanged();
            }
        }

        public string SwitchKeyboardText
        {
            get
            {
                return _switchKeyboardText;
            }
            set
            {
                _switchKeyboardText = value;
                NotifyPropertyChanged();
            }
        }

        public string PageUpKeyboardText
        {
            get
            {
                return _pageUpKeyboardText;
            }
            set
            {
                _pageUpKeyboardText = value;
                NotifyPropertyChanged();
            }
        }

        public string PageDownKeyboardText
        {
            get
            {
                return _pageDownKeyboardText;
            }
            set
            {
                _pageDownKeyboardText = value;
                NotifyPropertyChanged();
            }
        }


        public bool IsCapturing
        {
            get
            {
                return _isCapturing;
            }
            set
            {
                _isCapturing = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotCapturing));
            }
        }

        public bool IsNotCapturing
        {
            get
            {
                return !_isCapturing;
            }
        }

        public ControllerMappingViewModel()
        {
            LoadedConfiguration = new ControlConfiguration();
        }

        internal void SetControlIfCapturing(System.Windows.Input.Key key)
        {
            if (!IsCapturing)
            {
                return;
            }

            LoadedConfiguration.Set(_controlToCapture, key);

            UpdateButtonText();

            IsCapturing = false;
        }

        private void UpdateButtonText()
        {
            if (LoadedConfiguration.KeyboardInputs.ContainsKey(GameControl.Ok))
            {
                OkKeyboardText = LoadedConfiguration.KeyboardInputs[GameControl.Ok]?.DisplayText;
            }

            if (LoadedConfiguration.KeyboardInputs.ContainsKey(GameControl.Cancel))
            {
                CancelKeyboardText = LoadedConfiguration.KeyboardInputs[GameControl.Cancel]?.DisplayText;
            }

            if (LoadedConfiguration.KeyboardInputs.ContainsKey(GameControl.Menu))
            {
                MenuKeyboardText = LoadedConfiguration.KeyboardInputs[GameControl.Menu]?.DisplayText;
            }

            if (LoadedConfiguration.KeyboardInputs.ContainsKey(GameControl.Switch))
            {
                SwitchKeyboardText = LoadedConfiguration.KeyboardInputs[GameControl.Switch]?.DisplayText;
            }

            if (LoadedConfiguration.KeyboardInputs.ContainsKey(GameControl.Pageup))
            {
                PageUpKeyboardText = LoadedConfiguration.KeyboardInputs[GameControl.Pageup]?.DisplayText;
            }

            if (LoadedConfiguration.KeyboardInputs.ContainsKey(GameControl.Pagedown))
            {
                PageDownKeyboardText = LoadedConfiguration.KeyboardInputs[GameControl.Pagedown]?.DisplayText;
            }
        }

        internal void StartCapturingInput(GameControl controlToCapture)
        {
            IsCapturing = true;
            _controlToCapture = controlToCapture;
        }
    }
}
