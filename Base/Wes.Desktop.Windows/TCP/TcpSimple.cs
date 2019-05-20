using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Wes.Utilities;

namespace Wes.Desktop.Windows.TCP
{
    public class TcpSimple
    {
        public static readonly string ALARM_OPEN = "open";
        public static readonly string ALARM_CLOSE = "close";

        public static void SendAlarmDevice(string ip, int port, string message)
        {
            if (string.IsNullOrEmpty(ip))
            {
                LoggingService.Error("ip is null");
            }
            if (port <= 0 || port > 65535)
            {
                LoggingService.Error("port error");
            }
            if (string.IsNullOrEmpty(message))
            {
                LoggingService.Error("message is null");
            }

            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            try
            {
                LoggingService.Info($"send message:[{message}] to {ip}");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(ipEndPoint);
                byte[] sendData = Encoding.ASCII.GetBytes(message);
                client.Send(sendData);
                LoggingService.Info($"send message:[{message}] to {ip} completed");
                client.Close();
            }
            catch (Exception ex)
            {
                LoggingService.Error($"socket error:{ip},{port}");
                LoggingService.Error(ex);
            }
        }
    }
}
