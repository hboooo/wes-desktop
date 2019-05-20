using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using Wes.Utilities;
using Wes.Utilities.Exception;

namespace Wes.Desktop.Windows.Printer
{
    public class SerialPortManager
    {
        /// 串口与打印机对应的md5值
        /// Key:打印机名称md5值 Value:SerialPort
        /// </summary>
        private static Dictionary<string, string> _portMd5Strs = new Dictionary<string, string>();
        /// <summary>
        /// 串口线程
        /// Key:SerialPort Value:打开串口的线程
        /// </summary>
        private static Dictionary<string, Task> _portThreads = new Dictionary<string, Task>();
        /// <summary>
        /// <summary>
        /// 串口接收数据回调
        /// Key:SerialPort
        /// </summary>
        private static Dictionary<string, Action<string>> _callbacks = new Dictionary<string, Action<string>>();
        /// <summary>
        /// 已打开的串口
        /// Key:SerialPort
        /// </summary>
        private static Dictionary<string, SerialPort> _serialPorts = new Dictionary<string, SerialPort>();

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="md5">打印机名称md5值</param>
        /// <param name="openCallback">执行打开串口回调</param>
        /// <param name="callback">收到串口设备发送的数据回调</param>
        public static void OpenSerialPort(string md5, Action<bool, string> openCallback, Action<string> callback)
        {
            if (string.IsNullOrWhiteSpace(md5))
            {
                openCallback(false, "");
                return;
            }

            SerialPortItem portConfig = SerialPortProvider.GetSerialPortConfig(md5);
            if (portConfig == null || string.IsNullOrEmpty(portConfig.Name))
            {
                LoggingService.InfoFormat("未配置com口，key:{0}", md5);
                openCallback(false, "");
                return;
            }

            if (_serialPorts.ContainsKey(portConfig.Name))
            {
                _portMd5Strs[md5] = portConfig.Name;
                AddComState(portConfig.Name, openCallback, callback);
                return;
            }

            var task = OpenSerialPort(md5, portConfig, openCallback, callback);
            if (task != null) _portThreads[portConfig.Name] = task;
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="md5">打印机名称md5值</param>
        /// <param name="openCallback">执行打开串口回调</param>
        /// <param name="callback">收到串口设备发送的数据回调</param>
        public static void OpenSerialPortByCom(string comName, Action<bool, string> openCallback, Action<string> callback)
        {
            if (string.IsNullOrWhiteSpace(comName))
                return;

            if (_serialPorts.ContainsKey(comName))
            {
                AddComState(comName, openCallback, callback);
            }
            else
            {
                SerialTaskOption taskOption = new SerialTaskOption();
                taskOption.PortItem = GetDefaultPortItem(comName);
                taskOption.ReceiveCallback = callback;
                taskOption.OpenCallback = openCallback;

                var task = Task.Factory.StartNew((option) =>
                {
                    SerialTaskOption taskArgs = option as SerialTaskOption;
                    if (taskArgs != null)
                    {
                        try
                        {
                            var serialPort = Open(taskArgs.PortItem);
                            if (serialPort != null)
                            {
                                if (taskArgs.ReceiveCallback != null)
                                    _callbacks[serialPort.PortName] = taskArgs.ReceiveCallback;
                                else
                                    _callbacks.Remove(serialPort.PortName);
                                taskArgs.OpenCallback?.Invoke(true, taskArgs.PortItem.Name);
                            }
                            else
                            {
                                taskArgs.OpenCallback?.Invoke(false, taskArgs.PortItem.Name);
                                CloseSerialPort(taskArgs.PortItem.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingService.Error(new WesException("串口异常：" + taskArgs.PortItem.Name, ex));
                            CloseSerialPort(taskArgs.PortItem.Name);
                        }
                    }
                }, taskOption);

                if (task != null) _portThreads[comName] = task;
            }
        }

        /// <summary>
        /// com默认参数配置
        /// </summary>
        /// <param name="comName"></param>
        /// <returns></returns>
        public static SerialPortItem GetDefaultPortItem(string comName)
        {
            SerialPortItem defaultItem = new SerialPortItem();
            defaultItem.Name = comName;   //设置默认参数
            defaultItem.BaudRate = 9600;
            defaultItem.DataBits = 8;
            defaultItem.Parity = 0;
            defaultItem.StopBits = 1;
            return defaultItem;
        }

        private static void AddComState(string comName, Action<bool, string> openCallback, Action<string> callback)
        {
            if (callback != null)
                _callbacks[comName] = callback;
            else
                _callbacks.Remove(comName);
            openCallback?.Invoke(true, comName);
        }

        private static Task OpenSerialPort(string md5, SerialPortItem portConfig, Action<bool, string> openCallback, Action<string> callback)
        {
            string portName = portConfig.Name;
            if (!Utils.IsContainsPort(portName))
            {
                LoggingService.DebugFormat("Port configuration does not exist,port:{0}", portName);
                openCallback?.Invoke(false, portConfig.Name);
                return null;
            }
            SerialTaskOption taskOption = new SerialTaskOption();
            taskOption.Md5OfPrinter = md5;
            taskOption.PortItem = portConfig;
            taskOption.ReceiveCallback = callback;
            taskOption.OpenCallback = openCallback;

            return Task.Factory.StartNew((option) =>
            {
                SerialTaskOption taskArgs = option as SerialTaskOption;
                if (taskArgs != null)
                {
                    try
                    {
                        var serialPort = Open(taskArgs.PortItem);
                        if (serialPort != null)
                        {
                            if (taskArgs.ReceiveCallback != null)
                                _callbacks[serialPort.PortName] = taskArgs.ReceiveCallback;
                            else
                                _callbacks.Remove(serialPort.PortName);
                            _portMd5Strs[taskArgs.Md5OfPrinter] = serialPort.PortName;
                            taskArgs.OpenCallback?.Invoke(true, taskArgs.PortItem.Name);
                        }
                        else
                        {
                            taskArgs.OpenCallback?.Invoke(false, taskArgs.PortItem.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error(new WesException("串口异常：" + taskArgs.PortItem.Name, ex));
                        CloseSerialPort(taskArgs.PortItem.Name);
                    }
                }
            }, taskOption);

        }

        private static SerialPort Open(SerialPortItem portConfig)
        {
            try
            {
                if (portConfig != null && !string.IsNullOrEmpty(portConfig.Name))
                {
                    LoggingService.InfoFormat("打开串口:{0}", portConfig.Name);
                    SerialPort serialPort = new SerialPort();
                    serialPort.PortName = portConfig.Name;
                    serialPort.BaudRate = portConfig.BaudRate;
                    serialPort.StopBits = (StopBits)portConfig.StopBits;
                    serialPort.Parity = (Parity)portConfig.Parity;
                    serialPort.DataBits = portConfig.DataBits;
                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.DataReceived += SerialPort_DataReceived;
                    serialPort.ErrorReceived -= SerialPort_ErrorReceived;
                    serialPort.ErrorReceived += SerialPort_ErrorReceived;
                    serialPort.Open();
                    _serialPorts[serialPort.PortName] = serialPort;
                    LoggingService.InfoFormat("打开串口:{0}成功", portConfig.Name);
                    return serialPort;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(new WesException("打开串口失败", ex));
            }
            return null;
        }

        private static void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            SerialPort serialPort = sender as SerialPort;
            if (serialPort == null) return;
            LoggingService.Debug("Receiving serial port error event,release serial port");
            CloseSerialPort(serialPort.PortName);
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = sender as SerialPort;
            if (serialPort == null) return;
            LoggingService.DebugFormat("{0} receiving data", serialPort.PortName);
            if (_callbacks.ContainsKey(serialPort.PortName))
            {
                _callbacks[serialPort.PortName](serialPort.PortName);
                ClearCallback(serialPort.PortName);
            }
        }

        private static void CloseSerialPort(string comName)
        {
            try
            {
                HashSet<string> md5Strs = new HashSet<string>();
                foreach (var item in _portMd5Strs)
                {
                    if (item.Value == comName)
                        md5Strs.Add(item.Key);
                }
                foreach (var item in md5Strs)
                {
                    _portMd5Strs.Remove(item);
                }

                try
                {
                    _callbacks.Remove(comName);
                    if (_serialPorts.ContainsKey(comName)) _serialPorts[comName].Dispose();
                    _serialPorts.Remove(comName);
                    if (_portThreads.ContainsKey(comName))
                    {
                        _portThreads.Remove(comName);
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Error(new WesException(string.Format("Release com:{0} error", comName), ex));
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(new WesException(string.Format("Task {0} close failed", comName), ex));
            }
        }

        public static void ClearCallback(string comName)
        {
            _callbacks.Remove(comName);
        }
    }

    public class SerialTaskOption
    {
        public string Md5OfPrinter { get; set; }

        public Action<string> ReceiveCallback { get; set; }

        public SerialPortItem PortItem { get; set; }

        public Action<bool, string> OpenCallback { get; set; }
    }
}
