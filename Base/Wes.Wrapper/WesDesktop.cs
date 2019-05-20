using System;
using System.Collections.Generic;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Languages;

namespace Wes.Wrapper
{
    public class WesDesktop
    {
        private static WesDesktop _wesDesktop;

        private static readonly object _locker = new object();

        public static WesDesktop Instance
        {
            get
            {
                if (_wesDesktop == null)
                {
                    lock (_locker)
                    {
                        if (_wesDesktop == null)
                        {
                            _wesDesktop = new WesDesktop();
                        }
                    }
                }
                return _wesDesktop;
            }
        }

        public WesDesktop()
        {
            this.Version = DesktopVersion.GetVersionNo();
            this.ReleaseDate = DesktopVersion.GetVersionPublishDate();
            this.MacAddress = Utils.GetLocalMacAddress();
            this.IP = Utils.GetMachineIP();
            OnPropertyChanged();
        }

        public string Version { get; private set; }
        public string ReleaseDate { get; private set; }
        public string MacAddress { get; private set; }
        public string IP { get; private set; }

        private UserInfo _userInfo;
        private Authority _authority;
        private AddIn _addIn;

        /// <summary>
        /// 用户登录相关信息
        /// </summary>
        public UserInfo User
        {
            get { return _userInfo; }
        }

        /// <summary>
        /// 用户权限
        /// </summary>
        public Authority Authority
        {
            get { return _authority; }
        }

        /// <summary>
        /// 当前插件信息
        /// </summary>
        public AddIn AddIn
        {
            get { return _addIn; }
            private set
            {
                _addIn = value;
            }
        }

        public string AddInName
        {
            get
            {
                return AddIn != null ? AddIn.Name : null;
            }
        }

        public void LogIn(string user, string password, string station, string userName, string uid)
        {
            _userInfo = new UserInfo(user, password, station, userName, uid);
            OnPropertyChanged();
        }

        /// <summary>
        /// 设置权限
        /// </summary>
        /// <param name="isDelete"></param>
        /// <param name="isPrint"></param>
        /// <param name="isNotCheckLogNo"></param>
        public void UpdateAuthority(int isDelete, int isPrint, int isNotCheckLogNo)
        {
            _authority = new Authority(isDelete, isPrint, isNotCheckLogNo);
            OnPropertyChanged();
        }

        /// <summary>
        /// 設置插件信息
        /// </summary>
        /// <param name="addin"></param>
        /// <param name="endCustomer"></param>
        /// <param name="endCustomerName"></param>
        public void UpdateAddIn(string addin, string endCustomer, string endCustomerName)
        {
            _addIn = new AddIn(addin, endCustomer, endCustomerName);
            AppPath.AddinName = _addIn.Name;
            OnAddInChanged(_addIn.Name);
            OnPropertyChanged(1);
        }

        public void ResetAddIn()
        {
            _addIn = null;
            AppPath.AddinName = null;
            OnPropertyChanged();
        }

        public void LogOut()
        {
            _userInfo = null;
            _authority = null;
            ResetAddIn();
            OnPropertyChanged();
        }

        public Action<int> PropertyChanged;
        public void OnPropertyChanged(int mode = 0)
        {
            WesApp.SystemInfomation = DynamicJson.SerializeObject(this, new string[] { "PropertyChanged", "AddInChanged", "Supports", "Password" }, false);
            PropertyChanged?.Invoke(mode);
        }

        public delegate void AddInHandle(string name);
        public event AddInHandle AddInChanged;
        public void OnAddInChanged(string name)
        {
            if (_addIn != null)
                AddInChanged?.Invoke(_addIn.Name);
        }
    }

    /// <summary>
    /// 当前用户信息
    /// </summary>
    public class UserInfo
    {
        public UserInfo(string user, string password, string station, string userName, string uid)
        {
            this.Code = user;
            this.Uid = uid;
            this.Password = password;
            this.Station = station;
            this.UserName = userName;
        }

        public string Code { get; private set; }

        public string Uid { get; private set; }

        private string _password = string.Empty;

