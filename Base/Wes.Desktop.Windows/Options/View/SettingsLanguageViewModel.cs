using FirstFloor.ModernUI.Presentation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wes.Utilities.Languages;

namespace Wes.Desktop.Windows.Options.View
{
    public class SettingsLanguageViewModel : NotifyPropertyChanged
    {
        private ObservableCollection<KeyValuePair<string, string>> _languages = new ObservableCollection<KeyValuePair<string, string>>();

        public ObservableCollection<KeyValuePair<string, string>> Languages
        {
            get { return _languages; }
        }

        private string _selectedLanguage;

        public string SelectedLanguage
        {
            get { return this._selectedLanguage; }
            set
            {
                if (this._selectedLanguage != value)
                {
                    this._selectedLanguage = value;
                    OnPropertyChanged(nameof(SelectedLanguage));
                    LanguageService.ChangeLanguages(this.SelectedLanguage);
                }
            }
        }

        public SettingsLanguageViewModel()
        {
            InitLanguages();
        }

        private void InitLanguages()
        {
            _languages.Clear();
            _languages.Add(new KeyValuePair<string, string>("中文简体", "zh-CN"));
            _languages.Add(new KeyValuePair<string, string>("中文繁體", "zh-CHT"));
            _languages.Add(new KeyValuePair<string, string>("English", "en-US"));
        }

    }
}
