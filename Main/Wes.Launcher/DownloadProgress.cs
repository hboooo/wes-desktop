using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Linq;
using Wes.Launcher;
using Wes.Launcher.Util;

namespace Wes.Launcher
{
    public partial class DownloadProgress : Form
    {
        private bool isFinished = false;
        private List<FileVersion> downloadFileList = null;
        private ManualResetEvent evtDownload = null;
        private ManualResetEvent evtPerDonwload = null;
        private WebClient clientDownload = null;

        private List<FileVersion> downloadedList = null;
        private int Total = 0;

        #region The constructor of DownloadProgress
        public DownloadProgress(List<FileVersion> downloadFileListTemp)
        {
            InitializeComponent();
            this.downloadFileList = downloadFileListTemp;
            downloadedList = new List<FileVersion>();
            Total = downloadFileList.Count();
        }
        #endregion

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isFinished)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                if (clientDownload != null)
                    clientDownload.CancelAsync();

                evtDownload.Set();
                evtPerDonwload.Set();
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            labelUrl.Text = ConfigUtil.Instance.FtpServer;
            evtDownload = new ManualResetEvent(true);
            evtDownload.Reset();
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.ProcDownload));
        }

        long total = 0;
        long nDownloadedTotal = 0;

        private void ProcDownload(object o)
        {
            downloadedList = new List<FileVersion>();
            evtPerDonwload = new ManualResetEvent(false);
            total = downloadFileList.Select(a => a.Size).Sum();
            Console.WriteLine("start download file count: " + downloadFileList.Count());
            while (!evtDownload.WaitOne(0, false))
            {
                try
                {
                    if (this.downloadFileList.Count == 0)
                        break;


                    FileVersion file = this.downloadFileList[0];

                    Console.WriteLine("start download file" + file.Name);

                    string path = Path.GetDirectoryName(Path.Combine(ConfigUtil.Instance.InstallPath, file.RelativePath));

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    this.ShowCurrentDownloadFileName(file.Name);

                    //Download
                    clientDownload = new WebClient();


                    clientDownload.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                    {
                        this.SetProcessBar(e.ProgressPercentage, (int)((nDownloadedTotal + e.BytesReceived) * 100 / total));
                    };

                    clientDownload.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                    {
                        FileVersion dfile = e.UserState as FileVersion;
                        nDownloadedTotal += dfile.Size;
                        this.SetProcessBar(0, (int)(nDownloadedTotal * 100 / total));
                        evtPerDonwload.Set();
                        downloadedList.Add(dfile);
                    };

                    evtPerDonwload.Reset();

                    clientDownload.DownloadFileAsync(new Uri(file.Url), Path.Combine(ConfigUtil.Instance.InstallPath, file.RelativePath), file);

                    //Thread.Sleep(50);
                    evtPerDonwload.WaitOne();
                    clientDownload.Dispose();
                    clientDownload = null;

                    this.downloadFileList.Remove(file);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            if (this.downloadFileList.Count == 0)
                Exit(true);
            else
                Exit(false);
            evtDownload.Set();
        }

        delegate void ShowCurrentDownloadFileNameCallBack(string name);
        private void ShowCurrentDownloadFileName(string name)
        {
            if (this.labelCurrentItem.InvokeRequired)
            {
                ShowCurrentDownloadFileNameCallBack cb = new ShowCurrentDownloadFileNameCallBack(ShowCurrentDownloadFileName);
                this.Invoke(cb, new object[] { name });
            }
            else
            {
                this.labelCurrentItem.Text = name;
            }
        }

        delegate void SetProcessBarCallBack(int current, int total);
        private void SetProcessBar(int current, int total)
        {
            if (this.progressBarCurrent.InvokeRequired)
            {
                SetProcessBarCallBack cb = new SetProcessBarCallBack(SetProcessBar);
                this.Invoke(cb, new object[] { current, total });
            }
            else
            {
                this.progressBarCurrent.Value = current;
                this.progressBarTotal.Value = total;
                labelTotal.Text = downloadedList.Count() + " / " + Total;
            }
        }

        delegate void ExitCallBack(bool success);
        private void Exit(bool success)
        {
            if (this.InvokeRequired)
            {
                ExitCallBack cb = new ExitCallBack(Exit);
                this.Invoke(cb, new object[] { success });
            }
            else
            {
                this.isFinished = success;
                this.DialogResult = success ? DialogResult.OK : DialogResult.Cancel;
                this.Close();
            }
        }


    }
}