using System;

namespace Wes.Core
{
    /// <summary>
    /// 第一次扫描事件参数
    /// </summary>
    public class ActionFlowEventArgs : EventArgs
    {
        public ActionFlowEventArgs(string scanValue)
        {
            this.ScanValue = scanValue;
        }

        public ActionFlowEventArgs(Exception ex)
        {
            this.Ex = ex;
            Type = ActionNotityType.ExceptionAction;
        }

        public string ScanValue { get; set; }

        public bool Handled { get; set; } = false;

        public Exception Ex { get; set; }

        public ActionNotityType Type { get; set; } = ActionNotityType.FirstAction;
    }


    public enum ActionNotityType
    {
        FirstAction = 0x01,
        ExceptionAction = 0xf1,
    }
}
