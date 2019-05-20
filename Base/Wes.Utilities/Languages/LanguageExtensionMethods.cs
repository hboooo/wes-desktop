using System;

namespace Wes.Utilities.Languages
{
    public static class LanguageExtensionMethods
    {
        public static string GetLanguage(this string wesFlowName)
        {
            return LanguageService.GetLanguages(wesFlowName);
        }

        public static string GetLanguage(this Enum wesName)
        {
            string language = LanguageService.GetLanguages(wesName.GetType().FullName + "." + wesName.GetType().GetField(wesName.ToString()).Name);
            if (string.IsNullOrEmpty(language)) language = wesName.GetType().GetField(wesName.ToString()).Name;
            return language;
        }
    }
}
