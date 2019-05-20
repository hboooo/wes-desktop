using System;
using System.ComponentModel.Composition;

namespace Wes.Server.Listener
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HttpRequestAttribute : ExportAttribute, IHttpRequestMetadata
    {
        /// <summary>
        /// http请求
        /// </summary>
        /// <param name="contractName">请求模块</param>
        public HttpRequestAttribute(string contractName) : base(contractName, typeof(IKernelRequest))
        {

        }

        /// <summary>
        /// 请求接口
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 保留
        /// </summary>
        public string Type { get; set; }
    }

    public interface IHttpRequestMetadata
    {
        /// <summary>
        /// 请求接口
        /// </summary>
        string Action { get; }
        /// <summary>
        /// 保留
        /// </summary>
        string Type { get; }
    }
}
