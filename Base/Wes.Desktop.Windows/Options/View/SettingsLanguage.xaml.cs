namespace Wes.Desktop.Windows.Options.View
{
    /// <summary>
    /// SettingsLanguage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsLanguage : OptionTabControlBase, IOptionControl
    {
        public SettingsLanguage()
        {
            InitializeComponent();
            this.DataContext = new SettingsLanguageViewModel();
            this.LoadOption();
        }

        protected override string ID => "Language";

        public object Control => this;

        public void LoadOption()
        {
            LoadPropertyValue();
        }

        public bool SaveOption()
        {
            return SavePropertyValue();
        }
    }
}
