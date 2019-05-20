using Seagull.BarTender.Print;
using System;
using System.Collections.Generic;
using Wes.Utilities;
using Wes.Utilities.Exception;

namespace Wes.Print
{
    /// <summary>
    /// BarTender打印机设备工具类
    /// </summary>
    public class WesPrinter
    {
        /// <summary>
        /// 获取系统默认打印设备
        /// </summary>
        /// <returns></returns>
        public static Printer GetDefaultPrinter()
        {
            try
            {
                Printers printers = new Printers();
                return printers.Default;
            }
            catch (Exception ex)
            {
                LoggingService.Error(new WesException("獲取系統打印機列表失敗，請聯繫管理員", ex));
            }
            return null;
        }

        /// <summary>
        /// 获取系统所有打印机
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetPrinters()
        {
            Dictionary<string, string> printerDic = new Dictionary<string, string>();
            try
            {
                Printers printers = new Printers();
                foreach (var item in printers)
                {
                    printerDic[item.PrinterName] = item.PrinterName;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(new WesException("獲取系統打印機列表失敗，請聯繫管理員", ex));
            }
            return printerDic;
        }
    }
}
