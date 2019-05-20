using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Wes.Desktop.Windows.Options.View;
using Wes.Utilities.Xml;

namespace Wes.Desktop.Windows.Options
{
    public class OptionConfigureService
    {
        public static string GetLanguageValue()
        {
            string lan = EnvironmetService.GetValue("Language", "Language");
            if (string.IsNullOrEmpty(lan))
            {
                lan = "zh-CHT";   //默认中文繁体
                SetLanguageValue(lan);
            }
            return lan;
        }

        public static void SetLanguageValue(string language)
        {
            EnvironmetService.AddValue("Language", "Language", language);
        }

        public static double GetFontSize(string id)
        {
            string val = EnvironmetService.GetValue(id, "FontSize");
            double.TryParse(val, out double size);
            if (size == 0) size = FontSizeAppearance.FontSize;
            return size;
        }

        public static string GetPalette(string id)
        {
            string palette = EnvironmetService.GetValue(id, "Palette");
            if (string.IsNullOrEmpty(palette))
            {
                palette = "metro";
                AddValue("Appearance", "Palette", palette);
            }
            return palette;
        }

        public static Color GetAccentColor(string id)
        {
            Color color = Color.FromRgb(0x33, 0x99, 0x33);
            string value = EnvironmetService.GetValue(id, "AccentColor");
            if (!string.IsNullOrEmpty(value))
            {
                string[] values = value.ToString().Split(',');
                color = Color.FromRgb(Convert.ToByte(values[0]), Convert.ToByte(values[1]), Convert.ToByte(values[2]));
            }
            else
            {
                AddValue("Appearance", "AccentColor", color.String());
            }
            return color;
        }

        public static string GetTheme(string id)
        {
            string theme = EnvironmetService.GetValue(id, "Theme");
            if (string.IsNullOrEmpty(theme))
            {
                theme = AppearanceManager.LightThemeSource.ToString();
                AddValue("Appearance", "Theme", theme);
            }
            return theme;
        }

        public static void WriteAddinName(string key, string addin)
        {
            EnvironmetService.AddWesValue(key, addin);
        }

        public static Dictionary<string, object> GetProperties(string id)
        {
            return EnvironmetService.GetKeyValues(id);
        }

        public static bool SetProperties(string id, Dictionary<string, object> properties)
        {
            return EnvironmetService.AddKeyValues(id, properties);
        }

        public static bool AddValue(string element, string name, string value)
        {
            return EnvironmetService.AddValue(element, name, value);
        }

        public static void DeleteElement(string element)
        {
            EnvironmetService.DeleteElement(element);
        }
    }
}
