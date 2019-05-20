using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Wes.Utilities
{
    public static class Utils
    {
        /// <summary>
        /// 获取本地Mac地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalMacAddress()
        {
            try
            {
                //获取网卡硬件地址 
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return "unknow";
            }
        }

        /// <summary>
        /// 獲取當前函數的調用者類型
        /// </summary>
        /// <param name="level">向上獲取層級</param>
        /// <returns></returns>
        public static Type GetMethodInvokeType(int level)
        {
            StackTrace trace = new StackTrace();
            StackFrame frame = trace.GetFrame(level);   //向上獲取層級
            if (frame != null)
            {
                MethodBase method = frame.GetMethod();
                if (method != null)
                {
                    return method.ReflectedType;
                }
            }
            return null;
        }

        /// <summary>
        /// 獲取當前函數的調用者類型
        /// </summary>
        /// <param name="level">向上獲取層級</param>
        /// <returns></returns>
        public static Type GetMethodInvokeTypeLevel(int level)
        {
            Type type = null;
            while (type == null && level > 1)
            {
                type = GetMethodInvokeType(level);
                level--;
            }
            if (type != null) return type;
            return null;
        }

        /// <summary>
        /// 检查配置端口是否存在
        /// </summary>
        /// <param name="com"></param>
        /// <returns></returns>
        public static bool IsContainsPort(string com)
        {
            try
            {
                string[] comPorts = SerialPort.GetPortNames();
                if (comPorts != null && comPorts.Length > 0)
                {
                    var query = comPorts.Where(c => string.Compare(c, com) == 0);
                    if (query.Count() > 0)
                        return true;
                }
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
            return false;
        }

        /// <summary>
        /// 獲取本機IP
        /// </summary>
        /// <returns></returns>
        public static string GetMachineIP()
        {
            try
            {
                System.Net.IPAddress[] _IPList = System.Net.Dns.GetHostAddresses(Environment.MachineName);
                for (int i = 0; i != _IPList.Length; i++)
                {
                    if (_IPList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return _IPList[i].ToString();
                    }
                }
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
            return "";
        }

        /// <summary>
        /// 檢測本機目錄是否存在,否則創建
        /// </summary>
        /// <param name="isCreateDirectory"></param>
        /// <returns></returns>
        public static bool CheckCreateDirectoroy(string path, bool isCreateDirectory = false)
        {
            if (!Directory.Exists(path))
            {
                if (isCreateDirectory) Directory.CreateDirectory(path);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 刪除文件
        /// </summary>
        /// <param name="filename"></param>
        public static void DeleteFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    FileInfo fileInfo = new FileInfo(filename);
                    if ((fileInfo.Attributes & FileAttributes.ReadOnly) > 0)
                        fileInfo.Attributes ^= FileAttributes.ReadOnly;
                    fileInfo.Delete();
                }
            }
            catch (System.Exception ex)
            {
                LoggingService.Error("刪除文件失敗", ex);
            }
        }

        /// <summary>
        /// 删除目录中的所有文件
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFiles(string path)
        {
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                foreach (var item in files)
                {
                    DeleteFile(item);
                }
            }
        }

        /// <summary>
        /// 加载图片资源
        /// 自动释放图片资源
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static BitmapImage GetBitmapImage(string filename)
        {
            BitmapImage bitmap = new BitmapImage();
            if (File.Exists(filename))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                using (Stream ms = new MemoryStream(File.ReadAllBytes(filename)))
                {
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 删除指定exe进程
        /// </summary>
        /// <param name="exeName"></param>
        public static void KillProcess(string exeName)
        {
            try
            {
                Process[] process = Process.GetProcessesByName(exeName);
                foreach (var item in process)
                {
                    item.Kill();
                    LoggingService.DebugFormat("{0} killed,id:{1}", item.ProcessName, item.Id);
                }
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        /// <summary>
        /// 删除指定exe进程，保留一个
        /// </summary>
        /// <param name="exeName"></param>
        public static void KillProcessRemaining(string exeName, int count)
        {
            try
            {
                Process[] process = Process.GetProcessesByName(exeName);
                for (int i = 0; i < process.Length - count; i++)
                {
                    Process item = process[i];
                    item.Kill();
                    LoggingService.DebugFormat("{0} killed,id:{1}", item.ProcessName, item.Id);
                }
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        /// <summary>
        /// 获取最早启动的exe
        /// </summary>
        /// <param name="exeName"></param>
        /// <returns></returns>
        public static Process GetProcess(string exeName)
        {
            try
            {
                Process[] process = Process.GetProcessesByName(exeName);
                if (process != null && process.Length > 0)
                {
                    return process.OrderBy(p => p.StartTime).First();
                }
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
            return null;
        }
    }
}
