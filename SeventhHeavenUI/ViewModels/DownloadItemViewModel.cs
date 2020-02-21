using _7thHeaven.Code;
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


    public class DownloadItemViewModel : ViewModelBase
    {
        private bool _isSelected;

        public DownloadItem Download { get; set; }

        public string ItemName
        {
            get
            {
                return Download.ItemName;
            }
            set
            {
                if (value != Download.ItemName)
                {
                    Download.ItemName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double PercentComplete
        {
            get
            {
                return Download.PercentComplete;
            }
            set
            {
                if (value != Download.PercentComplete)
                {
                    Download.PercentComplete = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string DownloadSpeed
        {
            get
            {
                return Download.DownloadSpeed;
            }
            set
            {
                Download.DownloadSpeed = value;
                NotifyPropertyChanged();
            }
        }

        public string RemainingTime
        {
            get
            {
                return Download.RemainingTime;
            }
            set
            {
                Download.RemainingTime = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged();
            }
        }


        public DownloadItemViewModel(DownloadItem download)
        {
            this.Download = download;
        }

    }
}