        public string Password
        {
            get
            {
#if DEBUG
                return _password;
#else
                return "******";
#endif
            }
            set
            {
                _password = value;
            }
        }

        public string UserName { get; private set; }

        public string Station { get; private set; }

    }

    /// <summary>
    /// 权限
    /// </summary>
    public class Authority
    {
        public Authority(int isDelete, int isPrint, int isNotCheckLogNo)
        {
            this.IsDelete = isDelete;
            this.IsPrint = isPrint;
            this.IsNotCheckLogNo = isNotCheckLogNo;
        }

        public int IsDelete { get; private set; }

        public int IsPrint { get; private set; }

        public int IsNotCheckLogNo { get; private set; }
    }

    /// <summary>
    /// 插件相关信息
    /// </summary>
    public class AddIn
    {
        public AddIn()
        {
            this.Supports = new Supports();
        }

        public AddIn(string name, string endCustomer, string endCustomerName)
        {
            this.Name = name;
            this.EndCustomer = endCustomer;
            this.EndCustomerName = endCustomerName;
            this.Supports = new Supports();
        }

        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name { get; set; } = "";

        public string EndCustomer { get; set; } = "";

        public string EndCustomerName { get; set; } = "";

        /// <summary>
        /// 支援
        /// </summary>
        public Supports Supports { get; private set; }

        public bool IsCurrentEndCustomer(string orderEndCustomerCode, out string errMsg)
        {
            errMsg = string.Empty;
            //只能處理當前插件客戶單號
            if (!EndCustomer.Equals(orderEndCustomerCode, StringComparison.CurrentCultureIgnoreCase))
            {
                errMsg = string.Format("AddinEndCustomerNotMatch".GetLanguage(), EndCustomerName);
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 支援
    /// </summary>
    public class Supports
    {
        public Supports()
        {
            this.Users = new Dictionary<string, SupportUser>();
        }

        /// <summary>
        /// 获取支援用户列表
        /// </summary>
        public Dictionary<string, SupportUser> Users { get; private set; }


        /// <summary>
        /// 获取支援用户列表包含当前登录用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="flow"></param>
        /// <returns></returns>
        public Dictionary<string, SupportUser> GetSupports(WesFlowID flow)
        {
            return GetSupports(WesDesktop.Instance.User.Code, flow);
        }

        /// <summary>
        /// 获取支援用户
        /// 並且增加当前登录用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="flow"></param>
        /// <returns></returns>
        public Dictionary<string, SupportUser> GetSupports(string user, WesFlowID flow)
        {
            Dictionary<string, SupportUser> users = new Dictionary<string, SupportUser>();
            foreach (var item in Users)
            {
                users[item.Key] = item.Value;
            }

            if (users.ContainsKey(user))
            {
                users[user].Flows.Add(flow);
            }
            else
            {
                users[user] = new SupportUser()
                {
                    UserCode = user,
                    Flows = new HashSet<WesFlowID>() { flow },
                };
            }
            return users;
        }

        /// <summary>
        /// 添加用户支援,包含用户支援的流程
        /// </summary>
        /// <param name="user"></param>
        /// <param name="flow"></param>
        public void Add(string user, WesFlowID flow)
        {
            if (this.Users.ContainsKey(user))
            {
                this.Users[user].Flows.Add(flow);
            }
            else
            {
                this.Users[user] = new SupportUser()
                {
                    UserCode = user,
                    Flows = new HashSet<WesFlowID>() { flow },
                };
            }
        }

        /// <summary>
        /// 取消支援
        /// </summary>
        /// <param name="user"></param>
        /// <param name="flow"></param>
        public void Remove(string user, WesFlowID flow)
        {
            if (this.Users.ContainsKey(user))
            {
                this.Users[user].Flows.Remove(flow);
                if (this.Users[user].Flows.Count == 0)
                {
                    this.Users.Remove(user);
                }
            }
        }
    }

    /// <summary>
    /// 支援人员
    /// </summary>
    public class SupportUser
    {
        /// <summary>
        /// 支援类型
        /// </summary>
        public HashSet<WesFlowID> Flows { get; set; }

        public string UserCode { get; set; }
    }
}
