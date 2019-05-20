using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Wes.Addins.ICommand;
using Wes.Core.Service;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Controls;
using Wes.Desktop.Windows.DebugCommand;
using Wes.Desktop.Windows.View;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Desktop.Menu
{
    public class MenuService : IMenu
    {
        private readonly string GeometryData_LastName = "_GEOMETRY";

        public Action AddinChangedCallback { get; set; }

        private ContextMenu _contextMenu;

        #region main menu

        public List<NavButton> InitDesktopNavigator()
        {
            List<NavButton> navButtons = new List<NavButton>();
            var menuCommands = LoadFlowMudule.Instance.MuduleViews;
            var groupBy = menuCommands.GroupBy(kvp => WesFlow.Instance.GetFlowMask(kvp.Key.CommandName))
                .OrderBy(kvp => kvp.Key);
            foreach (var item in groupBy)
            {
                navButtons.Add(CreateNavButton(item.Key, WesFlow.Instance.GetFlowName(item.Key)));
            }

            return navButtons;
        }

        public List<NavButton> InitLeftNavigator()
        {
            List<NavButton> navButtons = new List<NavButton>();
            var conf = ConfigurationMapping.Instance.GlobalConf;
            foreach (var item in conf.leftMenu.menu)
            {
                navButtons.Add(CreateLeftNavButton(item));
            }

            return navButtons;
        }

        private NavButton CreateNavButton(int flowId, string key)
        {
            NavButton navButton = new NavButton();
            navButton.FlowID = flowId;
            navButton.FlowKey = key;
            Binding binding = new Binding("Value");
            binding.Source = new Wes.Utilities.Languages.LanguageExtension(key);
            BindingOperations.SetBinding(navButton, NavButton.TextProperty, binding);
            navButton.IconData = Application.Current.Resources[key + GeometryData_LastName].ToString();
            navButton.MouseLeftButtonDown += OutNav_MouseLeftButtonDown;
            return navButton;
        }

        private NavButton CreateLeftNavButton(dynamic menuItem)
        {
            NavButton navButton = new NavButton();
            navButton.Width = 200;
            navButton.Height = 200;
            navButton.MinWidth = 200;
            navButton.MinHeight = 200;
            navButton.Margin = new Thickness(0, 10, 0, 0);
            navButton.FlowKey = menuItem.name.ToString();
            navButton.ToolTip = menuItem.tooltip != null ? menuItem.tooltip.ToString() : null;
            Binding binding = new Binding("Value");
            binding.Source = new Wes.Utilities.Languages.LanguageExtension(menuItem.name.ToString());
            BindingOperations.SetBinding(navButton, NavButton.TextProperty, binding);
            navButton.IconData = Application.Current.Resources[menuItem.icon.ToString()].ToString();
            if (Convert.ToBoolean(menuItem.isCommand) == false)
            {
                navButton.MouseLeftButtonDown += (obj, e) =>
                {
                    try
                    {
                        #region webkit控件暫時擱置，安裝包過大

                        //if (DotnetDetection.IsDotnet452Installed())
                        //{
                        //    Process[] procs = Process.GetProcessesByName("wesWebBrowser");
                        //    if (procs == null || procs.Length == 0)
                        //    {
                        //        Process proc = new Process();
                        //        proc.StartInfo.FileName = System.IO.Path.Combine(AppPath.WebKitPath, "WesWebBrowser.exe");
                        //        proc.StartInfo.WorkingDirectory = AppPath.WebKitPath;
                        //        proc.StartInfo.Arguments = $"/title {menuItem.name.ToString()} /uri {menuItem.uri.ToString()}";
                        //        proc.Start();
                        //    }
                        //    else
                        //    {
                        //        var proc = procs[0];
                        //        string message = $"/title {menuItem.name.ToString()} /uri {menuItem.uri.ToString()}";
                        //        byte[] data = System.Text.Encoding.Default.GetBytes(message);
                        //        COPYDATASTRUCT copyData = new COPYDATASTRUCT();
                        //        copyData.dwData = IntPtr.Zero;
                        //        copyData.cbData = data.Length;
                        //        copyData.lpData = message;
                        //        WindowsAPI.SendMessage((int)proc.MainWindowHandle, WindowsAPI.WM_COPYDATA, 0, ref copyData);
                        //        WindowsAPI.SetForegroundWindow((int)proc.MainWindowHandle);
                        //    }
                        //}
                        //else
                        //{
                        //    WesModernDialog.ShowWesMessage("當前系統需要.NET Framework 4.5.2以上版本，建議您安裝后使用，本次系統將使用瀏覽器打開子系統");
                        //    System.Diagnostics.Process.Start(menuItem.uri.ToString());
                        //}

                        #endregion

                        var queryParams = string.Format("&_a=(query:(language:lucene,query:'{0}'))",
                            WesDesktop.Instance.User.Code);
                        if (menuItem.uri != null)
                        {
                            string webUri = null;
                            if (menuItem.id != null && menuItem.id == "1c79964c-fbeb-4222-8de5-68ae83a1602d")
                                webUri = menuItem.uri.ToString() + queryParams;
                            else
                                webUri = menuItem.uri.ToString();
                            System.Diagnostics.Process.Start(webUri);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error(ex);
                    }
                };
            }
            else if (menuItem.command != null)
            {
                navButton.MouseLeftButtonDown += (obj, e) =>
                {
                    DebugCommandService.ExecuteCommand(menuItem.command.ToString());
                };
            }

            return navButton;
        }

        private void OutNav_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavButton navButton = sender as NavButton;
            MenuServiceHelper.OpenFlowWindow(navButton.FlowID);
        }

        #endregion

        #region contextMenu

        public ContextMenu CreateContextMenu(UIElement placementTarget)
        {
            if (_contextMenu == null)
            {
                var contextMenu = new ContextMenu();
                contextMenu.PlacementTarget = placementTarget;
                contextMenu.Placement = PlacementMode.Bottom;

                if (WesMain.UseDebugAttach)
                {
                    var customer = new MenuItem { Header = "AddInName".GetLanguage() };
                    CreateAddinMenu(customer);
                    contextMenu.Items.Add(customer);
                }

                var menuCommands = LoadFlowMudule.Instance.MuduleViews;
                var groupBy = menuCommands.GroupBy(kvp => WesFlow.Instance.GetFlowMask(kvp.Key.CommandName))
                    .OrderBy(kvp => kvp.Key);
                MenuItem menuItem = null;
                foreach (var item in groupBy)
                {
                    menuItem = new MenuItem();
                    menuItem.Header = WesFlow.Instance.GetFlowName(item.Key).GetLanguage();
                    contextMenu.Items.Add(menuItem);
                    CreateFlowMenuItem(menuItem, item);
                }

                List<object> menuItems = BuildMainMenu();
                if (contextMenu.Items.Count == 0) menuItems.RemoveAt(0);
                foreach (var item in menuItems)
                    contextMenu.Items.Add(item);
                _contextMenu = contextMenu;
            }
            return _contextMenu;
        }

        private List<object> BuildMainMenu()
        {
            var menus = MenuServiceHelper.GetMenuCommands();
            return BuildMenuItems(menus.Where(m => m.Metadata.IsDisplay).OrderBy(m => m.Metadata.Order));
        }

        private List<object> BuildMenuItems(IEnumerable<Lazy<object, IMenuCommandMetadata>> commands)
        {
            List<object> menuItems = new List<object>();
            foreach (var entry in commands)
            {
                if (!string.IsNullOrEmpty(entry.Metadata.Parent)) continue;

                if (entry.Metadata.Type == "Menu")
                {
                    if (entry.Metadata.AppendSeparator == MenuAppendType.Front)
                    {
                        Separator separator = new Separator();
                        menuItems.Add(separator);
                    }

                    MenuItem menuItem = new MenuItem { Header = entry.Metadata.Header.GetLanguage() };
                    menuItem.IsEnabled = entry.Metadata.IsEnabled;
                    menuItem.Command = MenuCommandWrapper.Unwrap((ICommand)entry.Value);
                    if (!string.IsNullOrEmpty(entry.Metadata.Icon))
                    {
                        menuItem.Icon = CreateMenuIconPath(entry.Metadata.Icon);
                        menuItem.MouseEnter += (obj, e) =>
                        {
                            MenuItem menu = obj as MenuItem;
                            Path path = menu.Icon as Path;
                            path.Fill = Brushes.White;
                        };
                        menuItem.MouseLeave += (obj, e) =>
                        {
                            MenuItem menu = obj as MenuItem;
                            Path path = menu.Icon as Path;
                            path.Fill = new SolidColorBrush(AppearanceManager.Current.AccentColor);
                        };
                    }

                    menuItems.Add(menuItem);

                    if (entry.Metadata.AppendSeparator == MenuAppendType.Back)
                    {
                        Separator separator = new Separator();
                        menuItems.Add(separator);
                    }
                }
                else if (entry.Metadata.Type == "MenuItem")
                {
                    MenuItem menuItem = new MenuItem { Header = entry.Metadata.Header.GetLanguage() };
                    menuItem.IsEnabled = entry.Metadata.IsEnabled;
                    var childItems = BuildMenuItems(commands.Where(c => c.Metadata.Parent == entry.Metadata.Header)
                        .OrderBy(m => m.Metadata.Order));
                    if (childItems != null)
                    {
                        foreach (var item in childItems)
                            menuItem.Items.Add(item);
                    }

                    menuItems.Add(menuItem);
                }
            }

            return menuItems;
        }

        private Path CreateMenuIconPath(string geometryKey)
        {
            Path path = new Path();
            string data = Application.Current.Resources[geometryKey].ToString();
            var converter = TypeDescriptor.GetConverter(typeof(Geometry));
            path.Data = (Geometry)converter.ConvertFrom(data);
            path.Height = 16;
            path.Width = 16;
            path.Fill = new SolidColorBrush(AppearanceManager.Current.AccentColor);
            path.Stretch = Stretch.Uniform;
            return path;
        }

        private void CreateAddinMenu(MenuItem menuItem)
        {
            var conf = ConfigurationMapping.Instance.GlobalConf;
            dynamic addIns = conf.addin;
            foreach (var item in addIns.mapping)
            {
                string name = item.name.ToString();
                if (item.menuAppend != null && item.menuAppend == (int)MenuAppendType.Front)
                {
                    Separator separator = new Separator();
                    menuItem.Items.Add(separator);
                }

                var menu = new MenuItem()
                {
                    Header = name,
                    Tag = name,
                    IsChecked = string.Compare(name, WesDesktop.Instance.AddInName, true) == 0,
                };
                WesDesktop.Instance.AddInChanged += (n) =>
                {
                    menu.IsChecked = string.Compare(name, n, true) == 0;
                };

                menu.Click += (obj, e) =>
                {
                    if (WindowHelper.IsWorking()) return;

                    MenuItem childMenuItem = obj as MenuItem;
                    string addin = childMenuItem.Tag.ToString();
                    if (string.Compare(addin, WesDesktop.Instance.AddInName, true) != 0)
                    {
                        WS.GetService<IStartup>().UpdateAddIn(addin);
                        AddinChangedCallback?.Invoke();
                    }
                };
                menuItem.Items.Add(menu);

                if (item.menuAppend != null && item.menuAppend == (int)MenuAppendType.Back)
                {
                    Separator separator = new Separator();
                    menuItem.Items.Add(separator);
                }
            }
        }

        private void CreateFlowMenuItem(MenuItem menuItem, IGrouping<int, KeyValuePair<ICommandMetaData, object>> items)
        {
            if (items != null && items.Count() > 0)
            {
                var orderBy = items.OrderBy(kvp => kvp.Key.CommandIndex);
                foreach (var item in orderBy)
                {
                    var childMenu = new MenuItem
                    {
                        Header = item.Key.CommandName.GetLanguage(),
                        IsEnabled = WesMain.IsEnabled
                    };
                    childMenu.CommandParameter = WesFlow.Instance.GetFlowID(item.Key.CommandName);
                    childMenu.Command = MenuCommandWrapper.Unwrap(MenuServiceHelper.GetFlowCommand());
                    menuItem.Items.Add(childMenu);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 打开流程窗口
    /// </summary>
    public static class MenuServiceHelper
    {
        private static IEnumerable<Lazy<object, IMenuCommandMetadata>> _menuItems = null;

        public static IEnumerable<Lazy<object, IMenuCommandMetadata>> GetMenuCommands()
        {
            if (_menuItems == null)
                _menuItems =
                    CommandComposition.DesktopContainer.GetExports<object, IMenuCommandMetadata>("MenuCommand");
            return _menuItems;
        }

        public static ICommand GetFlowCommand()
        {
            if (_menuItems != null)
                return _menuItems.Where(l => l.Metadata.Header == "Flow").First().Value as ICommand;
            return null;
        }

        public static void OpenFlowWindow(object flow, List<string> workNos = null)
        {
            int childFlowId = Convert.ToInt32(flow);
            int flowId = WesFlow.Instance.GetFlowMask(childFlowId);
            LoggingService.InfoFormat("Open flow window:{0}", flow.ToString());
            var window = IsFlowWindowOpened(flowId);
            if (window != null)
            {
                if (window is WesFlowWindow)
                {
                    WesFlowWindow flowWindow = window as WesFlowWindow;
                    if (childFlowId > 0)
                        flowWindow.ChildFlowID = childFlowId;
                    flowWindow.InitializeWorkNo(workNos);
                }

                window.WindowState = WindowState.Maximized;
                window.Activate();
            }
            else
            {
                var items = LoadFlowMudule.Instance.GetMuduleViews(flowId);
                if (items != null && items.Count > 0)
                {
                    WesFlowWindow wesFlowWindow = new WesFlowWindow();
                    Binding binding = new Binding("Value");
                    binding.Source =
                        new Wes.Utilities.Languages.LanguageExtension(WesFlow.Instance.GetFlowName(flowId));
                    BindingOperations.SetBinding(wesFlowWindow, WesFlowWindow.TitleProperty, binding);
                    wesFlowWindow.FlowID = flowId;
                    wesFlowWindow.InitFlows(items);
                    if (childFlowId > 0) wesFlowWindow.ChildFlowID = childFlowId;
                    wesFlowWindow.Show();
                    wesFlowWindow.InitializeWorkNo(workNos);
                }
                else
                {
                    WesModernDialog.ShowWesMessage(string.Format("Addin_Unrealize".GetLanguage(),
                        WesDesktop.Instance.AddInName));
                }
            }
        }

        private static Window IsFlowWindowOpened(int flowID)
        {
            var windows = WindowHelper.GetOpenedWindows<WesFlowWindow>();
            var query = windows.Where(w => w.FlowID == flowID);
            if (query.Count() > 0)
                return query.FirstOrDefault();
            return null;
        }
    }
}