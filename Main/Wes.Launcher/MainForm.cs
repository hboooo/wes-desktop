using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Wes.Launcher.Util;

namespace Wes.Launcher
{
    public partial class MainForm : Form
    {
        public delegate void DelReadStdOutput(string result, Color color);

        public event DelReadStdOutput readStdOutput;

        public bool IsFullPublish { get; set; } = false;

        public MainForm()
        {
            InitializeComponent();
            readStdOutput += new DelReadStdOutput(SetResult);
        }

        delegate void SetResultCallBack(string text, Color color);

        private void SetResult(string text, Color color)
        {
            int start = this.rtbConsole.TextLength;
            this.rtbConsole.AppendText(text + "    " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff") + "\r\n");
            this.rtbConsole.Select(start, start + text.Length);
            this.rtbConsole.SelectionColor = color;
            Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  " + text + "\r\n");
        }

        public void DataReceivedHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.Invoke(readStdOutput, new object[] {e.Data, Color.Gray});
            }
        }

        public void ErrorReceivedHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                HasError = true;
                this.Invoke(readStdOutput, new object[] {e.Data, Color.Red});
            }
        }

        static bool HasError = false;
        static bool isProcess = false;

        private void CmdProcess_Exited(object sender, EventArgs e)
        {
            isProcess = false;
        }


        private void RealAction(string StartFileName, string StartFileArg)
        {
            Process CmdProcess = new Process();

            CmdProcess.StartInfo.FileName = StartFileName; // 命令
            CmdProcess.StartInfo.Arguments = StartFileArg; // 参数

            CmdProcess.StartInfo.CreateNoWindow = true; // 不创建新窗口
            CmdProcess.StartInfo.UseShellExecute = false;
            CmdProcess.StartInfo.RedirectStandardInput = true; // 重定向输入
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出
            CmdProcess.StartInfo.RedirectStandardError = true; // 重定向错误输出
            //CmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            CmdProcess.OutputDataReceived += new DataReceivedEventHandler(DataReceivedHandler);
            CmdProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorReceivedHandler);

            CmdProcess.EnableRaisingEvents = true; // 启用Exited事件
            CmdProcess.Exited += new EventHandler(CmdProcess_Exited); // 注册进程结束事件

            CmdProcess.Start();
            isProcess = true;
            CmdProcess.BeginOutputReadLine();
            CmdProcess.BeginErrorReadLine();

            // 如果打开注释，则以同步方式执行命令，此例子中用Exited事件异步执行。
            //CmdProcess.WaitForExit();
        }

        private void MakeVersionNum()
        {
            SetResult("setup Assembly info  ....", Color.Green);
            bool exists = File.Exists(Path.Combine(ConfigUtil.Instance.BuildPath, "UpdateAssemblyInfo.exe"));
            if (!exists)
            {
                throw new Exception("UpdateAssemblyInfo.exe not found ");
            }

            RealAction("UpdateAssemblyInfo.exe", "");
            while (isProcess) Application.DoEvents();
            RealAction("UpdateAssemblyInfo.exe", @"--folder Wes.Components\AVNET");
        }

        private void BuildVersionMaker()
        {
            SetResult("开始编译 UpdateAssemblyInfo ....", Color.Green);
            string bConfig = ConfigUtil.Instance.BuildPath.EndsWith("debug\\") ? "Debug" : "Release";
            string projectArgs = "..\\..\\Tools\\UpdateAssemblyInfo\\UpdateAssemblyInfo.csproj -p:configuration=" +
                                 bConfig + " -t:rebuild";
            RealAction("MSBuild.exe", projectArgs);
            if (HasError)
            {
                throw new Exception("发布失败");
            }
        }

        private void BuildProject()
        {
            SetResult("开始编译 ....", Color.Green);
            string bConfig = ConfigUtil.Instance.BuildPath.EndsWith("debug\\") ? "Debug" : "Release";
            RealAction("MSBuild.exe", "..\\..\\WESV2.sln -p:configuration=" + bConfig + " -t:rebuild");
            if (HasError)
            {
                throw new Exception("发布失败");
            }
        }

        private void ZipFile()
        {
            string fileName = "Microsoft.Windows.Shell.dll";
            string zipFileName = "mwsd.zip";
            string publishLocalPath = Path.Combine(ConfigUtil.Instance.BuildPath.Replace("\\debug", "\\release"));
            string filePath = Path.Combine(publishLocalPath, fileName);
            string zipPath = Path.Combine(publishLocalPath, zipFileName);
            bool result = ZipUtil.Zip(filePath, zipPath, "wes-v2");
            if (result)
            {
                //File.Delete(filePath);
            }
        }

        private void UploadFile()
        {
            SetResult("开始上传  url:" + ConfigUtil.Instance.FtpServer, Color.Green);

            string publishLocalPath = Path.Combine(ConfigUtil.Instance.BuildPath.Replace("\\debug", "\\release"));
            List<FileVersion> files = ConfigUtil.Instance.GetBuildFileVersions(publishLocalPath);
            ConfigUtil.Instance.SaveConfig(files, IsFullPublish, publishLocalPath);
            FileVersion configFile = files.Where(f => f.Name.Contains(".wes")).FirstOrDefault();
            FileVersion newConfigFile =
                ConfigUtil.Instance.BuildFileVersion(Path.Combine(publishLocalPath,
                    ConfigUtil.Instance.UpdateConfigFile));
            files.Remove(configFile);
            files.Add(newConfigFile);
            ConfigUtil.Instance.SaveConfig(files, IsFullPublish, publishLocalPath);

            List<dynamic> remoteFiles = new List<dynamic>();
            try
            {
                dynamic versionInfo = ConfigUtil.Instance.GetRemoteVersionInfo();
                if (versionInfo != null && versionInfo.files != null)
                {
                    foreach (var item in versionInfo.files)
                    {
                        remoteFiles.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            FtpUtil ftp = new FtpUtil(new Uri(ConfigUtil.Instance.FtpServer), ConfigUtil.Instance.UserName,
                ConfigUtil.Instance.Passwrod);
            ftp.FtpUploadProgressEvent += Ftp_FtpUploadProgressEvent;
            ftp.FtpUploadFinishEvent += Ftp_FtpUploadFinishEvent;
            int count = 0;
            foreach (var file in files)
            {
                bool isEquals = false;
                foreach (var item in remoteFiles)
                {
                    if (string.Compare(item.Unique, file.Unique, true) == 0 &&
                        string.Compare(item.LastVer, file.LastVer) == 0)
                    {
                        //SetResult("忽略上傳文件 :" + file.Name + "", Color.Yellow);
                        isEquals = true;
                        break;
                    }
                }

                if (isEquals) continue;

                SetResult("上传文件 :" + file.Name + " ...", Color.Green);
                string localPath = Path.Combine(publishLocalPath, file.RelativePath);
                //Console.WriteLine("進度:{0}%", 0);
                bool result = ftp.UploadFile(localPath, "/" + file.RelativePath, true);
                if (!result)
                {
                    string error = string.Format("文件上传失败 file:{0},path:{1}, relativePath:{2}", file.Name, localPath,
                        file.RelativePath);
                    SetResult(error + ",error:" + ftp.ErrorMsg, Color.Red);
                    Thread.Sleep(3000);
                    throw new Exception(error);
                }

                //SetResult("上传文件 :" + file.Name + "完成", Color.Green);
                count++;
            }

            SetResult($"共上传 {count} 个文件，版本:{ConfigUtil.Instance.GetVersionNo()}", Color.Green);
            Thread.Sleep(2000);
        }

        private void Ftp_FtpUploadFinishEvent(string elapsed)
        {
            Console.WriteLine("\r{0}  进度:{1}%  用时:{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), 100, elapsed);
        }

        private void Ftp_FtpUploadProgressEvent(int value)
        {
            Console.Write("\r{0}  进度:{1}%", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), value);
        }

        /// <summary>
        /// 最后上传版本文件,避免发布过程中出现更新的情况
        /// </summary>
        private void UploadVersionInfo()
        {
            SetResult("上传文件 :" + ConfigUtil.Instance.UpdateConfigFile + " ...", Color.Green);

            string relativePath = "/" + ConfigUtil.Instance.UpdateConfigFile;
            string localPath = Path.Combine(ConfigUtil.Instance.BuildPath, ConfigUtil.Instance.UpdateConfigFile);

            FtpUtil ftp = new FtpUtil(new Uri(ConfigUtil.Instance.FtpServer), ConfigUtil.Instance.UserName,
                ConfigUtil.Instance.Passwrod);

            bool result = ftp.UploadFile(localPath, relativePath, true);
            if (!result)
            {
                string error = string.Format("文件上传失败 file:{0},path:{1}, relativePath:{2}",
                    ConfigUtil.Instance.UpdateConfigFile, localPath, ConfigUtil.Instance.UpdateConfigFile);
                SetResult(error + ",error:" + ftp.ErrorMsg, Color.Red);
            }

            SetResult("上传文件 :" + ConfigUtil.Instance.UpdateConfigFile + "完成", Color.Green);
        }

        private void UploadSetUp()
        {
            SetResult("开始上传  url:" + ConfigUtil.Instance.FtpServer, Color.Green);

            FtpUtil ftp = new FtpUtil(new Uri(ConfigUtil.Instance.FtpServer), ConfigUtil.Instance.UserName,
                ConfigUtil.Instance.Passwrod);

            string configPath = Path.Combine(ConfigUtil.Instance.BuildPath, "");
            ftp.UploadFile(configPath, "/" + ConfigUtil.Instance.UpdateConfigFile, true);

            SetResult("上传文件 :" + ConfigUtil.Instance.UpdateConfigFile + "完成", Color.Green);
        }

        public int Pushlish()
        {
            try
            {
                SetResult("开始发布 ....", Color.Green);
                ConfigUtil.Instance.PrepareUri(this.cmbftp.Text, this.txtFtpUser.Text, this.txtPassword.Text);
                ZipFile();
                UploadFile();
                SetResult("发布成功.:" + ConfigUtil.Instance.FtpServer, Color.Green);
                //記録發佈日誌信息
                this.AddDeployLog();
                return 0;
            }
            catch (Exception ex)
            {
                SetResult("发布失败, " + ex.ToString(), Color.Red);
                Console.WriteLine("发布失败,错误:" + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return 1;
        }

        /// <summary>
        /// 添加發佈日誌信息，以備版本校驗
        /// </summary>
        public void AddDeployLog()
        {
            //添加版本發佈信息到後臺數據庫
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("appName", "WES");
            dictionary.Add("appVersion", ConfigUtil.Instance.GetVersionNo());
            dictionary.Add("publishUserName", Environment.MachineName);
            //開始發送請求到後臺
            this.SendHttpRequest(dictionary);
        }

        /// <summary>
        /// 将字典类型序列化为json字符串
        /// </summary>
        /// <typeparam name="TKey">字典key</typeparam>
        /// <typeparam name="TValue">字典value</typeparam>
        /// <param name="dict">要序列化的字典数据</param>
        /// <returns>json字符串</returns>
        public string SerializeDictionaryToJsonString<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            if (dict.Count == 0)
                return "";

            string jsonStr = JsonConvert.SerializeObject(dict);
            return jsonStr;
        }

        /// <summary>
        /// 将json字符串反序列化为字典类型
        /// </summary>
        /// <typeparam name="TKey">字典key</typeparam>
        /// <typeparam name="TValue">字典value</typeparam>
        /// <param name="jsonStr">json字符串</param>
        /// <returns>字典数据</returns>
        public static Dictionary<TKey, TValue> DeserializeStringToDictionary<TKey, TValue>(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return new Dictionary<TKey, TValue>();

            Dictionary<TKey, TValue> jsonDict = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(jsonStr);

            return jsonDict;
        }

        /// <summary>
        /// 發送請求
        /// </summary>
        /// <param name="dictionary"></param>
        public void SendHttpRequest(Dictionary<string, object> dictionary)
        {
            var request = (HttpWebRequest) WebRequest.Create("http://172.16.4.23:9091/app-version/deploy-log");
//            var request = (HttpWebRequest) WebRequest.Create("http://192.168.1.156:9091/app-version/deploy-log");

            String value = JsonConvert.SerializeObject(dictionary);
            var data = Encoding.ASCII.GetBytes(value);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse) request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            string Msg = "當前信息:";
            Console.WriteLine(Msg+responseString);
        }


        private void btnPublish_Click(object sender, EventArgs e)
        {
            #if DEBUG
            MessageBox.Show("當前是 Debug Build,必須使用Release Build發佈");
            //return;
            #endif

            if (string.IsNullOrWhiteSpace(cmbftp.Text))
            {
                MessageBox.Show("請輸入FTP服務地址");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFtpUser.Text))
            {
                MessageBox.Show("請輸入FTP帳戶");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("請輸入FTP密碼");
                return;
            }

            try
            {
                ConfigUtil.Instance.PrepareUri(this.cmbftp.Text, this.txtFtpUser.Text, this.txtPassword.Text);
                this.rtbConsole.Clear();
                this.rtbConsole.Focus();

                SetResult("开始发布 ....", Color.Green);

                //BuildVersionMaker();
                //while (isProcess) Application.DoEvents();

                MakeVersionNum();
                while (isProcess) Application.DoEvents();

                //BuildProject();
                //while (isProcess) Application.DoEvents();

                Thread.Sleep(100);
                ZipFile();

                Thread.Sleep(100);
                UploadFile();

                Thread.Sleep(100);
                UploadVersionInfo();

                SetResult("发布成功.:" + ConfigUtil.Instance.FtpServer, Color.Green);
                MessageBox.Show("发布成功!");
            }
            catch (Exception ex)
            {
                SetResult("发布失败, " + ex.ToString(), Color.Red);
                return;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            ConfigUtil.Instance.InstallPath = ConfigUtil.Instance.BuildPath;
            UpdaterUtil.Instance.Update();
        }
    }
}