using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wes.Launcher.Util;

namespace Wes.Launcher
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                bool run = false;
                System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Wes.Launcher", out run);
                if (run)
                {
                    mutex.ReleaseMutex();
                    //设置应用程序处理异常方式：ThreadException处理
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                    //处理UI线程异常
                    Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                    //处理非UI线程异常
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                    #region 应用程序的主入口点
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    
                    if (args != null && args.Length > 0)
                    {
                        if (args.Contains("publish"))
                        {
                            try
                            {
                                var win = new MainForm();
                                if (args.Contains("full"))
                                {
                                    win.IsFullPublish = true;
                                }
                                else
                                {
                                    win.IsFullPublish = false;
                                }
                                win.Pushlish();
                            }
                            catch { }
                            return;
                        }
                    }

                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        Console.WriteLine("start with debug , opem main form");
                        Application.Run(new MainForm());
                    }
                    else
                    {
                        Console.WriteLine("start with runner, check install path");
                        if (!ConfigUtil.Instance.InstallPath.Equals(ConfigUtil.Instance.BuildPath, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("Please execute the file in the " + ConfigUtil.Instance.InstallPath + " \r\n " +
                                "Current Path " + ConfigUtil.Instance.BuildPath);
                            return;
                        }
                        Console.WriteLine("start UpdaterUtil.Instance.Update()");
                        UpdaterUtil.Instance.Update();
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                string str = GetExceptionMsg(ex, string.Empty);
                MessageBox.Show(str, "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            string str = GetExceptionMsg(e.Exception, e.ToString());
            MessageBox.Show(str, "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //LogManager.WriteLog(str);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string str = GetExceptionMsg(e.ExceptionObject as Exception, e.ToString());
            MessageBox.Show(str, "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //LogManager.WriteLog(str);
        }

        /// <summary>
        /// 生成自定义异常消息
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="backStr">备用异常消息：当ex为null时有效</param>
        /// <returns>异常字符串文本</returns>
        static string GetExceptionMsg(Exception ex, string backStr)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("****************************error****************************");
            sb.AppendLine("【datetime】：" + DateTime.Now.ToString());
            if (ex != null)
            {
                sb.AppendLine("【exceptionType】：" + ex.GetType().Name);
                sb.AppendLine("【message】：" + ex.Message);
                sb.AppendLine("【StackTrace】：" + ex.StackTrace);
            }
            else
            {
                sb.AppendLine("【unhanlder】：" + backStr);
            }
            sb.AppendLine("***************************************************************");
            return sb.ToString();
        }
    }
}
