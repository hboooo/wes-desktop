using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core.Service;

namespace Wes.Desktop.Windows
{
    [WSService("Wes.Desktop.Windows", Implementation = typeof(Startup))]
    public interface IStartup
    {
        /// <summary>
        /// WES启动时初始化
        /// </summary>
        void Run();
        /// <summary>
        /// 初始化插件项目
        /// </summary>
        void InstallAddIn();
        /// <summary>
        /// 根据客户编码获取客户
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        string GetAddInFromCode(string code);
        /// <summary>
        /// 刷新客户信息
        /// </summary>
        /// <param name="addinName"></param>
        /// <returns></returns>
        bool UpdateAddIn(string addinName);
        /// <summary>
        /// 注册积分Action
        /// </summary>
        /// <param name="addinName"></param>
        void RegisterListenInvoker(string addinName);
    }
}
