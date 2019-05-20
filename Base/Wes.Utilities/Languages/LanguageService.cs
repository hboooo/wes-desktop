using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;
using Wes.Utilities.Exception;

namespace Wes.Utilities.Languages
{
    public class LanguageService
    {
        public static string GetLanguages(string key)
        {
            string lan = null;
            object obj = Application.Current.TryFindResource(key);
            if (obj != null) lan = obj.ToString();
            if (string.IsNullOrEmpty(lan)) lan = key;
            return lan;
        }

        public static string GetLanguageByTypeName(string typeName, string key)
        {
            LoadLanguageResources(typeName);
            return GetLanguages(key);
        }

        public static void ChangeLanguages(string language)
        {
            CultureInfo ci = new CultureInfo(language);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            LoadLanguageResources();
            if (LanguageChanged != null)
            {
                LanguageChanged(null, null);
            }
        }

        /// <summary>
        /// 加载资源文件
        /// </summary>
        private static void LoadLanguageResources(string typeName = null)
        {

            string dir = null;
            if (!string.IsNullOrWhiteSpace(typeName))
                dir = Path.Combine(Environment.CurrentDirectory, "Languages", CultureInfo.CurrentCulture.Name, typeName);
            else
                dir = Path.Combine(Environment.CurrentDirectory, "Languages", CultureInfo.CurrentCulture.Name);

            string[] files = Directory.GetFiles(dir, "*.xaml", SearchOption.AllDirectories);
            ResourceDictionary resourceDictionary = null;
            foreach (var item in files)
            {
                try
                {
                    resourceDictionary = new ResourceDictionary();
                    resourceDictionary.BeginInit();
                    resourceDictionary.Source = new Uri(item);
                    resourceDictionary.EndInit();
                    Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                    Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

        }

        public static EventHandler<EventArgs> LanguageChanged;
    }
}
