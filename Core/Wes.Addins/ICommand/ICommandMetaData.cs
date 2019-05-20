using System;
using System.ComponentModel.Composition;

namespace Wes.Addins.ICommand
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ComponentsCommandAttribute : ExportAttribute, ICommandMetaData
    {
        public ComponentsCommandAttribute()
        {

        }
        /// <summary>
        /// 命令描述 定义在Wes.Flow中
        /// </summary>
        public string CommandName { get; set; }
        /// <summary>
        /// 命令显示的顺序 Command加载顺序
        /// </summary>
        public int CommandIndex { get; set; }
    }

    public interface ICommandMetaData
    {
        /// <summary>
        /// 命令描述
        /// </summary>
        string CommandName { get; }
        /// <summary>
        /// 命令显示的顺序
        /// </summary>
        int CommandIndex { get; }
    }
}
