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
    public class CheckboxMessageViewModel : ViewModelBase
    {
        private string _windowTitle;
        private string _message;
        private bool _isChecked;
        private string _checkboxText;


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

        public string CheckboxText
        {
            get
            {
                return _checkboxText;
            }
            set
            {
                _checkboxText = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                NotifyPropertyChanged();
            }
        }

        public MessageBoxResult Result { get; set; }


        public CheckboxMessageViewModel()
        {
            Result = MessageBoxResult.Cancel;
            IsChecked = false;
        }

    }
}
