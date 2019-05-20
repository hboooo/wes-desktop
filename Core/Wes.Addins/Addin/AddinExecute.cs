using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Wes.Addins.ICommand;
using Wes.Utilities;

namespace Wes.Addins.Addin
{
    public class AddinExecute
    {
        private Addins _addins = null;

        public static readonly AddinExecute Instance = new AddinExecute();

        private AddinExecute()
        {

        }

        public void ReleaseAddins()
        {
            if (_addins == null) return;

            if (_addins.ViewCommandList != null)
            {
                AddinManager.AddinContainer.ReleaseExports(_addins.ViewCommandList);
                _addins.ViewCommandList = null;
            }
            if (_addins.ViewModelCommandList != null)
            {
                AddinManager.AddinContainer.ReleaseExports(_addins.ViewModelCommandList);
                _addins.ViewModelCommandList = null;
            }
            if (_addins.KPICommandList != null)
            {
                AddinManager.AddinContainer.ReleaseExports(_addins.KPICommandList);
                _addins.KPICommandList = null;
            }
            LoggingService.Debug("Release addins");
        }

        private string PrepareAddInName(params object[] args)
        {
            string addinName = null;
            if (args != null && args.Length > 0)
                addinName = args[0].ToString();
            else
                LoggingService.Debug("Addin name can not find");
            return addinName;
        }

        private bool CheckAddinContext(Addins addins)
        {
            if (addins == null)
            {
                OnCommandLoadProgress(new CommandLoadEventArgs() { Total = 0, Complated = 0 });
                LoggingService.Debug("Addin 未初始化,請聯繫管理員");
                return false;
            }
            else if (addins.ViewCommandList == null || addins.ViewCommandList.Count() == 0)
            {
                OnCommandLoadProgress(new CommandLoadEventArgs() { Total = 0, Complated = 0 });
                LoggingService.Debug("Addin view 加载失败,請聯繫管理員");
                return false;
            }
            else if (addins.ViewModelCommandList == null || addins.ViewModelCommandList.Count() == 0)
            {
                OnCommandLoadProgress(new CommandLoadEventArgs() { Total = 0, Complated = 0 });
                LoggingService.Debug("Addin command 加载失败,請聯繫管理員");
                return false;
            }
            return true;
        }

        public Dictionary<ICommandMetaData, object> MainExecute(params object[] args)
        {
            Dictionary<ICommandMetaData, object> addinList = new Dictionary<ICommandMetaData, object>();
            string addinName = PrepareAddInName(args);
            if (string.IsNullOrEmpty(addinName)) return addinList;
            _addins = AddinManager.LoadAddins<Addins>(Path.Combine(System.Environment.CurrentDirectory, "Addins", addinName));
            if (!CheckAddinContext(_addins)) return addinList;

            var groupBy = _addins.ViewModelCommandList.GroupBy(c => c.Metadata.CommandName);
            foreach (var group in groupBy)
            {
                Lazy<IViewModelCommand, ICommandMetaData> item = null;
                if (group.Count() > 1)
                    item = group.Where(g => !g.Value.GetType().FullName.Contains("Wes.Component.Widgets")).FirstOrDefault();
                else
                    item = group.FirstOrDefault();
                OnCommandLoadProgress(new CommandLoadEventArgs() { CommandMetaData = item.Metadata });

                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    object viewModel = item.Value.Execute(args);
                    var viewCommand = _addins.ViewCommandList.Where(c =>
                           c.Metadata.CommandName == item.Metadata.CommandName && c.Value.GetType().Namespace == item.Value.GetType().Namespace)
                        .FirstOrDefault();
                    if (viewCommand != null)
                    {
                        WesApp.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            object view = viewCommand.Value.Execute(args);
                            if (ComposeView(viewModel, view, item.Metadata.CommandName))
                                addinList[item.Metadata] = view;
                        }));
                    }
                });
                WesApp.WaitThreadPool();
            }

            return addinList;
        }

        public Dictionary<IKPICommand, ICommandMetaData> KPIInitialize(params object[] args)
        {
            Dictionary<IKPICommand, ICommandMetaData> kpiList = new Dictionary<IKPICommand, ICommandMetaData>();

            if (_addins == null)
            {
                LoggingService.DebugFormat("Addin Initialize failed");
                return kpiList;
            }

            if (_addins.KPICommandList == null)
            {
                OnCommandLoadProgress(new CommandLoadEventArgs() { Total = 0, Complated = 0 });
                return kpiList;
            }

            var groupBy = _addins.KPICommandList.GroupBy(c => c.Metadata.CommandName);
            foreach (var group in groupBy)
            {
                Lazy<IKPICommand, ICommandMetaData> item = null;
                if (group.Count() > 1)
                    item = group.Where(g => !g.Value.GetType().FullName.Contains("Wes.Component.Widgets")).FirstOrDefault();
                else
                    item = group.FirstOrDefault();
                object kpi = null;
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    kpi = item.Value.Initialize(args);
                });
                WesApp.WaitThreadPool();
                kpiList[item.Value] = item.Metadata;
            }
            return kpiList;
        }

        private bool ComposeView(object viewModel, object view, string flowName)
        {
            if (viewModel != null && view != null)
            {
                var viewModelType = viewModel.GetType();
                var flowProperty = viewModelType.GetProperty("FlowName");
                if (flowProperty != null)
                {
                    flowProperty.SetValue(viewModel, flowName, null);
                }

                var type = view.GetType();
                var property = type.GetProperty("DataContext");
                if (property != null)
                {
                    property.SetValue(view, viewModel, null);
                    return true;
                }
            }
            return false;
        }

        public event EventHandler<CommandLoadEventArgs> CommandLoadProgress;

        private void OnCommandLoadProgress(CommandLoadEventArgs args)
        {
            if (CommandLoadProgress != null)
            {
                CommandLoadProgress(this, args);
            }
        }
    }
}
