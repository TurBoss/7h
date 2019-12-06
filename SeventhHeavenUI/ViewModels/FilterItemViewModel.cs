using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeavenUI.ViewModels
{

    public class FilterItemViewModel : ViewModelBase
    {
        private string _name;
        private bool _isChecked;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        public Action<bool> OnChecked { get; set; }

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
                OnChecked?.Invoke(_isChecked);
            }
        }

        public FilterItemViewModel(string name)
        {
            Name = name;
            IsChecked = false;
        }

        /// <summary>
        /// Sets <see cref="_isChecked"/> without invoking <see cref="OnChecked"/>
        /// </summary>
        public void SetIsChecked(bool isChecked)
        {
            _isChecked = isChecked;
            NotifyPropertyChanged(nameof(IsChecked));
        }
    }
}
