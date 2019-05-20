using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Wes.Desktop.Windows.View;
using Wes.Utilities;

namespace Wes.Desktop.Windows.DebugCommand
{
    [ExportDebugCommand(DebugID = "spread-memory", Description = "change flow memory data. eg spread-memory withoutCheck=true")]
    public class SysFlowMemoryCommand : BaseDebugCommand
    {
        public override void Execute(object parameter)
        {
            if (ExecuteParameter(this, parameter))
                return;
        }

        protected override bool AfterExecuteParameter(List<string> parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                string data = parameters[0];
                try
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        string[] props = data.Split('=');
                        if (props != null && props.Length == 2)
                        {
                            Window window = WindowHelper.GetActivedWindow();
                            if (window != null && window is WesFlowWindow)
                            {
                                WesFlowWindow flowWindow = window as WesFlowWindow;
                                object viewModel = flowWindow.ActionViewModel;
                                if (viewModel != null)
                                {
                                    PropertyInfo prop = viewModel.GetType().GetProperty("SelfInfo");
                                    dynamic dynamicObject = prop.GetValue(viewModel, null) as dynamic;
                                    MethodInfo methodInfo = dynamicObject.GetType().GetMethod("TrySetMember");
                                    methodInfo.Invoke(dynamicObject, new object[] { new SetDynamicMemberBinder(props[0], false), props[1] });
                                    LoggingService.Info($"memory data reseted:SelfInfo.{props[0]}={props[1]}");
                                }
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LoggingService.Warn("memory data reset failed. message:" + ex.Message);
                }
            }
            return false;
        }
    }

}
