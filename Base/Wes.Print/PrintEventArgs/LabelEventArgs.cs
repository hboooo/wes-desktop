using System;
using System.Collections.Generic;
using Wes.Print.Model;

namespace Wes.Print
{
    public class LabelEventArgs : EventArgs
    {
        public LabelEventArgs(Dictionary<string, string> labelValues)
        {
            this.LabelValues = labelValues;
        }

        public LabelEventArgs(Dictionary<string, string> labelValues, PrintCheckType printCheckType) : this(labelValues)
        {
            this.PrintCheckType = printCheckType;
        }

        public Dictionary<string, string> LabelValues { get; set; }

        /// <summary>
        /// 是否为主动触发
        /// </summary>
        public PrintCheckType PrintCheckType { get; set; }
    }
}
