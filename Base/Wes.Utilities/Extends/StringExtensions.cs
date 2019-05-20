using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wes.Utilities.Extends
{
    public static class StringExtensions
    {
        public static string Underline2Hump(this string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower()).Replace("_", "");
        }

        /// <summary>
        ///     A string extension method that return the right part of the string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="length">The length.</param>
        /// <returns>The right part.</returns>
        public static string Right(this string @this, int length)
        {
            return @this.Substring(@this.Length - length);
        }

        /// <summary>
        /// 轉換對象為string
        /// </summary>
        public static string ToString<T>(this T val)
        {
            if (typeof(T) == typeof(string))
            {
                string s = (string) (object) val;
                return string.IsNullOrEmpty(s) ? null : s;
            }

            try
            {
                TypeConverter c = TypeDescriptor.GetConverter(typeof(T));
                string s = c.ConvertToInvariantString(val);
                return string.IsNullOrEmpty(s) ? null : s;
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
                return null;
            }
        }

        public static bool IsPackageID(this String val)
        {
            Regex regexP = new Regex(@"^X\d{10}P\d{3}-C\d{3}$", RegexOptions.IgnoreCase);
            Regex regexC = new Regex(@"^X\d{10}C\d{3}$", RegexOptions.IgnoreCase);
            Regex regexP1 = new Regex(@"X\d{10}P\d{3}-C\d{3}-\d{2}$", RegexOptions.IgnoreCase);
            Regex regexP2 = new Regex(@"^X\d{10}C\d{3}-\d{2}$", RegexOptions.IgnoreCase);
            Regex regexTXT = new Regex(@"^TXT\d{10}C\d{3}$", RegexOptions.IgnoreCase);
            Regex regexSXT = new Regex(@"^SXT\d{10}C\d{3}$", RegexOptions.IgnoreCase);
            if (regexP.IsMatch(val) || regexC.IsMatch(val) || regexTXT.IsMatch(val) || regexSXT.IsMatch(val) || regexP1.IsMatch(val) || regexP2.IsMatch(val))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsReceivingPackageID(this String val)
        {
            Regex regexP = new Regex(@"^X\d{10}P\d{3}-C\d{3}$", RegexOptions.IgnoreCase);
            Regex regexC = new Regex(@"^X\d{10}C\d{3}$", RegexOptions.IgnoreCase);
            return regexP.IsMatch(val) || regexC.IsMatch(val);
        }


        public static bool IsReceivingPalletPackageID(this String val)
        {
            Regex regexP = new Regex(@"^X\d{10}P\d{3}-C\d{3}$", RegexOptions.IgnoreCase);
            return regexP.IsMatch(val);
        }

        public static string ToPalletNo(this String cartonNo)
        {
            return cartonNo.Contains("-") ? cartonNo.Split('-').FirstOrDefault() : cartonNo;
        }

        public static bool IsSxt(this String val)
        {
            Regex regexSXT = new Regex(@"^SXT\d{10}$", RegexOptions.IgnoreCase);
            return regexSXT.IsMatch(val);
        }

        public static bool IsTxt(this String val)
        {
            Regex regexTXT = new Regex(@"^TXT\d{10}$", RegexOptions.IgnoreCase);
            return regexTXT.IsMatch(val);
        }

        public static bool IsPxt(this String val)
        {
            Regex regexTXT = new Regex(@"^PXT\d{10}$", RegexOptions.IgnoreCase);
            return regexTXT.IsMatch(val);
        }

        public static bool IsRxt(this String val)
        {
            Regex regexRXT = new Regex(@"^RXT\d{10}$", RegexOptions.IgnoreCase);
            return regexRXT.IsMatch(val);
        }

        public static bool IsWorkNo(this String val)
        {
            if (val.IsPxt() || val.IsRxt() || val.IsSxt() || val.IsTxt() || val.IsPackageID())
                return true;
            return false;
        }

        public static bool IsPalletEnd(this String val)
        {
            if (val.ToUpper()=="PALLETEND")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 是否為原箱
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool IsOriginalPackage(this String val)
        {
            Regex regexp = new Regex(@"^X\d{10}P\d{3}-C\d{3}$", RegexOptions.IgnoreCase);
            Regex regexc = new Regex(@"^X\d{10}C\d{3}$", RegexOptions.IgnoreCase);
            if (regexp.IsMatch(val) || regexc.IsMatch(val))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否為非原箱
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool IsNotOriginalPackage(this String val)
        {
            return !(IsOriginalPackage(val));
        }
    }
}