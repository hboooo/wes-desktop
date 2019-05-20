using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Xml;

namespace Wes.Server.Listener
{
    public class KernelHttpListener
    {
        private static ConcurrentDictionary<int, RequestParams> _requests = new ConcurrentDictionary<int, RequestParams>();

        private static HttpListener _httpListener = null;

        private static Timer _timer = null;

        public static bool Listen(string port = null)
        {
            try
            {
                if (!HttpListener.IsSupported)
                    throw new WesException("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");

                string[] prefixes = GetPrefixes(port);
                if (prefixes == null || prefixes.Length == 0)
                    throw new WesException("There are no prefixes.");

                _httpListener = new HttpListener();
                foreach (string prefix in prefixes)
                {
                    _httpListener.Prefixes.Add(prefix);
                }
                InitTimer();
                _httpListener.Start();
                LoggingService.Info("Listening...");
                BeginGetContext();
                return true;
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return false;
        }

        /// <summary>
        /// 以管理员身份运行cmd
        /// 1.删除错误urlacl                           netsh http delete urlacl url=http://*:12339/
        /// 2.将删除的*号地址重新加进url，user选择所有人    netsh http add urlacl url=http://*:12339/  user=Everyone
        /// 3.配置防火墙                                netsh advfirewall firewall Add rule name="http://*:12339/" dir=in protocol=tcp localport=12339 action=allow
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private static string[] GetPrefixes(string port)
        {
            if (port == null)
                port = EnvironmetService.GetValue("General", "Port");
            if (String.IsNullOrEmpty(port)) port = "12339";
            string prefixe = $"http://*:{port}/";
            return new string[] { prefixe };
        }

        #region Listener Release

        public static void Close()
        {
            try
            {
                _requests.Clear();
                CloseTimer();
                if (_httpListener != null)
                {
                    _httpListener.Close();
                    _httpListener = null;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        public static void Abort()
        {
            try
            {
                _requests.Clear();
                CloseTimer();
                if (_httpListener != null)
                {
                    _httpListener.Abort();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        #endregion

        private static void BeginGetContext()
        {
            try
            {
                if (_httpListener != null && _httpListener.IsListening)
                {
                    IAsyncResult asyncResult = _httpListener.BeginGetContext(new AsyncCallback(ListenCallback), _httpListener);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        private static void ListenCallback(IAsyncResult result)
        {
            dynamic returnValue = null;
            try
            {
                HttpListener listener = (HttpListener)result.AsyncState;
                if (_httpListener == null || !_httpListener.IsListening) return;
                HttpListenerContext httpListenerContext = listener.EndGetContext(result);
                HttpListenerRequest request = httpListenerContext.Request;

                try
                {
                    RequestParams requestParams = new RequestParams();
                    requestParams.RawUrl = request.Url.AbsolutePath;
                    requestParams.PathAndQuery = request.Url.PathAndQuery;
                    requestParams.HashCode = request.GetHashCode();
                    requestParams.Parameters = request.QueryString;
                    requestParams.Method = request.HttpMethod;
                    requestParams.RequestTime = DateTime.Now;

                    if (requestParams.ErrorCode == RequestCode.IgnoreRequest)
                    {
                        return;
                    }

                    LoggingService.Info($"{DateTime.Now.ToString("HH:mm:ss")}收到请求:{request.RawUrl},Id:{requestParams.HashCode}");
                    if (string.IsNullOrEmpty(requestParams.Module))
                    {
                        string msg = $"忽略无效请求{requestParams.RawUrl}请求.";
                        PopupService.ShowBalloonTip(msg);
                        returnValue = new
                        {
                            result = false,
                            message = msg
                        };
                    }
                    else if (requestParams.ErrorCode == 0)
                    {
                        string msg = $"已添加请求{requestParams.RawUrl}至任务列表,id:{requestParams.HashCode}";
                        PopupService.ShowBalloonTip(msg);
                        _requests.TryAdd(request.GetHashCode(), requestParams);
                        _requestEvent?.Invoke(requestParams, new RequestEventArgs(requestParams, RequestActionType.Add));
                        returnValue = new
                        {
                            result = true,
                            message = msg
                        };
                    }
                }
                catch (Exception ex)
                {
                    returnValue = new
                    {
                        result = false,
                        message = ex.Message
                    };
                    LoggingService.Error(ex);
                }
                finally
                {
                    ReturnBody(httpListenerContext, returnValue);
                    BeginGetContext();
                }

            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        public static void ReturnBody(HttpListenerContext httpListenerContext, dynamic returnValue = null)
        {
            try
            {
                HttpListenerResponse response = httpListenerContext.Response;
                string res = null;
                if (returnValue != null)
                {
                    try
                    {
                        res = DynamicJson.SerializeObject(new List<object>() { returnValue });
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error(ex);
                    }
                }

                if (string.IsNullOrEmpty(res))
                {
                    var val = new
                    {
                        result = true,
                        message = "请求已完成"
                    };
                    res = DynamicJson.SerializeObject(new List<object>() { val });
                }
                byte[] reply = Encoding.UTF8.GetBytes(res);
                response.ContentLength64 = reply.Length;
                response.OutputStream.Write(reply, 0, reply.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        private static void InitTimer()
        {
            try
            {
                CloseTimer();
                _timer = new Timer();
                _timer.Enabled = true;
                _timer.Interval = 10;
                _timer.Elapsed += _timer_Elapsed;
                _timer.Start();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timer.Stop();
                if (_requests.Count > 0)
                {
                    var req = _requests.First();
                    ControllerService.Execute(req.Value);
                    _requests.TryRemove(req.Key, out RequestParams tempValue);
                    _requestEvent?.Invoke(tempValue, new RequestEventArgs(tempValue, RequestActionType.Delete));
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            finally
            {
                _timer.Start();
            }
        }

        private static void CloseTimer()
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                    _timer = null;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        private static event RequestEventHandler _requestEvent;

        public static event RequestEventHandler RequestEvent
        {
            add { _requestEvent += value; }
            remove { _requestEvent -= value; }
        }
    }
}
