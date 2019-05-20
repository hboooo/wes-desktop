using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Wes.Core.Service;
using Wes.Desktop.Windows.Controls;

namespace Wes.Desktop.Menu
{
    [WSService("Wes.Desktop", Implementation = typeof(MenuService))]
    public interface IMenu
    {
        /// <summary>
        /// 插件切换后的回调
        /// </summary>
        Action AddinChangedCallback { get; set; }
        /// <summary>
        /// 加载主页面导航
        /// </summary>
        /// <returns></returns>
        List<NavButton> InitDesktopNavigator();
        /// <summary>
        /// 加载主页面左浮动栏导航
        /// </summary>
        /// <returns></returns>
        List<NavButton> InitLeftNavigator();
        /// <summary>
        /// 创建右键菜单
        /// </summary>
        /// <param name="placementTarget"></param>
        /// <returns></returns>
        ContextMenu CreateContextMenu(UIElement placementTarget);

    }
}
