﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Desktop.Windows.KPI;

namespace Wes.Customer.Sinbon.KPICommand
{
    [Export(typeof(IKPICommand))]
    [ComponentsCommand(CommandName = "KPI", CommandIndex = 1)]
    public class ActionKPICommand : AmoebaKpiHandler, IKPICommand
    {
        public HashSet<long> ActionHandlerId { get; set; } = new HashSet<long>();

        public object Initialize(object args)
        {
            return this;
        }

        public object Execute(object obj)
        {
            base.ExecuteKpi(obj);
            return this;
        }
    }
}
