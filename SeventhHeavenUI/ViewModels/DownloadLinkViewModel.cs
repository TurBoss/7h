using Iros._7th.Workshop;
using System.Collections.Generic;

namespace SeventhHeavenUI.ViewModels
{

    public class DownloadLinkViewModel : ViewModelBase
    {
        private string _sourceLinkInput;
        private string _linkKindInput;
        private List<string> _linkKindList;

        public List<string> LinkKindList
        {
            get
            {
                if (_linkKindList == null)
                {
                    _linkKindList = new List<string>()
                    {
                        LocationType.GDrive.ToString(),
                        LocationType.MegaSharedFolder.ToString(),
                        LocationType.Url.ToString(),
                        LocationType.ExternalUrl.ToString()
                    };
                }

                return _linkKindList;
            }
        }

        public string SourceLinkInput
        {
            get
            {
                return _sourceLinkInput;
            }
            set
            {
                _sourceLinkInput = value;
                NotifyPropertyChanged();
            }
        }

        public string LinkKindInput
        {
            get
            {
                return _linkKindInput;
            }
            set
            {
                _linkKindInput = value;
                NotifyPropertyChanged();
            }
        }

        public string GetFormattedLink()
        {
            return $"iros://{LinkKindInput}/{SourceLinkInput?.Replace("://", "$")}";
        }


        public DownloadLinkViewModel(string linkKind, string link)
        {
            LinkKindInput = linkKind;
            SourceLinkInput = link;
        }
    }
}
