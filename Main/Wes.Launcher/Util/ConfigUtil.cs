using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Wes.Launcher.Util
{
    public sealed class ConfigUtil
    {
        #region 单例

        private static readonly ConfigUtil instance = new ConfigUtil();
        private ConfigUtil()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wes.Desktop.exe");
                if (File.Exists(filePath))
                {
                    var exeConfig = ConfigurationManager.OpenExeConfiguration(filePath);
                    InstallPath = exeConfig.AppSettings.Settings["InstallPath"].Value;
                    if (exeConfig.AppSettings.Settings.AllKeys.Contains("FTP_SERVER"))
                        FtpServer = exeConfig.AppSettings.Settings["FTP_SERVER"].Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (string.IsNullOrEmpty(InstallPath)) InstallPath = @"c:\wes-v2\";
            if (string.IsNullOrEmpty(FtpServer)) FtpServer = @"ftp://172.16.4.23/pub/";  //生產環境
        }
        public static ConfigUtil Instance { get { return instance; } }

        #endregion

        public string UpdateConfigFile = "AutoUpdateService.wes";
        public string IgnoreExt = "pdb,xml,log,txt,btw";
        public string IgnoreDirectory = "app.publish,LabelTemplates";
        public string IgnoreFile = "Wes.Launcher.exe,UpdateAssemblyInfo.exe,ICSharpCode.SharpZipLib.dll,wes.db,Microsoft.Windows.Shell.dll,Wes.Launcher.vshost.exe";


        public string BuildPath = AppDomain.CurrentDomain.BaseDirectory;
        public string InstallPath = null;
        public string FtpServer = null;
        public string UserName = "";
        public string Passwrod = "";


        private List<FileVersion> FileVersions { get; set; }

        public void PrepareUri(string ftpUri, string ftpUser, string ftpPwd)
        {
            FtpServer = ftpUri;
            UserName = ftpUser;
            Passwrod = ftpPwd;

            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wes.Desktop.exe");
                if (File.Exists(filePath))
                {
                    var exeConfig = ConfigurationManager.OpenExeConfiguration(filePath);
                    exeConfig.AppSettings.Settings.Remove("FTP_SERVER");
                    exeConfig.AppSettings.Settings.Add("FTP_SERVER", ftpUri);
                    exeConfig.Save(ConfigurationSaveMode.Modified);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 反序列化XML字符串为指定类型
        /// </summary>
        private dynamic Deserialize(string file)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<FileVersion>));
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                List<FileVersion> result = null;
                using (StringReader stringReader = new StringReader(doc.InnerXml))
                {
                    object obj = xmlSerializer.Deserialize(stringReader);
                    result = (List<FileVersion>)obj;
                }
                return new
                {
                    files = result,
                    mode = GetXmlNamespaceBooleanValue(doc.LastChild, "publish-full"),
                    version = GetXmlNamespaceStringValue(doc.LastChild, "wes-version"),
                };
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
            return new
            {
                files = default(dynamic),
                mode = false,
                version = ""
            };
        }

        private string GetXmlNamespaceStringValue(XmlNode node, string xmlNamespace)
        {
            try
            {
                return node.GetNamespaceOfPrefix(xmlNamespace);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
            return "";
        }

        private bool GetXmlNamespaceBooleanValue(XmlNode node, string xmlNamespace)
        {
            try
            {
                var res = node.GetNamespaceOfPrefix(xmlNamespace);
                if (!string.IsNullOrEmpty(res))
                    return Convert.ToBoolean(res);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
            return false;
        }

        public string GetVersionNo()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                return assembly.GetName().Version.ToString();
            }
            return "";
        }

        /// <summary>
        /// 序列化object对象为XML字符串
        /// </summary>
        private string Serialize(object ObjectToSerialize, bool isFullPublish = false)
        {
            string result = null;
            try
            {
                dynamic localInfo = GetLocalVersionInfo();
                XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, new UTF8Encoding(false));
                    xmlTextWriter.Formatting = Formatting.Indented;
                    var namespaces = new XmlSerializerNamespaces();
                    namespaces.Add("publish-full", isFullPublish.ToString());
                    namespaces.Add("wes-version", GetVersionNo());
                    namespaces.Add("release-date", DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss"));
                    xmlSerializer.Serialize(xmlTextWriter, ObjectToSerialize, namespaces);
                    xmlTextWriter.Flush();
                    xmlTextWriter.Close();
                    UTF8Encoding uTF8Encoding = new UTF8Encoding(false, true);
                    result = uTF8Encoding.GetString(memoryStream.ToArray());
                }
            }
            catch (Exception innerException)
            {
                Debug.Write(innerException.Message);
            }
            return result;
        }

        /// <summary>
        /// 获取本地配置
        /// </summary>
        /// <returns></returns>
        public dynamic GetLocalVersionInfo()
        {
            string localXml = Path.Combine(InstallPath, ConfigUtil.Instance.UpdateConfigFile);
            return Deserialize(localXml);
        }

        /// <summary>
        /// 获取服务器配置
        /// </summary>
        /// <returns></returns>
        public dynamic GetRemoteVersionInfo()
        {
            string remoteXml = Path.Combine(ConfigUtil.Instance.FtpServer, ConfigUtil.Instance.UpdateConfigFile);
            return Deserialize(remoteXml);
        }

        public List<FileVersion> GetBuildFileVersions(string path)
        {
            FileVersions = new List<FileVersion>();
            getDirectoryFiles(path);
            return FileVersions;
        }

        private void GetLocalFileVersion(string path)
        {
            DirectoryInfo fdir = new DirectoryInfo(path);
            FileInfo[] files = fdir.GetFiles();

            foreach (var file in files)
            {
                FileVersion fv = BuildFileVersion(file);
                if (fv != null)
                {
                    FileVersions.Add(fv);
                }
            }
        }

        public FileVersion BuildFileVersion(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            return BuildFileVersion(fileInfo);
        }

        public FileVersion BuildFileVersion(FileInfo file)
        {
            //过滤忽略表中 带有忽略后缀名的文件
            if (ConfigUtil.Instance.IgnoreExt.Contains(file.Extension.Substring(1)))
                return null;
            if (ConfigUtil.instance.IgnoreFile.Contains(file.Name))
                return null;

            string md5 = MD5Util.GetMD5HashFromFile(file.FullName);
            FileVersion fv = new FileVersion();
            fv.Name = file.Name;
            fv.LastVer = md5;
            fv.Size = file.Length;
            fv.NeedRestart = true;

            //获取当前文件相对根目录的相对结构
            Uri u1 = new Uri(file.FullName);
            string publishLocalPath = Path.Combine(ConfigUtil.Instance.BuildPath.Replace("\\debug", "\\release"));
            Uri u2 = new Uri(publishLocalPath);
            Uri u3 = u2.MakeRelativeUri(u1);
            fv.RelativePath = u3.ToString();
            fv.Url = ConfigUtil.Instance.FtpServer + "/" + fv.RelativePath;
            fv.Unique = MD5Util.GetMD5Hash(fv.RelativePath);
            return fv;
        }

        public void SaveConfig(List<FileVersion> files, bool isFullPublish, string path)
        {
            string xmlStr = ConfigUtil.Instance.Serialize(files, isFullPublish);
            File.WriteAllText(Path.Combine(path, ConfigUtil.Instance.UpdateConfigFile), xmlStr);
        }

        //获得指定路径下所有子目录 的文件,
        private void getDirectoryFiles(string path)
        {
            string[] ignoreList = IgnoreDirectory.Split(',');
            foreach (var directory in ignoreList)
            {
                if (!string.IsNullOrEmpty(directory))
                {
                    if (path.Equals(Path.Combine(BuildPath, directory), StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }
            }

            GetLocalFileVersion(path);
            DirectoryInfo root = new DirectoryInfo(path);
            foreach (DirectoryInfo d in root.GetDirectories())
            {
                getDirectoryFiles(d.FullName);
            }
        }
    }
}
