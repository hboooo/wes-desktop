using System;
using System.Windows;
using System.Windows.Input;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Extends;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Desktop.Windows
{
    /// <summary>
    /// MasterAuthorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MasterAuthorWindow : BaseWindow
    {
        #region Properties

        public string LoginId
        {
            get { return (string)GetValue(LoginIdProperty); }
            set { SetValue(LoginIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LoginId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoginIdProperty =
            DependencyProperty.Register("LoginId", typeof(string), typeof(MasterAuthorWindow), new PropertyMetadata(""));

        #endregion

        private VerificationType _type = VerificationType.Print;

        public MasterAuthorWindow()
        {
            InitializeComponent();
            txtLoginId.Focus();
        }

        public MasterAuthorWindow(VerificationType type) : this()
        {
            _type = type;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (IsInput())
            {
                //登錄
                dynamic loginRes = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, ScriptID.CHECK_LOGIN_MASTER, false)
                .AddJsonBody("loginId", LoginId)
                .AddJsonBody("password", pwd.Password)
                .Execute()
                .To<object>();
                if (loginRes != null && loginRes.Count > 0)
                {
                    bool isPrint = false;
                    foreach (var item in loginRes)
                    {
                        isPrint = Verification(item);
                        break;
                    }
                    if (isPrint)
                        this.DialogResult = true;
                    else
                        WesModernDialog.ShowWesMessage("登錄用戶沒有相關權限,請聯繫主管");
                }
                else
                {
                    WesModernDialog.ShowWesMessage("登錄用戶信息錯誤,請聯繫主管");
                }
            }
        }

        private bool Verification(dynamic item)
        {
            bool authored = false;
            switch (_type)
            {
                case VerificationType.Print:
                    authored = item.LSPrint == 1 ? true : false;
                    break;
                case VerificationType.Delete:
                    authored = item.LSDelete == 1 ? true : false;
                    break;
                case VerificationType.NotCheckLotNo:
                    authored = item.LSNotCheckLotNo == 1 ? true : false;
                    break;
                default:
                    break;
            }
            return authored;
        }

        private bool IsInput()
        {
            if (string.IsNullOrWhiteSpace(LoginId))
            {
                WesModernDialog.ShowWesMessage("用戶名不能為空".GetLanguage());
                return false;
            }
            else if (string.IsNullOrWhiteSpace(pwd.Password))
            {
                WesModernDialog.ShowWesMessage("密碼不能為空".GetLanguage());
                pwd.Focus();
                return false;
            }
            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


    }

}
