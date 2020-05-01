using _7thHeaven.Code;
using SeventhHeaven.Classes;

namespace SeventhHeavenUI.ViewModels
{


    public class DownloadItemViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _isCancelling;

        public DownloadItem Download { get; set; }

        public string ItemName
        {
            get
            {
                return Download?.ItemName;
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

        public bool IsCancelling
        {
            get
            {
                return _isCancelling;
            }
            set
            {
                _isCancelling = value;
                NotifyPropertyChanged();
            }
        }

        public DownloadItemViewModel(DownloadItem download)
        {
            // translate hard coded english words from DownloadItem constructor
            if (download.RemainingTime == "Unknown")
            {
                download.RemainingTime = ResourceHelper.Get(StringKey.Unknown);
            }

            if (download.DownloadSpeed == "Pending...")
            {
                download.DownloadSpeed = ResourceHelper.Get(StringKey.Pending);
            }

            this.Download = download;
            IsCancelling = false;
        }

    }
}
