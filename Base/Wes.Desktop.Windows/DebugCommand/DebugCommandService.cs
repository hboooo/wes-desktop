using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Wes.Utilities;

namespace Wes.Desktop.Windows.DebugCommand
{
    public class DebugCommandService
    {
        private static IEnumerable<Lazy<ICommand, IDebugCommandMetadata>> _debugCommandItems = null;

        private static IEnumerable<Lazy<ICommand, IDebugCommandMetadata>> GetDebugCommands()
        {
            if (_debugCommandItems == null)
                _debugCommandItems = CommandComposition.ControlContainer.GetExports<ICommand, IDebugCommandMetadata>("DebugCommand");
            return _debugCommandItems;
        }

        public static string GetDebugHelpInfo()
        {
            GetDebugCommands();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("\r\n********************* colorful egg *********************");
            foreach (var item in _debugCommandItems)
            {
                builder.AppendLine(string.Format("{0}   --{1}", item.Metadata.DebugID.PadRight(20, ' '), item.Metadata.Description));
            }
            builder.AppendLine("*******自定義菜單:參考類SysScanHistoryCommand***********");
            builder.Append("********************************************************");
            return builder.ToString();
        }

        public static bool Debug(string inputValue, object sender = null)
        {
            GetDebugCommands();
            var query = _debugCommandItems.Where(c => c.Metadata.DebugID == inputValue.Split(' ')[0]);
            if (query.Count() > 0)
            {
                var paramObj = new
                {
                    input = inputValue,
                    metadata = query.FirstOrDefault().Metadata,
                    control = sender,
                };
                try
                {
                    LoggingService.Debug($"Debug command:{inputValue}");
                    query.FirstOrDefault().Value.Execute(paramObj);
                }
                catch (Exception ex)
                {
                    LoggingService.Error(ex);
                }
                return true;
            }
            return false;
        }

        public static bool ExecuteCommand(string type)
        {
            GetDebugCommands();
            var query = _debugCommandItems.Where(c => c.Value.GetType().Name == type);
            if (query.Count() > 0)
            {
                try
                {
                    var paramObj = new
                    {
                        input = "",
                        metadata = query.FirstOrDefault().Metadata,
                        control = default(dynamic),
                    };
                    LoggingService.Debug($"left navigate menu:execute command:{type}");
                    query.FirstOrDefault().Value.Execute(paramObj);
                    return true;
                }
                catch (Exception ex)
                {
                    LoggingService.Error(ex);
                }
            }
            return false;
        }
    }
}
