using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Flow;

namespace Wes.Customer.Common
{
    [Export(typeof(IKPICommand))]
    [ComponentsCommand(CommandName = "KPI", CommandIndex = 0)]
    public class AmoebaKpiManager : IKPICommand
    {
        readonly List<long> initAbilityIdList = new List<long>();
        readonly List<long> addKpiAbilityIdList = new List<long>();

        private HashSet<long> _actionHandlerId = new HashSet<long>();

        public HashSet<long> ActionHandlerId
        {
            get { return _actionHandlerId; }
            set { _actionHandlerId = value; }
        }

        public object Initialize(object args)
        {
            foreach (long listenActionId in Enum.GetValues(typeof(EnumListeningActionId)))
            {
                ActionHandlerId.Add(listenActionId);
            }

            foreach (string name in Enum.GetNames(typeof(EnumListeningActionId)))
            {
                var listenAbilityId = (long)Enum.Parse(typeof(EnumListeningActionId), name);
                if (name.Contains("AddKpiId"))
                {
                    addKpiAbilityIdList.Add(listenAbilityId);
                }
            }
            return this;
        }

        public object Execute(object obj)
        {
            if (obj == null) return this;

            ActionDefinition actionDefinition = obj as ActionDefinition;

            if (actionDefinition == null) return this;

            int flowId = WesFlow.Instance.GetFlowID(actionDefinition.FlowName);
            int flowMask = WesFlow.Instance.GetFlowMask(flowId);

            if (addKpiAbilityIdList.Contains(actionDefinition.AbilityID))
            {
                //添加KPI操作记录
                AmoebaKpiHelper.AddTeamKpi(actionDefinition);
            }
            return this;
        }
    }
}
