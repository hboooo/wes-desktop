using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Wes.Utilities;

namespace Wes.Desktop.Windows.Updater
{
    /// <summary>
    /// 版本升级
    /// </summary>
    public class WesUpdater
    {
        public static readonly string FTP = ConfigurationManager.AppSettings["UpdateUrl"];
        public static readonly string FTP_USER = ConfigurationManager.AppSettings["FTP_USER"];
        public static readonly string FTP_PASSWORD = ConfigurationManager.AppSettings["FTP_PASSWORD"];

        /// <summary>
        /// 版本信息文件
        /// </summary>
        public static readonly string VersionFile = "AutoUpdateService.wes";

        /// <summary>
        /// 自动更新
        /// </summary>
        public void WesUpdate()
        {
            var publishVersion = GetLatestVersionInfo();
            var localVersion = GetCurrentVersionInfo();
            if (IsNeedUpdate(publishVersion, localVersion))
            {
                try
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = Path.Combine(AppPath.BasePath, $"{WesApp.LAUNCHER_NAME}.exe");
                    proc.StartInfo.WorkingDirectory = AppPath.BasePath;
                    proc.Start();
                }
                catch (Exception ex)
                {
                    LoggingService.Error(ex);
                }
                finally
                {
                    Environment.Exit(0);
                }
            }
        }

        public string GetLatestVersion()
        {
            var publishVersion = GetLatestVersionInfo();
            var localVersion = GetCurrentVersionInfo();
            if (IsNeedUpdate(publishVersion, localVersion))
            {
                return publishVersion.version;
            }
            return null;
        }

        private bool IsNeedUpdate(dynamic versionInfo, dynamic localInfo)
        {
            List<object> updateFiles = new List<object>();
            if (string.Compare(localInfo.version, versionInfo.version) != 0)
            {
                if (versionInfo.mode == true)
                {
                    if (versionInfo.files != null)
                    {
                        foreach (var item in versionInfo.files)
                            updateFiles.Add(item);
                    }
                }
                else
                {
                    if (versionInfo.files != null && versionInfo.files.Count > 0 &&
                        localInfo.files != null && localInfo.files.Count > 0)
                    {
                        foreach (var remote in versionInfo.files)
                        {
                            try
                            {
                                var changed = default(dynamic);
                                foreach (var item in localInfo.files)
                                {
                                    if (string.Compare(item.Unique.ToString(), remote.Unique.ToString(), true) == 0)
                                    {
                                        changed = item;
                                        break;
                                    }
                                }
                                if (changed != null && string.Compare(changed.LastVer.ToString(), remote.LastVer.ToString(), true) != 0)
                                {
                                    updateFiles.Add(remote);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.Write(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        if (versionInfo.files != null)
                        {
                            foreach (var item in versionInfo.files)
                                updateFiles.Add(item);
                        }
                    }
                }
            }
            if (updateFiles.Count > 0)
                return true;

            return false;
        }

        private dynamic GetLatestVersionInfo()
        {
            string versionFilePath = Path.Combine(FTP, VersionFile);
            return Deserialize(versionFilePath);
        }

        private dynamic GetCurrentVersionInfo()
        {
            string versionFilePath = Path.Combine(AppPath.BasePath, VersionFile);
            return Deserialize(versionFilePath);
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

        /// <summary>
        /// 反序列化XML字符串为指定类型
        /// </summary>
        private dynamic Deserialize(string file)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);

                string data = DynamicJson.SerializeXmlNode(doc.LastChild, true);
                dynamic dynamicData = DynamicJson.DeserializeObject<object>(data);
                return new
                {
                    files = dynamicData.FileVersion,
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
    }
    
}
