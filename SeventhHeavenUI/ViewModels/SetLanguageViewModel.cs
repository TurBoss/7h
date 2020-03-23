using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using SeventhHeaven.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeavenUI.ViewModels
{

    public class SetLanguageViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _selectedLanguage;
        private Dictionary<string, string> _languagesMap;

        public List<string> Languages
        {
            get
            {
                return _languagesMap?.Keys.ToList();
            }
        }

        public Dictionary<string, string> LanguagesMap
        {
            get
            {
                return _languagesMap;
            }
            set
            {
                _languagesMap = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Languages));
            }
        }

        public string SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            set
            {
                _selectedLanguage = value;
                NotifyPropertyChanged();
            }
        }


        public SetLanguageViewModel()
        {
            LanguagesMap = new Dictionary<string, string>();
            LanguagesMap.Add($"{ResourceHelper.Get(StringKey.German)} (Deutsche)", "de");
            LanguagesMap.Add($"{ResourceHelper.Get(StringKey.English)} (English)", "en");
            LanguagesMap.Add($"{ResourceHelper.Get(StringKey.Spanish)} (Español)", "es");
            LanguagesMap.Add($"{ResourceHelper.Get(StringKey.French)} (Français)", "fr");
            LanguagesMap.Add($"{ResourceHelper.Get(StringKey.Japanese)} (\u65e5\u672c\u8a9e)", "ja");


            string defaultLang = ConfigurationManager.AppSettings["DefaultAppLanguage"];

            if (string.IsNullOrWhiteSpace(defaultLang))
            {
                SelectedLanguage = LanguagesMap.Where(kv => kv.Value == "en").Select(k => k.Key).FirstOrDefault();
            }
            else if (defaultLang.Length >= 2)
            {
                defaultLang = defaultLang.Substring(0, 2);
                SelectedLanguage = LanguagesMap.Where(kv => kv.Value == defaultLang).Select(k => k.Key).FirstOrDefault();
            }
        }

        internal bool SaveSelectedLanguageAsDefault()
        {
            try
            {
                Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (!appConfig.AppSettings.Settings.AllKeys.Any(k => k == "DefaultAppLanguage"))
                {
                    appConfig.AppSettings.Settings.Add("DefaultAppLanguage", LanguagesMap[SelectedLanguage]);
                }
                else
                {
                    appConfig.AppSettings.Settings["DefaultAppLanguage"].Value = LanguagesMap[SelectedLanguage];
                }

                appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                (App.Current as SeventhHeavenUI.App).SetLanguageDictionary(LanguagesMap[SelectedLanguage]);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }


            return true;
        }
    }
}
