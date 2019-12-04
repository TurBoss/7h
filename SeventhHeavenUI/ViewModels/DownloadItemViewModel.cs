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
    public enum DownloadMode
    {
        Download,
        Install,
    }

    public class DownloadItemViewModel : ViewModelBase
    {
        private string _itemName;
        private double _percentComplete;
        private bool _isSelected;
        private string _downloadSpeed;

        public Guid UniqueId { get; set; }

        public Install.InstallProcedure IProc { get; set; }

        public DateTime LastCalc { get; set; }
        public long LastBytes { get; set; }

        public Action PerformCancel { get; set; }
        public Action OnCancel { get; set; }

        public Action OnError { get; set; }


        public string ItemName
        {
            get
            {
                return _itemName;
            }
            set
            {
                _itemName = value;
                NotifyPropertyChanged();
            }
        }

        public double PercentComplete
        {
            get
            {
                return _percentComplete;
            }
            set
            {
                _percentComplete = value;
                NotifyPropertyChanged();
            }
        }

        public string DownloadSpeed
        {
            get
            {
                return _downloadSpeed;
            }
            set
            {
                _downloadSpeed = value;
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




        public DownloadItemViewModel()
        {
            LastCalc = DateTime.Now;
            UniqueId = Guid.NewGuid();
            PercentComplete = 0;
        }

    }
}
