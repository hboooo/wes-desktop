using System;
using System.IO;
using Wes.Utilities;
using Wes.Utilities.Exception;

namespace Wes.Print
{
    /// <summary>
    /// ftp下载模块
    /// </summary>
    public class FtpDownload : ILabelPrintTemplateDownload
    {
        /// <summary>
        /// 当前模板缓存路径
        /// </summary>
        public static string _templatePath = System.Environment.CurrentDirectory;

        /// <summary>
        /// 当前模板缓存文件夹
        /// </summary>
        public static string _labelDir = "LabelTemplates";

        public bool Download(string name, ref string filename)
        {
            try
            {
                filename = string.Empty;
                FtpAccountHelper ftpAccount = new FtpAccountHelper();
                FtpHelper ftpHelper = new FtpHelper(new Uri(ftpAccount.Url), ftpAccount.UserName, ftpAccount.Password);
                filename = Path.Combine(_templatePath, _labelDir, name);
                return ftpHelper.DownloadFile(Path.Combine(ftpAccount.FtpDir, name), filename, name);
            }
            catch (Exception ex)
            {
                LoggingService.Error("下載文件異常", new WesException(ex));
            }
            return false;
        }

        public bool DownloadFile(string name, ref string filename)
        {
            try
            {
                filename = string.Empty;
                filename = Path.Combine(_templatePath, _labelDir, name);
                if (File.Exists(filename))
                {
                    return true;
                }
                else
                {
                    FtpAccountHelper ftpAccount = new FtpAccountHelper();
                    FtpHelper ftpHelper =
                        new FtpHelper(new Uri(ftpAccount.Url), ftpAccount.UserName, ftpAccount.Password);
                    return ftpHelper.DownloadFile(Path.Combine(ftpAccount.FtpDir, name), filename);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("下載文件異常", new WesException(ex));
            }
            return false;
        }

        public void DownloadFileAsync(string name, Action<string, bool> action)
        {
            try
            {
                string filename = Path.Combine(_templatePath, _labelDir, name);
                FtpAccountHelper ftpAccount = new FtpAccountHelper();
                FtpHelper ftpHelper = new FtpHelper(new Uri(ftpAccount.Url), ftpAccount.UserName, ftpAccount.Password);
                ftpHelper.DownloadFileAsync(Path.Combine(ftpAccount.FtpDir, name), filename, name, (res) =>
                {
                    action(filename, res);
                });
            }
            catch (Exception ex)
            {
                LoggingService.Error("下載文件異常", new WesException(ex));
                action(name, false);
            }
        }
    }
}
