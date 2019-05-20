using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Wes.Utilities
{
    public class DesktopVersion
    {
        /// <summary>
        /// 获取版本号
        /// </summary>
        /// <returns></returns>
        public static string GetVersionNo()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            return GetVersionNo(assembly);
        }

        public static string GetVersionPublishDate()
        {
            return System.IO.File.GetLastWriteTime(Assembly.GetEntryAssembly().Location).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 获取版本号 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetVersionNo(Assembly assembly)
        {
            if (assembly != null)
            {
                return assembly.GetName().Version.ToString();
            }
            return "";
        }

        /// <summary>
        /// 获取版本号 包含git hashcode
        /// </summary>
        /// <returns></returns>
        public static string GetFullVersoinNo()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            return GetFullVersoinNo(assembly);
        }

        /// <summary>
        /// 获取版本号 包含git hashcode
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetFullVersoinNo(Assembly assembly)
        {
            if (assembly != null)
            {
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyInformationalVersionAttribute informationalVersionAttribute = (AssemblyInformationalVersionAttribute)attributes[0];
                    if (informationalVersionAttribute.InformationalVersion != "")
                    {
                        return informationalVersionAttribute.InformationalVersion;
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// 获取插件版本
        /// </summary>
        /// <param name="addinName"></param>
        /// <returns></returns>
        public static string GetAddinFullVersionNo(string addinName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string dir = AppPath.AddinPath;
            string path = Path.Combine(dir, addinName);

            if (String.Compare(addinName, WesApp.GeneralAddIn, true) == 0)
            {
                var addinAssemblies = assemblies.Where(a => !a.IsDynamic && a.Location.ToLower().Contains("wes.component.widgets"));
                if (addinAssemblies.Count() > 0)
                {
                    Assembly assembly = addinAssemblies.First();
                    return DesktopVersion.GetFullVersoinNo(assembly);
                }
            }
            else
            {
                var addinAssemblies = assemblies.Where(a => !a.IsDynamic && a.Location.ToLower().Contains(path.ToLower()));
                string version = string.Empty;
                if (addinAssemblies.Count() > 0)
                {
                    var query = addinAssemblies.Where(a => a.Location.ToLower().Contains("wes.customer"));
                    if (query.Count() > 0)
                    {
                        Assembly assembly = query.First();
                        return DesktopVersion.GetFullVersoinNo(assembly);
                    }
                }
            }
            return "";
        }
    }
}
