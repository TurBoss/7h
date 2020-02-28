using Iros.Mega;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeaven.ViewModels
{
    class MegaLinkGenViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _folderIDInput;
        private string _linkOutput;
        private bool _isGenerating;

        public string FolderIDInput
        {
            get
            {
                return _folderIDInput;
            }
            set
            {
                _folderIDInput = value;
                NotifyPropertyChanged();
            }
        }

        public string LinkOutput
        {
            get
            {
                return _linkOutput;
            }
            set
            {
                _linkOutput = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsGenerating
        {
            get
            {
                return _isGenerating;
            }
            set
            {
                _isGenerating = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotGenerating));
            }
        }

        public bool IsNotGenerating
        {
            get
            {
                return !_isGenerating;
            }
        }

        public MegaLinkGenViewModel()
        {
            FolderIDInput = "";
            LinkOutput = "";
        }

        public Task GenerateLinksAsync()
        {
            IsGenerating = true;
            LinkOutput = "Generating links ...";

            Task t = Task.Factory.StartNew(() =>
            {
                GenerateLinks();
            });

            t.ContinueWith((result) =>
            {
                IsGenerating = false;

                if (result.IsFaulted)
                {
                    LinkOutput = $"Failed to generate links: {result.Exception.GetBaseException()?.Message}.";
                    Logger.Warn(result.Exception.GetBaseException());
                }
            });

            return t;
        }

        public void GenerateLinks()
        {
            string megaFolderId = FolderIDInput;

            if (string.IsNullOrWhiteSpace(megaFolderId))
            {
                LinkOutput = $"Enter a Mega Folder ID to generate links.";
                return;
            }

            MegaIros mega = new MegaIros(megaFolderId, String.Empty);

            // wait up to 15 seconds to get nodes from mega
            for (int i = 0; i < 15; i++)
            {
                System.Threading.Thread.Sleep(1000);

                // break out of loop if got nodes
                if (mega.GetNodes().Any()) break;
            }

            if (!mega.GetNodes().Any())
            {
                LinkOutput = $"No links found in folder: {megaFolderId}";
                return;
            }

            LinkOutput = "";
            foreach (MegaIros.IrosNode n in mega.GetNodes())
            {
                LinkOutput += String.Format("iros://MegaSharedFolder/{0},{1},{2}\r\n", megaFolderId, Iros.Mega.Base64.btoa(n.Handle), n.Name);
            }
        }
    }
}
