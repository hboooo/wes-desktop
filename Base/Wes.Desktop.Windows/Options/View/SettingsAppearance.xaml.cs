namespace Wes.Desktop.Windows.Options.View
{
    public partial class SettingsAppearance : OptionTabControlBase, IOptionControl
    {
        public SettingsAppearance()
        {
            InitializeComponent();
            this.DataContext = new SettingsAppearanceViewModel();
            this.LoadOption();
        }

        protected override string ID => "Appearance";

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
