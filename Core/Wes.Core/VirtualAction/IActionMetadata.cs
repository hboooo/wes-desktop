using System;
using System.ComponentModel.Composition;
using Wes.Core.Base;

namespace Wes.Core.VirtualAction
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VirtualActionAttribute : ExportAttribute, IActionMetadata
    {
        /// <summary>
        /// 子Action特性
        /// </summary>
        /// <param name="contractName">父类类型名，如avnet采集： 父类类型名GatherAction</param>
        public VirtualActionAttribute(string contractName) : base(contractName, typeof(IScanAction))
        {

        }

        public string Type { get; set; }

        public string Mode { get; set; }
    }

    public interface IActionMetadata
    {
        /// <summary>
        /// 類型,如:shipper,customer
        /// </summary>
        string Type { get; }
        /// <summary>
        /// 保留
        /// </summary>
        string Mode { get; }
    }
}
