using System.Configuration;

namespace Wes.Utilities
{
    /// <summary>
    /// ftp账户相关信息
    /// </summary>
    public class FtpAccountHelper
    {
        public string Url { get; set; }

        public string FtpDir { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public FtpAccountHelper()
        {
            GetAccount();
        }

        /// <summary>
        /// 获取账户密码
        /// </summary>
        private void GetAccount()
        {
            this.Url = ConfigurationManager.AppSettings["PRINT_TEMPLATE_URL"];
            this.FtpDir = ConfigurationManager.AppSettings["PRINT_FTP_DIR"];
            this.UserName = ConfigurationManager.AppSettings["PRINT_FTP_USER"];
            this.Password = ConfigurationManager.AppSettings["PRINT_FTP_PASSWORD"];
        }
    }
}
