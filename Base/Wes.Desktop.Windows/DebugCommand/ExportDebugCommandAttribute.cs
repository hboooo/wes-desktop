using System;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace Wes.Desktop.Windows.DebugCommand
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportDebugCommandAttribute : ExportAttribute, IDebugCommandMetadata
    {
        public ExportDebugCommandAttribute() : base("DebugCommand", typeof(ICommand))
        {

        }
        public string DebugID { get; set; }

        public string Description { get; set; }
    }

    public interface IDebugCommandMetadata
    {
        string DebugID { get; }

        string Description { get; }
    }
}
