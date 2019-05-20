using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Wes.Utilities;

namespace Wes.Server.Listener
{
    public class ControllerService
    {
        private static Dictionary<string, Lazy<IKernelRequest, IHttpRequestMetadata>> _requestActions = new Dictionary<string, Lazy<IKernelRequest, IHttpRequestMetadata>>();

        private static Lazy<IKernelRequest, IHttpRequestMetadata> GetRequests(string module)
        {
            Lazy<IKernelRequest, IHttpRequestMetadata> action = null;
            if (_requestActions.ContainsKey(module))
            {
                action = _requestActions[module];
            }
            else
            {
                try
                {
                    action = HttpRequestComposition.ExportComposition().GetExport<IKernelRequest, IHttpRequestMetadata>(module);
                    if (action != null)
                        _requestActions[module] = action;
                }
                catch (Exception ex)
                {
                    LoggingService.Error(ex);
                }
            }

            return action;
        }

        public static void Execute(RequestParams requestParams)
        {
            try
            {
                Lazy<IKernelRequest, IHttpRequestMetadata> requestAction = GetRequests(requestParams.Module);
                if (requestAction == null || requestAction.Value == null)
                {
                    string message = $"{requestParams.Module}接口未实现,id:{requestParams.HashCode}";
                    _requestEvent?.Invoke(requestParams, new RequestEventArgs(requestParams, message));
                    return;
                }

                requestAction.Value.Params = requestParams;
                MethodInfo method = requestAction.Value.GetType().GetMethods().Where(m =>
                {
                    RequestUrlAttribute attr = GetRequestAttribute(m);
                    if (attr != null && attr.Url == requestParams.RequestUri)
                    {
                        requestAction.Value.Params.Attribute = attr;
                        return true;
                    }
                    return false;
                }).FirstOrDefault();
                if (method == null)
                {
                    _requestEvent?.Invoke(requestParams, new RequestEventArgs(requestParams, $"{requestParams.RequestTime.ToString("HH:mm:ss")}收到的为无效请求{requestParams.RawUrl}，已忽略,id:{requestParams.HashCode}"));
                    return;
                }
                _requestEvent?.Invoke(requestParams, new RequestEventArgs(requestParams, $"正在处理{requestParams.RequestTime.ToString("HH:mm:ss")}请求{requestParams.RawUrl}...", true));

                try
                {
                    requestParams.IsHandling = true;
                    LoggingService.Debug($"Query params:{requestParams.PathAndQuery},id:{requestParams.HashCode}");
                    method.Invoke(requestAction.Value, new object[] { requestParams.QueryParameters });
                    requestParams.IsHandling = false;
                }
                catch (Exception ex)
                {
                    _requestEvent?.Invoke(requestParams, new RequestEventArgs(requestParams, $"服务处理请求错误，请联系管理员,id:{requestParams.HashCode}"));
                    LoggingService.Error(ex);
                }
            }
            catch (Exception ex)
            {
                _requestEvent?.Invoke(requestParams, new RequestEventArgs(requestParams, ex.Message + $"id:{requestParams.HashCode}"));
                LoggingService.Error(ex);
            }
        }

        private static RequestUrlAttribute GetRequestAttribute(MethodInfo method)
        {
            RequestUrlAttribute attribute = (RequestUrlAttribute)method.GetCustomAttributes(typeof(RequestUrlAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                return attribute;
            }
            return null;
        }

        private static event RequestEventHandler _requestEvent;

        public static event RequestEventHandler RequestEvent
        {
            add { _requestEvent += value; }
            remove { _requestEvent -= value; }
        }

    }
}
