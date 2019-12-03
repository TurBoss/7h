using Iros._7th.Workshop;
using Microsoft.Win32;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.ViewModels
{
    public class InputTextViewModel : ViewModelBase
    {
        private string _windowTitle;
        private string _message;
        private string _textInput;

        public string WindowTitle
        {
            get
            {
                return _windowTitle;
            }
            set
            {
                _windowTitle = value;
                NotifyPropertyChanged();
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                NotifyPropertyChanged();
            }
        }

        public string TextInput
        {
            get
            {
                return _textInput;
            }
            set
            {
                _textInput = value;
                NotifyPropertyChanged();
            }
        }


        public InputTextViewModel()
        {
        }

    }
}
