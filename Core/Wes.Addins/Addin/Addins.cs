using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Wes.Addins.ICommand;

namespace Wes.Addins.Addin
{
    public class Addins
    {
        [ImportMany]
        public IEnumerable<Lazy<IViewModelCommand, ICommandMetaData>> ViewModelCommandList;

        [ImportMany]
        public IEnumerable<Lazy<IViewCommand, ICommandMetaData>> ViewCommandList;

        [ImportMany]
        public IEnumerable<Lazy<IKPICommand, ICommandMetaData>> KPICommandList;
    }
}
