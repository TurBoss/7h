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

    public class SubscriptionSettingViewModel : ViewModelBase
    {
        private string _name;
        private string _url;



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

        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                NotifyPropertyChanged();
            }
        }


        public SubscriptionSettingViewModel(string url, string name)
        {
            Url = url;
            Name = name;
        }
    }
}
