using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wes.Core.Base;
using Wes.Core.Bean;

namespace Wes.Core.VirtualAction
{
    public class ActionService
    {
        private static Dictionary<string, IEnumerable<Lazy<IScanAction, IActionMetadata>>> _virtualActions = new Dictionary<string, IEnumerable<Lazy<IScanAction, IActionMetadata>>>();

        private static IEnumerable<Lazy<IScanAction, IActionMetadata>> GetVirtualActoin(Assembly assembly, string mode)
        {
            string key = $"{assembly.GetName().Name}_{mode}";

            IEnumerable<Lazy<IScanAction, IActionMetadata>> virtualAction = null;
            if (_virtualActions.ContainsKey(key))
            {
                virtualAction = _virtualActions[key];
            }
            else
            {
                virtualAction = CoreComposition.ExportComposition(assembly).GetExports<IScanAction, IActionMetadata>(mode);

                if (virtualAction != null && virtualAction.Count() > 0)
                {
                    List<Lazy<IScanAction, IActionMetadata>> temp = new List<Lazy<IScanAction, IActionMetadata>>();
                    foreach (var item in virtualAction)
                    {
                        var type = item.Value.GetType();
                        var iScanAction = ViewModelFactory.CreateActionForType(type) as IScanAction;
                        Lazy<IScanAction, IActionMetadata> lazy = new Lazy<IScanAction, IActionMetadata>(new Func<IScanAction>(() => iScanAction), item.Metadata);
                        temp.Add(lazy);
                    }
                    _virtualActions[key] = temp;
                }
            }
            return virtualAction;
        }

        public static Dictionary<string, IScanAction> GetVirtualActoins(Assembly assembly, string mode)
        {
            string key = $"{assembly.GetName().Name}_{mode}";

            GetVirtualActoin(assembly, mode);
            Dictionary<string, IScanAction> actions = new Dictionary<string, IScanAction>();
            IEnumerable<Lazy<IScanAction, IActionMetadata>> virtualAction = null;
            if (_virtualActions.ContainsKey(key))
            {
                virtualAction = _virtualActions[key];
                if (virtualAction != null)
                {
                    foreach (var item in virtualAction)
                    {
                        actions[item.Metadata.Type] = item.Value;
                    }
                }
            }
            return actions;
        }

    }
}
