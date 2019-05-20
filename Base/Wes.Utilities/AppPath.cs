using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Wes.Utilities
{
    public sealed class AppPath
    {
        /// <summary>
        /// 当前工作路径
        /// </summary>
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 系统生成文件位置
        /// </summary>
        public static string DataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        /// <summary>
        /// 日誌文件位置
        /// </summary>
        public static string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "logs");

        /// <summary>
        /// 系统缓存文件位置
        /// </summary>
        public static string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Cache");


        /// <summary>
        /// Label模板文件位置
        /// </summary>
        public static string LabelTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LabelTemplates");

        /// <summary>
        /// Label模板图片文件位置
        /// </summary>
        public static string LabelTemplateImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LabelTemplates", "Images");


        /// <summary>
        /// 插件目录
        /// </summary>
        public static string AddinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Addins");

        /// <summary>
        /// WebKit路徑
        /// </summary>
        public static string WebKitPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebBrowser");

        /// <summary>
        /// 插件名称
        /// </summary>
        public static string AddinName = "";

        /// <summary>
        /// 客户系统目录
        /// </summary>
        public static string ExtensionAddinPath
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Addins", AddinName);
            }
        }

    }
}
