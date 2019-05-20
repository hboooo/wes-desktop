using System;
using System.Collections.Generic;
using Wes.Print.Model;

namespace Wes.Print
{
    public class PrintTemplateModel : IComparable<PrintTemplateModel>
    {
        /// <summary>
        /// 打印数据
        /// 仅支持bartender
        /// </summary>
        public Dictionary<string, object> PrintDataValues { get; set; }

        /// <summary>
        /// 打印數據 json格式
        /// 支持bartender/TFORMer
        /// </summary>
        public dynamic PrintData
        {
            get; set;
        }

        /// <summary>
        /// 打印模板名称,完整路径(如.btw文件)
        /// </summary>
        public string TemplateFile { get; internal set; }

        /// <summary>
        /// 打印模板名称，必填(如.btw文件)
        /// </summary>
        public string TemplateFileName { get; set; }

        /// <summary>
        /// 打印设备名称(默认获取系统默认打印设备)
        /// </summary>
        public string Printer { get; set; }

        /// <summary>
        /// 打印类型
        /// </summary>
        public PrintMode Mode { get; set; } = PrintMode.BarTender;

        public string SortKey { get; set; }

        public int CompareTo(PrintTemplateModel obj)
        {
            if (string.IsNullOrEmpty(SortKey))
            {
                throw new Exception("SortKey 为空,不能排序");
            }
            if (string.IsNullOrEmpty(PrintDataValues[SortKey].ToString()))
            {
                throw new Exception("SortKey 对应的值为空,不能排序");
            }
            return PrintDataValues[SortKey].ToString().CompareTo(obj.PrintDataValues[SortKey]);
        }
    }

    public class PrintParam
    {
        /// <summary>
        /// 初始化窗体
        /// </summary>
        /// <param name="message">显示内容</param>
        public PrintParam(string message)
        {
            this.Message = message;
        }

        /// <summary>
        /// 初始化窗体
        /// </summary>
        /// <param name="isMaskedLayerDisplay">是否顯示提示層</param>
        public PrintParam(bool isMaskedLayerDisplay)
        {
            this.IsMaskedLayerDisplay = isMaskedLayerDisplay;
        }

        /// <summary>
        /// 初始化窗体
        /// </summary>
        /// <param name="isMaskedLayerDisplay">是否顯示提示層</param>
        public PrintParam(string message, bool isMaskedLayerDisplay)
        {
            this.Message = message;
            this.IsMaskedLayerDisplay = isMaskedLayerDisplay;
        }

        /// <summary>
        /// 初始化窗体
        /// </summary>
        /// <param name="message">显示内容</param>
        /// <param name="action">窗体关闭时的回调</param>
        public PrintParam(string message, Action<bool> action)
        {
            this.Message = message;
            this.DialogCallback = action;
        }

        /// <summary>
        /// 初始化窗体
        /// </summary>
        /// <param name="message">显示内容</param>
        /// <param name="timeout">窗体显示时间,单位:秒</param>
        public PrintParam(string message, int timeout)
        {
            this.Message = message;
            this.Timeout = timeout;
        }

        /// <summary>
        /// 初始化窗体
        /// </summary>
        /// <param name="message">显示内容</param>
        /// <param name="timeout">窗体显示时间,单位:秒</param>
        /// <param name="action">窗体关闭时的回调,收到串口数据为true,否则为false</param>
        public PrintParam(string message, int timeout, Action<bool> action)
        {
            this.Message = message;
            this.Timeout = timeout;
            this.DialogCallback = action;
        }

        /// <summary>
        /// 窗体上将要显示的内容
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// 窗体显示的时间,将在指定时间内关闭, 单位:秒
        /// </summary>
        public int Timeout { get; set; } = int.MaxValue;

        /// <summary>
        /// 窗体关闭时的回调
        /// </summary>
        public Action<bool> DialogCallback { get; set; } = null;

        /// <summary>
        /// 是否顯示提示層
        /// </summary>
        public bool IsMaskedLayerDisplay { get; set; } = true;

        /// <summary>
        /// 打印完成是否需要立即检查label
        /// </summary>
        public PrintCheckType RiseCheckLabel { get; set; } = PrintCheckType.None;

        /// <summary>
        /// 打印完成需要立即检查的标签数据
        /// </summary>
        public Dictionary<string, string> LabelCheckDatas { get; set; }

        /// <summary>
        /// 未配置的标签默认是否检测
        /// </summary>
        public bool DefaultCheck { get; set; } = true;

        /// <summary>
        /// 是否需要检查
        /// </summary>
        public bool IsCheck { get; set; } = true;
    }
}
