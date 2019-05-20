using System;
using System.ComponentModel.Composition;
using Wes.Desktop.Windows;

namespace Wes.Desktop.Menu
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportMenuCommandAttribute : ExportAttribute, IMenuCommandMetadata
    {
        public ExportMenuCommandAttribute() : base("MenuCommand", typeof(object))
        {

        }

        public string Header { get; set; }

        public string Tooltip { get; set; }

        public bool IsEnabled { get; set; } = true;

        public int Flow { get; set; }

        public int Order { get; set; }

        public bool IsDisplay { get; set; } = true;

        public string Type { get; set; } = "Menu";

        public string Parent { get; set; }

        public string Icon { get; set; }

        public MenuAppendType AppendSeparator { get; set; } = 0;
    }

    public interface IMenuCommandMetadata
    {
        string Header { get; }

        string Tooltip { get; }

        bool IsEnabled { get; }

        int Flow { get; }

        int Order { get; }

        bool IsDisplay { get; }

        string Type { get; }

        string Parent { get; }

        string Icon { get; }

        MenuAppendType AppendSeparator { get; }
    }

}
