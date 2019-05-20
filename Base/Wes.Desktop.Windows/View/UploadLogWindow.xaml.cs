using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Wes.Utilities;
using Wes.Wrapper;
using static Wes.Utilities.FtpHelper;

namespace Wes.Desktop.Windows.View
{
    /// <summary>
    /// UploadLogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UploadLogWindow : BaseWindow
    {
        public UploadLogWindow()
        {
            InitializeComponent();
            this.Closing += UploadLogWindow_Closing;
        }

        public string UploadFile
        {
            get { return this.txtUploadAddr.Text; }
            set { this.txtUploadAddr.Text = value; }
        }

        public Visibility MessageVisbility
        {
            get { return this.txtScreenshotMessage.Visibility; }
            set { this.txtScreenshotMessage.Visibility = value; }
        }

        private void UploadLogWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (spProgress.Visibility == Visibility.Visible)
            {
                WesModernDialog.ShowWesMessage("正在上傳文件，不能關閉。");
                e.Cancel = true;

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnSelectUrl_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\logs\\";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtUploadAddr.Text = ofd.FileName;
                LoggingService.Info("選擇文件:" + ofd.FileName);
            }
        }


        private void BtnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            string ftp = ConfigurationManager.AppSettings["WMS_LOG_FTP_URL"];
            Uri uri = new Uri(ftp);
            FtpHelper ftpHelper = new FtpHelper(uri, "ftpuser", "ftpuser");
            FileInfo fileInfo = new FileInfo(txtUploadAddr.Text);

            string extensionName = fileInfo.Extension;
            string file = fileInfo.Name;
            string filePathName = txtUploadAddr.Text;
            string ip = Utils.GetMachineIP();
            string filename = string.Format("{0}_({1})_({2})_({3}){4}", Path.GetFileNameWithoutExtension(fileInfo.FullName), WesDesktop.Instance.User.Code, Environment.MachineName, ip, extensionName);

            ftpHelper.FtpUploadProgressEvent += FtpHelper_FtpUploadProgressEvent;
            ftpHelper.FtpUploadFinishEvent += FtpHelper_FtpUploadFinishEvent;
            //调用线程执行上传接口
            Thread t = new Thread(new ThreadStart(delegate
            {
                try
                {
                    ftpHelper.UploadFile(filePathName, filename);
                }
                catch (Exception ex)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        WesModernDialog.ShowWesMessage(string.Format("{0}", ex.Message));
                    }));
                }
            }));
            t.IsBackground = true;
            t.Start();
        }

        private void FtpHelper_FtpUploadFinishEvent()
        {
            FtpHelper_FtpUploadProgressEvent(100);
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //設置控件屬性
                BuSelectFile.IsEnabled = true;
                BtnUploadFile.IsEnabled = true;
                spProgress.Visibility = Visibility.Collapsed;
                WesModernDialog.ShowWesMessage(txtUploadAddr.Text + "文件上傳成功");
            }));
        }

        private void FtpHelper_FtpUploadProgressEvent(int value)
        {
            if (spProgress.Visibility != Visibility.Visible)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    BuSelectFile.IsEnabled = false;
                    BtnUploadFile.IsEnabled = false;
                    spProgress.Visibility = Visibility.Visible;
                }));
            }

            if (System.Threading.Thread.CurrentThread != BtnUploadFile.Dispatcher.Thread)
            {
                BtnUploadFile.Dispatcher.Invoke(new FtpUploadProgress(FtpHelper_FtpUploadProgressEvent), value);
            }
            else
            {
                this.progressBar.Value = value;
                this.textblock.Text = value.ToString() + "%";
            }
        }
        
    }
}


