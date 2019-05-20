using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using Wes.Utilities;
using Wes.Utilities.Xml;

namespace Wes.Desktop.Windows
{
    public class SerialPortProvider
    {
        #region 打印机防呆串口

        /// <summary>
        /// 通过打印机设备找到对应串口
        /// </summary>
        /// <param name="md5">打印机名称的md5值</param>
        /// <returns></returns>
        public static SerialPortItem GetSerialPortConfig(string md5)
        {
            string name = EnvironmetService.GetValue("SerialPort" + md5, "SerialItem");
            int baudRate = EnvironmetService.GetIntValue("SerialPort" + md5, "BaudRateItem");
            int dataBits = EnvironmetService.GetIntValue("SerialPort" + md5, "DataBitsItem");
            int parity = EnvironmetService.GetIntValue("SerialPort" + md5, "ParityItem");
            int stopBits = EnvironmetService.GetIntValue("SerialPort" + md5, "StopBitsItem");
            return new SerialPortItem
            {
                Name = name,
                BaudRate = baudRate,
                DataBits = dataBits,
                Parity = parity,
                StopBits = stopBits,
            };
        }

        /// <summary>
        /// 檢測工站是否已經配置打印機與串口設置
        /// </summary>
        /// <returns></returns>
        public static bool IsSerialPortConfig()
        {
            var configs = EnvironmetService.GetElements("SerialPort");
            if (configs.Count > 0)
            {
                foreach (var item in configs)
                {
                    if (item.Value.ContainsKey("SerialItem"))
                    {
                        string comName = item.Value["SerialItem"] == null ? null : item.Value["SerialItem"].ToString();
                        if (!string.IsNullOrWhiteSpace(comName))
                        {
                            return true;
                        }
                    }
                }
            }
            LoggingService.Info("未配置打印机串口防呆设备");
            return false;
        }

        /// <summary>
        /// 獲取所有已配置的串口
        /// </summary>
        /// <returns></returns>
        public static HashSet<string> GetAllPortConfig()
        {
            int valueCount = 6;   //配置頁面串口配置項個數
            HashSet<string> coms = new HashSet<string>();
            Dictionary<string, Dictionary<string, object>> portConfigs = EnvironmetService.GetElements("SerialPort");
            foreach (var item in portConfigs)
            {
                if (item.Value.Count >= valueCount && item.Value.ContainsKey("SerialItem"))
                {
                    string comName = item.Value["SerialItem"] == null ? null : item.Value["SerialItem"].ToString();
                    if (!string.IsNullOrWhiteSpace(comName))
                    {
                        coms.Add(comName);
                    }
                }
            }
            return coms;
        }

        /// <summary>
        /// 獲取配置錯誤的窗口
        /// </summary>
        /// <returns></returns>
        public static HashSet<string> GetInvalidPortConfig()
        {
            HashSet<string> errorComs = new HashSet<string>();
            string[] ports = SerialPort.GetPortNames();
            List<string> existsPorts = ports != null ? ports.ToList<string>() : new List<string>();
            Dictionary<string, Dictionary<string, object>> portConfigs = EnvironmetService.GetElements("SerialPort");
            foreach (var item in portConfigs)
            {
                if (item.Value.ContainsKey("SerialItem"))
                {
                    string comName = item.Value["SerialItem"] == null ? null : item.Value["SerialItem"].ToString();
                    if (!string.IsNullOrWhiteSpace(comName))
                    {
                        if (!existsPorts.Contains(comName))
                        {
                            errorComs.Add(comName);
                        }
                    }
                }
            }

            if (errorComs.Count > 0)
            {
                //有錯誤配置時,刪除配置
                EnvironmetService.DeleteElement("SerialPort");
            }
            return errorComs;
        }

        #endregion
    }

    public class SerialPortItem
    {
        public string Name { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public int Parity { get; set; }
        public int StopBits { get; set; }
    }
}
