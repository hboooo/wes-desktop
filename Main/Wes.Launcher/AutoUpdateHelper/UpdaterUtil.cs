using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Wes.Launcher.Util;

namespace Wes.Launcher
{
    public delegate void ShowHandler();
    public sealed class UpdaterUtil
    {
        private static readonly UpdaterUtil instance = new UpdaterUtil();
        private UpdaterUtil() { }
        public static UpdaterUtil Instance { get { return instance; } }

        List<FileVersion> downloadList = new List<FileVersion>();

        private bool IsDesktopRunning(ref string exeName)
        {
            try
            {
                Process[] process = Process.GetProcessesByName("wes.desktop");
                if (process != null && process.Length > 0)
                {
                    exeName = process[0].ProcessName;
                    return true;
                }
                process = Process.GetProcessesByName("wes.server");
                if (process != null && process.Length > 0)
                {
                    exeName = process[0].ProcessName;
                    return true;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            return false;
        }

        public void Update()
        {
            try
            {
                downloadList.Clear();
                dynamic localInfo = ConfigUtil.Instance.GetLocalVersionInfo();
                dynamic versionInfo = ConfigUtil.Instance.GetRemoteVersionInfo();
                if (string.Compare(localInfo.version, versionInfo.version) != 0
                    || (string.IsNullOrEmpty(localInfo.version) && string.IsNullOrEmpty(versionInfo.version)))
                {
                    if (versionInfo.mode == true)
                    {
                        DeleteFiles(ConfigUtil.Instance.InstallPath);
                        if (versionInfo.files != null)
                        {
                            foreach (var item in versionInfo.files)
                                downloadList.Add(item);
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
                                        if ((string.Compare(item.Unique, remote.Unique, true) == 0
                                            || string.IsNullOrEmpty(item.Unique))
                                            && string.Compare(item.RelativePath, remote.RelativePath, true) == 0)
                                        {
                                            changed = item;
                                            break;
                                        }
                                    }
                                    if (changed != null && string.Compare(changed.LastVer, remote.LastVer, true) != 0)
                                    {
                                        downloadList.Add(remote);
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
                                    downloadList.Add(item);
                            }
                        }
                    }
                }
                try
                {
                    if (downloadList.Count > 0)
                    {
                        string exeName = string.Empty;
                        bool isRunning = IsDesktopRunning(ref exeName);
                        while (isRunning)
                        {
                            DialogResult dialogResult = MessageBox.Show($"WES 正在運行，無法更新。\r\n繼續更新，請關閉{exeName}后，點擊“確定”，若要退出更新請點擊“取消”", "告警", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            if (dialogResult == DialogResult.Cancel) return;
                            isRunning = IsDesktopRunning(ref exeName);
                        }

                        StartDownload(downloadList);
                        UnZipMwsd();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    CreateShortCut();
                    StartDesktop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("update failed: " + ex.Message);
            }
        }

        #region delete dirs and files

        private void DeleteFiles(string root)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(root);
                string[] files = Directory.GetFiles(root, "*.*", SearchOption.TopDirectoryOnly);
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        if (file.EndsWith("Wes.Launcher.exe", StringComparison.OrdinalIgnoreCase)) continue;
                        try
                        {
                            System.IO.File.SetAttributes(file, FileAttributes.Normal);
                            System.IO.File.Delete(file);
                        }
                        catch { }
                    }
                }

                foreach (string dir in dirs)
                {
                    if (dir.EndsWith("Data", StringComparison.OrdinalIgnoreCase)
                          || dir.EndsWith("LabelTemplates", StringComparison.OrdinalIgnoreCase))
                        continue;

                    DeleteFiles(dir);
                    try
                    {
                        Directory.Delete(dir);
                    }
                    catch { }
                    DeleteFiles(dir);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion

        public void UnZipMwsd()
        {
            string zipFileName = "mwsd.zip";

            string filePath = Path.Combine(ConfigUtil.Instance.InstallPath);
            string zipPath = Path.Combine(ConfigUtil.Instance.InstallPath, zipFileName);
            if (System.IO.File.Exists(zipPath))
            {
                bool result = ZipUtil.UnZip(zipFileName, filePath, "wes-v2");
                if (result)
                {
                    System.IO.File.Delete(zipPath);
                }
            }
        }

        public void StartDownload(List<FileVersion> downloadList)
        {
            DownloadProgress dp = new DownloadProgress(downloadList);
            dp.ShowDialog();
        }

        private void StartDesktop()
        {
            Application.ApplicationExit += Application_ApplicationExit;
            Application.Exit();
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = Path.Combine(ConfigUtil.Instance.InstallPath, "Wes.Desktop.exe");
                proc.StartInfo.WorkingDirectory = ConfigUtil.Instance.InstallPath;
                proc.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void CreateShortCut()
        {
            string DesktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);         //得到桌面文件夹
            if (System.IO.File.Exists(Path.Combine(DesktopPath, "WesV2.lnk")))
            {
                return;
            }
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(DesktopPath + "\\WesV2.lnk");
            shortcut.TargetPath = Path.Combine(ConfigUtil.Instance.InstallPath, "Wes.Launcher.exe");
            shortcut.Arguments = "";                                                                                 // 参数
            shortcut.Description = "Wes Launcher lnk";
            shortcut.WorkingDirectory = ConfigUtil.Instance.InstallPath;                                             //程序所在文件夹，在快捷方式图标点击右键可以看到此属性
            shortcut.IconLocation = Path.Combine(ConfigUtil.Instance.InstallPath, "ico.ico");                        //图标
            shortcut.WindowStyle = 1;
            shortcut.Save();
        }
    }
}
