using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Desktop.Windows.Common
{
    /// <summary>
    /// WesTeamSupport.xaml 的交互逻辑
    /// </summary>
    public partial class WesTeamSupport : BaseWindow
    {
        ObservableCollection<SupportUser> _collection = new ObservableCollection<SupportUser>();
        private WesFlowID _flow;

        public WesTeamSupport(WesFlowID flow)
        {
            InitializeComponent();
            txtUserCode.Focus();
            _flow = flow;
            InitDgGrid();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Enter && txtUserCode.IsFocused)
            {
                AddUser();
            }
        }

        private void InitDgGrid()
        {
            _collection.Clear();
            Dictionary<string, SupportUser> users = WesDesktop.Instance.AddIn.Supports.Users;
            foreach (var supportUser in users)
            {
                if (supportUser.Value.Flows.Contains(_flow))
                {
                    _collection.Add(supportUser.Value);
                }
            }
            var bind = new Binding { Source = _collection };
            DgData.SetBinding(ItemsControl.ItemsSourceProperty, bind);
        }

        private void AddUser()
        {
            txtUserCode.SelectAll();
            string userCode = txtUserCode.Text.Trim();
            if (string.IsNullOrEmpty(userCode))
            {
                WesModernDialog.ShowWesMessage("用戶賬號不能為空");
                return;
            }
            if (string.Compare(WesDesktop.Instance.User.Code, userCode, true) == 0)
            {
                WesModernDialog.ShowWesMessage("输入賬號為當前登錄賬號，無法支援。");
                return;
            }

            if (CheckUserCode(userCode))
            {
                WesDesktop.Instance.AddIn.Supports.Add(userCode, _flow);
                InitDgGrid();
            }
        }


        private bool CheckUserCode(string useCode)
        {
            RestApi.NewInstance(Method.POST)
                            .AddUriParam(RestUrlType.WmsServer, ScriptID.CHECK_TEAM_SUPPORT_USER, false)
                            .AddJsonBody("user", useCode)
                            .Execute()
                            .To<object>();
            return true;
        }


        private void Leave_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var userCode = btn.Tag.ToString();
            WesDesktop.Instance.AddIn.Supports.Remove(userCode, _flow);
            InitDgGrid();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddUser();
        }
    }

}
