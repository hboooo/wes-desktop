using System.Collections.Generic;

namespace Wes.Flow
{
    /// <summary>
    /// 扫描命令
    /// </summary>
    public struct WesScanCommand
    {
        /// <summary>
        /// 无
        /// </summary>
        public const string NONE = "NONE";

        /// <summary>
        /// 箱结束
        /// </summary>
        public const string CARTON_END = "CARTONEND";

        /// <summary>
        /// 删除TXT下所有组板/箱
        /// </summary>
        public const string DELETE_CARTON = "DELETEC";

        /// <summary>
        /// 删除指定板
        /// </summary>
        public const string DELETE_PALLET = "DELETEP";

        /// <summary>
        /// 板结束
        /// </summary>
        public const string PALLET_END = "PALLETEND";

        /// <summary>
        /// 盘结束
        /// </summary>
        public const string BOX_END = "BOXEND";

        /// <summary>
        /// 盘结束
        /// </summary>
        public const string NO_BOX = "NOBOX";

        /// <summary>
        /// 整箱盘结束
        /// </summary>
        public const string FILL_BOX = "FILLBOX";

        /// <summary>
        /// 收貨確認
        /// </summary>
        public const string RCV_END = "RCVEND";

        /// <summary>
        /// Print
        /// </summary>
        public const string RE_PRINT = "REPRINT";

        /// <summary>
        /// ReelEnd
        /// </summary>
        public const string FLOW_END = "FLOWEND";

        /// <summary>
        /// ReelStart
        /// </summary>
        public const string FLOW_START = "FLOWSTART";

        private static Dictionary<string, string> _commands = new Dictionary<string, string>();

        public static string GetCommandName(string scan)
        {
            if (string.IsNullOrWhiteSpace(scan)) return null;
            if (_commands.Count == 0)
            {
                var fieldInfos = typeof(WesScanCommand).GetFields();
                foreach (var item in fieldInfos)
                {
                    _commands[item.Name] = item.GetValue(null).ToString();
                }
            }
            string scanCommand = scan.ToUpper();
            foreach (var item in _commands)
            {
                if (string.Compare(item.Value, scanCommand) == 0)
                {
                    return item.Key;
                }
            }
            return null;
        }
    }
}