using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Wes.Utilities;

namespace Wes.Desktop.Windows.DebugCommand
{
    [ExportDebugCommand(DebugID = "spread-log", Description = "show system log, paramter:clean last date[format:yyyy-MM-dd]")]
    public class SysLogCommand : BaseDebugCommand
    {
        private int _lastLineCount = 500;   //讀取文件最後行數
        private string _lastLog = Path.Combine(AppPath.DataPath, "lastLog.txt");

        public override void Execute(object parameter)
        {
            if (ExecuteParameter(this, parameter))
                return;

            string filename = DateTime.Now.ToString("yyyy-MM-dd");
            string file = System.IO.Path.Combine(AppPath.LogPath, filename + ".log");
            if (File.Exists(file))
            {
                Process.Start(file);
            }
        }

        protected override bool AfterExecuteParameter(List<string> parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                string date = parameters[0];
                try
                {
                    string filename = Convert.ToDateTime(date).ToString("yyyy-MM-dd");
                    if (!string.IsNullOrEmpty(filename))
                    {
                        string file = System.IO.Path.Combine(AppPath.LogPath, filename + ".log");
                        if (File.Exists(file))
                        {
                            Process.Start(file);
                        }
                    }
                    return true;
                }
                catch { }
            }
            return false;
        }

        public void CommandParamClean(List<string> paramters)
        {
            string[] logFiles = Directory.GetFiles(AppPath.LogPath, "*.log");
            foreach (var item in logFiles)
            {
                try
                {
                    File.SetAttributes(item, FileAttributes.Normal);
                    File.Delete(item);
                }
                catch { }
            }
            string[] imgFiles = Directory.GetFiles(AppPath.LogPath, "*.png");
            foreach (var item in imgFiles)
            {
                try
                {
                    File.SetAttributes(item, FileAttributes.Normal);
                    File.Delete(item);
                }
                catch { }
            }
        }

        public void CommandParamLast(List<string> paramters)
        {
            string filename = DateTime.Now.ToString("yyyy-MM-dd");
            string file = System.IO.Path.Combine(AppPath.LogPath, filename + ".log");
            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file, Encoding.Default);
                List<string> lastLines = new List<string>();
                //索引開始位置
                int readLineStartIndex = lines.Length;
                int readLineEndIndex = lines.Length > _lastLineCount ? lines.Length - _lastLineCount : 0;
                for (int i = readLineStartIndex - 1; i >= readLineEndIndex; i--)
                {
                    lastLines.Add(lines[i]);
                }

                FileStream fs = File.Create(_lastLog);
                lastLines.Reverse();
                foreach (var item in lastLines)
                {
                    byte[] bytes = Encoding.Default.GetBytes(item);
                    fs.Write(bytes, 0, bytes.Length);
                    byte[] space = Encoding.Default.GetBytes("\r\n");
                    fs.Write(space, 0, space.Length);
                }
                fs.Close();
                Process.Start("notepad.exe", _lastLog);
            }
        }

        public void CommandParamTrace(List<string> paramters)
        {
            if (paramters.Count == 1)
            {
                string filename = DateTime.Now.ToString("yyyy-MM-dd");
                string file = System.IO.Path.Combine(AppPath.LogPath, filename + "trace.log");
                if (File.Exists(file))
                {
                    Process.Start(file);
                }
            }
            else if (paramters.Count > 1)
            {
                string date = paramters[1];
                try
                {
                    string filename = Convert.ToDateTime(date).ToString("yyyy-MM-dd");
                    if (!string.IsNullOrEmpty(filename))
                    {
                        string file = System.IO.Path.Combine(AppPath.LogPath, filename + "trace.log");
                        if (File.Exists(file))
                        {
                            Process.Start(file);
                        }
                    }
                }
                catch { }
            }
        }
    }
}
