using FirstFloor.ModernUI.Presentation;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace Wes.Desktop.Windows.Options.View
{
    /// <summary>
    /// A simple view model for configuring theme, font and accent colors.
    /// </summary>
    public class SettingsAppearanceViewModel
        : NotifyPropertyChanged
    {
        private const string PaletteMetro = "metro";
        private const string PaletteWP = "windows phone";

        // 9 accent colors from metro design principles
        private Color[] metroAccentColors = new Color[]{
            Color.FromRgb(0x33, 0x99, 0xff),   // blue
            Color.FromRgb(0x00, 0xab, 0xa9),   // teal
            Color.FromRgb(0x33, 0x99, 0x33),   // green
            Color.FromRgb(0x8c, 0xbf, 0x26),   // lime
            Color.FromRgb(0xf0, 0x96, 0x09),   // orange
            Color.FromRgb(0xff, 0x45, 0x00),   // orange red
            Color.FromRgb(0xe5, 0x14, 0x00),   // red
            Color.FromRgb(0xff, 0x00, 0x97),   // magenta
            Color.FromRgb(0xa2, 0x00, 0xff),   // purple            
        };

        // 20 accent colors from Windows Phone 8
        private Color[] wpAccentColors = new Color[]{
            Color.FromRgb(0xa4, 0xc4, 0x00),   // lime
            Color.FromRgb(0x60, 0xa9, 0x17),   // green
            Color.FromRgb(0x00, 0x8a, 0x00),   // emerald
            Color.FromRgb(0x00, 0xab, 0xa9),   // teal
            Color.FromRgb(0x1b, 0xa1, 0xe2),   // cyan
            Color.FromRgb(0x00, 0x50, 0xef),   // cobalt
            Color.FromRgb(0x6a, 0x00, 0xff),   // indigo
            Color.FromRgb(0xaa, 0x00, 0xff),   // violet
            Color.FromRgb(0xf4, 0x72, 0xd0),   // pink
            Color.FromRgb(0xd8, 0x00, 0x73),   // magenta
            Color.FromRgb(0xa2, 0x00, 0x25),   // crimson
            Color.FromRgb(0xe5, 0x14, 0x00),   // red
            Color.FromRgb(0xfa, 0x68, 0x00),   // orange
            Color.FromRgb(0xf0, 0xa3, 0x0a),   // amber
            Color.FromRgb(0xe3, 0xc8, 0x00),   // yellow
            Color.FromRgb(0x82, 0x5a, 0x2c),   // brown
            Color.FromRgb(0x6d, 0x87, 0x64),   // olive
            Color.FromRgb(0x64, 0x76, 0x87),   // steel
            Color.FromRgb(0x76, 0x60, 0x8a),   // mauve
            Color.FromRgb(0x87, 0x79, 0x4e),   // taupe
        };

        private string selectedPalette = OptionConfigureService.GetPalette("Appearance");

        private Color wesAccentColor;
        private LinkCollection themes = new LinkCollection();
        private Link wesTheme;
        private double selectedFontSize;

        public SettingsAppearanceViewModel()
        {
            this.themes.Add(new Link { DisplayName = "dark", Source = AppearanceManager.DarkThemeSource });
            this.themes.Add(new Link { DisplayName = "light", Source = AppearanceManager.LightThemeSource });
            this.themes.Add(new Link { DisplayName = "truck", Source = new Uri("/Wes.Desktop.Windows;component/Themes/ModernUI.Truck.xaml", UriKind.Relative) });
            this.themes.Add(new Link { DisplayName = "house", Source = new Uri("/Wes.Desktop.Windows;component/Themes/ModernUI.House.xaml", UriKind.Relative) });

            this.SelectedFontSize = FontSizeAppearance.FontSize;
            SyncThemeAndColor();

            AppearanceManager.Current.PropertyChanged += OnAppearanceManagerPropertyChanged;
        }

        private void SyncThemeAndColor()
        {
            this.SelectedTheme = this.themes.FirstOrDefault(l => l.Source.Equals(AppearanceManager.Current.ThemeSource)).Source.ToString();
            this.SelectedAccentColor = AppearanceManager.Current.AccentColor.String();
        }

        private void OnAppearanceManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ThemeSource" || e.PropertyName == "AccentColor")
            {
                SyncThemeAndColor();
            }
        }

        public LinkCollection Themes
        {
            get { return this.themes; }
        }

        public double[] FontSizes
        {
            get { return FontSizeAppearance.FontSizeList.ToArray(); }
        }

        public string[] Palettes
        {
            get { return new string[] { PaletteMetro, PaletteWP }; }
        }

        public Color[] AccentColors
        {
            get { return this.selectedPalette == PaletteMetro ? this.metroAccentColors : this.wpAccentColors; }
        }

        public string SelectedPalette
        {
            get { return this.selectedPalette; }
            set
            {
                if (this.selectedPalette != value)
                {
                    this.selectedPalette = value;
                    OnPropertyChanged("SelectedPalette");
                    OnPropertyChanged("AccentColors");
                    this.SelectedAccentColor = this.AccentColors.FirstOrDefault().String();
                }
            }
        }

        public Link WesTheme
        {
            get { return this.wesTheme; }
            set
            {
                if (this.wesTheme != value)
                {
                    this.wesTheme = value;
                    OnPropertyChanged("WesTheme");
                    AppearanceManager.Current.ThemeSource = value.Source;
                }
            }
        }

        public string SelectedTheme
        {
            get { return this.wesTheme.Source.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var theme = this.themes.FirstOrDefault(l => l.Source.ToString() == value);
                    if (theme != null)
                        this.WesTheme = theme;
                }
            }
        }

        public double SelectedFontSize
        {
            get { return this.selectedFontSize; }
            set
            {
                if (this.selectedFontSize != value && value > 0)
                {
                    this.selectedFontSize = value;
                    OnPropertyChanged("SelectedFontSize");
                    FontSizeAppearance.SetFontSize(value);
                }
            }
        }

        public Color WesAccentColor
        {
            get { return this.wesAccentColor; }
            set
            {
                if (this.wesAccentColor != value)
                {
                    this.wesAccentColor = value;
                    OnPropertyChanged("WesAccentColor");
                    AppearanceManager.Current.AccentColor = value;
                }
            }
        }

        public string SelectedAccentColor
        {
            get { return string.Format("{0},{1},{2}", this.wesAccentColor.R, this.wesAccentColor.G, this.wesAccentColor.B); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string[] values = value.ToString().Split(',');
                    if (values != null && values.Length == 3)
                    {
                        this.WesAccentColor = Color.FromRgb(Convert.ToByte(values[0]), Convert.ToByte(values[1]), Convert.ToByte(values[2]));
                    }
                }
            }
        }
    }
    public static class ColorExtender
    {
        public static string String(this Color color)
        {
            return string.Format("{0},{1},{2}", color.R, color.G, color.B);
        }
    }

}
