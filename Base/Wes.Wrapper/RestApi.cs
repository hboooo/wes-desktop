using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Exception;

namespace Wes.Wrapper
{
    public class RestApi
    {
        private RestApi(Method method)
        {
            _restClient = new RestClient();

            _request = new RestRequest((RestSharp.Method) method);
            _request.JsonSerializer = new JsonSerializer();
            _request.AddHeader("Content-Type", "application/json");
            string userCode = WesDesktop.Instance.User != null ? WesDesktop.Instance.User.Code : "";
            string deviceVersion = WesDesktop.Instance.User != null ? WesDesktop.Instance.Version : "";
            string endCustomer = WesDesktop.Instance.AddIn != null ? WesDesktop.Instance.AddIn.EndCustomer : "";
            string endCustomerName = WesDesktop.Instance.AddIn != null ? WesDesktop.Instance.AddIn.EndCustomerName : "";
            string uid = WesDesktop.Instance.User != null ? WesDesktop.Instance.User.Uid : "";
            string and = Guid.NewGuid().ToString().Substring(0, 8) + "----";
            string equal = Guid.NewGuid().ToString().Substring(0, 8) + "---";
            _request.AddHeader("Separate", $"{and}={equal}");
            _request.AddHeader("User-Info",
                $"name{equal}{userCode}{and}endCustomer{equal}{endCustomer}{and}endCustomerName{equal}{endCustomerName}{and}version{equal}{deviceVersion}{and}userId{equal}{uid}");
        }

        private String _url;
        private IRestClient _restClient;
        private IRestRequest _request;
        private IRestResponse _response;
        private Dictionary<string, object> _bodyDictionary;
        private Dictionary<string, object> _paramDictionary;

        public static RestApi NewInstance(Method method)
        {
            return new RestApi(method);
        }

        public static RestApi Of(Method method)
        {
            return NewInstance(method);
        }

        public RestApi SetUrl(RestUrlType rut, string path)
        {
            path = path.Trim('/');
            this._url = RestUrl.NewInstance().In(rut) + path;
            _restClient.BaseUrl = new Uri(this._url);
            return this;
        }

        public RestApi SetUrl(RestUrlType rut)
        {
            this._url = RestUrl.NewInstance().In(rut).ToString();
            _restClient.BaseUrl = new Uri(this._url);
            return this;
        }

        public RestApi SetUrl(string url)
        {
            this._url = url;
            _restClient.BaseUrl = new Uri(this._url);
            return this;
        }

        public RestApi SetUrl()
        {
            return SetUrl(RestUrlType.WmsServer, "1/{si}");
        }

        public RestApi SetNotTranUrl()
        {
            return SetUrl(RestUrlType.WmsServer, "0/{si}");
        }

        public RestApi SetMethod()
        {
            return this;
        }

        public RestApi AddHeader(string contentType, string value)
        {
            _request.AddHeader(contentType, value);
            return this;
        }

        public RestApi AddUrlSegment(string key, string value)
        {
            _request.AddUrlSegment(key, value);
            return this;
        }

        public RestApi AddBranch(string value)
        {
            _request.AddQueryParameter("branch", value);
            return this;
        }

        public RestApi AddScriptId(string value)
        {
            _request.AddUrlSegment("si", value);
            return this;
        }

        public RestApi AddScriptId(long value)
        {
            _request.AddUrlSegment("si", value.ToString());
            return this;
        }

        public RestApi AddUrlSegment(string key, long value)
        {
            _request.AddUrlSegment(key, value.ToString());
            return this;
        }

        public RestApi AddScriptId(ScriptID value)
        {
            _request.AddUrlSegment("si", ((long) value).ToString());
            return this;
        }

        public RestApi AddUri(string router)
        {
            return AddUri(RestUrlType.WmsServer, router);
        }

        public RestApi AddFlowUri(string router)
        {
            return AddFlowUri(RestUrlType.WmsServer, router);
        }

        public RestApi AddUri(RestUrlType rut, string router)
        {
            router = router.TrimStart('/');
            return SetUrl(rut, $"{WesDesktop.Instance.AddInName.ToLower()}/{router}");
        }

        public RestApi SetWmsGlobalUri(string router)
        {
            router = router.TrimStart('/');
            return SetUrl(RestUrlType.WmsServer, router);
        }

        public RestApi SetWmsCustomerUri(string router)
        {
            router = router.TrimStart('/');
            return AddUri(RestUrlType.WmsServer, router);
        }

        public RestApi AddFlowUri(RestUrlType rut, string router)
        {
            router = router.TrimStart('/');
            return SetUrl(rut, $"flow/{router}");
        }

        public RestApi AddCommonUri(RestUrlType rut, string router)
        {
            router = router.TrimStart('/');
            return SetUrl(rut, router);
        }

        public RestApi AddUriParam(RestUrlType rut, string scriptId, bool? useTrans = null)
        {
            if (useTrans == null)
            {
                SetUrl(rut, "{si}");
            }
            else
            {
                if ((bool) useTrans)
                    SetUrl(rut, "1/{si}");
                else
                    SetUrl(rut, "0/{si}");
            }

            return AddScriptId(scriptId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rut"></param>
        /// <param name="scriptId"></param>
        /// <param name="useTrans">null:默认值，无事务，解析服务使用；false:不使用事务，wms服务使用；true:使用事务，wms服务使用</param>
        /// <returns></returns>
        public RestApi AddUriParam(RestUrlType rut, long scriptId, bool? useTrans = null)
        {
            return AddUriParam(rut, scriptId.ToString(), useTrans);
        }

        public RestApi AddUriParam(RestUrlType rut, ScriptID scriptId, bool? useTrans = null)
        {
            return AddUriParam(rut, ((long) scriptId).ToString(), useTrans);
        }

        public RestApi AddQueryParameter(string key, object value)
        {
            if (_paramDictionary == null)
            {
                _paramDictionary = new Dictionary<string, object>();
            }

            if (_paramDictionary.ContainsKey(key))
            {
                _paramDictionary.Remove(key);
            }

            _paramDictionary.Add(key, value?.ToString());
            return this;
        }

        public RestApi AddQueryParameter(object jsonObj)
        {
            String value = DynamicJson.SerializeObject(jsonObj);
            var dictionary = DynamicJson.DeserializeObject<Dictionary<string, object>>(value);
            if (_paramDictionary != null)
            {
                foreach (var key in dictionary.Keys)
                {
                    this._paramDictionary.Add(key, dictionary[key]);
                }
            }
            else
            {
                this._paramDictionary = dictionary;
            }

            return this;
        }

        public RestApi AddParams(object jsonObj)
        {
            if (this._request.Method == RestSharp.Method.GET)
            {
                this.AddQueryParameter(jsonObj);
            }
            else
            {
                this.AddJsonBody(jsonObj);
            }

            return this;
        }

        public RestApi AddParams(string key, object value)
        {
            if (this._request.Method == RestSharp.Method.GET)
            {
                this.AddQueryParameter(key, value);
            }
            else
            {
                this.AddJsonBody(key, value);
            }

            return this;
        }

        public RestApi AddJsonBody(object jsonObj)
        {
            String value = DynamicJson.SerializeObject(jsonObj);
            var dictionary = DynamicJson.DeserializeObject<Dictionary<string, object>>(value);
            if (_bodyDictionary != null)
            {
                foreach (var key in dictionary.Keys)
                {
                    this._bodyDictionary.Add(key, dictionary[key]);
                }
            }
            else
            {
                this._bodyDictionary = dictionary;
            }

            return this;
        }

        public RestApi AddJsonBody(string key, object value)
        {
            if (_bodyDictionary == null)
            {
                _bodyDictionary = new Dictionary<string, object>();
            }

            if (_bodyDictionary.ContainsKey(key))
            {
                _bodyDictionary.Remove(key);
            }

            _bodyDictionary.Add(key, value);
            return this;
        }

        public RestApi ExecuteAsync(Action<bool, Exception, RestApi> callback)
        {
            PrepareRequest();
            var obj = _restClient.ExecuteAsync(this._request, (response) =>
            {
                this._response = response;
                Exception ex = PrepareAsyncResponse();
                callback?.Invoke(ex == null ? true : false, ex, ex == null ? this : null);
            });
            return this;
        }

        public RestApi Execute()
        {
            PrepareRequest();
            using (DebugTimer.InfoTime("WMS RestApi"))
            {
                Func<IRestRequest, IRestResponse> action = new Func<IRestRequest, IRestResponse>(_restClient.Execute);
                IAsyncResult result = action.BeginInvoke(_request, null, null);
                LoggingService.Debug("Send data completed.");
                this._response = action.EndInvoke(result);
            }

            Exception ex = PrepareAsyncResponse();
            if (ex != null) throw ex;
            return this;
        }

        public RestApi ExecuteNotJson()
        {
            this._response = _restClient.Execute(this._request);
            return this;
        }

        private void PrepareRequest()
        {
            if (_bodyDictionary != null)
            {
                _bodyDictionary["_endCustomer"] =
                    WesDesktop.Instance.AddIn == null ? "" : WesDesktop.Instance.AddIn.EndCustomer;
                BuildJsonBody();
                _request.AddJsonBody(_bodyDictionary);
            }
            else
            {
                Dictionary<string, string> dicParams = new Dictionary<string, string>();
                dicParams["_endCustomer"] =
                    WesDesktop.Instance.AddIn == null ? "" : WesDesktop.Instance.AddIn.EndCustomer;
                _request.AddJsonBody(dicParams);
            }

            if (_paramDictionary != null)
            {
                _paramDictionary["_endCustomer"] =
                    WesDesktop.Instance.AddIn == null ? "" : WesDesktop.Instance.AddIn.EndCustomer;
                foreach (var key in _paramDictionary.Keys)
                {
                    _request.AddQueryParameter(key,
                        _paramDictionary[key] == null ? null : _paramDictionary[key].ToString());
                }
            }

            LoggingService.DebugFormat("Query   data:{0}",
                _paramDictionary != null ? DynamicJson.SerializeObject(_paramDictionary) : "null");
            LoggingService.DebugFormat("Body    data:{0}",
                _bodyDictionary != null ? DynamicJson.SerializeObject(_bodyDictionary) : "null");
            LoggingService.DebugFormat("RestApi request:{0}", _restClient.BuildUri(_request).OriginalString);
        }

        /// <summary>
        /// 替換jsonBody中的空值
        /// </summary>
        private void BuildJsonBody()
        {
            HashSet<string> keys = new HashSet<string>();
            foreach (var item in _bodyDictionary)
            {
                if (item.Value != null && item.Value.GetType().FullName == "MS.Internal.NamedObject")
                {
                    keys.Add(item.Key);
                }
            }

            foreach (var item in keys)
            {
                _bodyDictionary[item] = null;
            }
        }

        private Exception PrepareAsyncResponse()
        {
            LoggingService.DebugFormat("RestApi response:{0}", _response.Content);
            Exception ex = null;
            if (this._response.StatusCode != HttpStatusCode.OK)
            {
                var respStatus = 0;
                dynamic respContent = default(dynamic);
                try
                {
                    respStatus = (int) this._response.StatusCode;
                    respContent = DynamicJson.DeserializeObject<object>(_response.Content);
                }
                catch
                {
                    string guid = Guid.NewGuid().ToString();
                    LoggingService.Error($"錯誤id:{guid}" + _response.Content);
                    ex = new WesRestException("服務器異常，請稍後重試，詳細:" + ex.Message + $"。錯誤id:{guid}");
                    return ex;
                }

                switch (respStatus)
                {
                    case 0:
                        ex = new WesRestException("无法连接到目标服务器, 状态码: 0");
                        break;
                    case 404:
                        ex = new WesRestException("未找到接口资源, 状态码: 404");
                        break;
                    case 500:
                    case 555:
                        string message;
                        string messageId;
                        string messageCode;
                        if (!string.IsNullOrEmpty(this._response.Content))
                        {
                            message = respContent.error;
                            messageId = respContent.id;
                            messageCode = respContent.code;
                        }
                        else
                        {
                            message = "未添加提示信息, 请联系IT人员添加此提示信息";
                            messageId = "0";
                            messageCode = "0";
                        }

                        message = message.TrimEnd('.').TrimEnd(',').TrimEnd('。').TrimEnd('，');
                        var wesRestException =
                            new WesRestException($"{message}\n\n状态码: {respStatus}\n消息码: {messageId}");
                        wesRestException.MessageId = long.Parse(messageId);
                        wesRestException.MessageCode = long.Parse(messageCode);
                        wesRestException.RespStatus = respStatus;

                        ex = wesRestException;
                        break;
                    case 502:
                        ex = new WesRestException("服务正在更新，请稍等片刻再试。状态码: 502");
                        break;
                    default:
                        ex = new WesRestException($"呦吼，系統發生錯誤,火速按下PrintScreen提交錯誤信息。 \r\n状态码: {respStatus}");
                        break;
                }
            }

            return ex;
        }

        public RestApi OutRef(out IRestClient rc, out IRestResponse resp, out IRestRequest res)
        {
            rc = this._restClient;
            resp = this._response;
            res = this._request;
            return this;
        }

        public RestApi OutRef(out IRestResponse resp, out IRestRequest res)
        {
            resp = this._response;
            res = this._request;
            return this;
        }

        public RestApi OutRef(out IRestResponse resp)
        {
            resp = this._response;
            return this;
        }

        public RestApi OutRef(out IRestRequest res)
        {
            res = this._request;
            return this;
        }

        public long RespStatus()
        {
            return (long) this._response.StatusCode;
        }

        public int ToInt()
        {
            return (int) ExecFuncForReturnStruct<int>();
        }

        public long ToLong()
        {
            return (long) ExecFuncForReturnStruct<long>();
        }

        public float ToFloat()
        {
            return (float) ExecFuncForReturnStruct<float>();
        }

        public double ToFloat(Func<IRestResponse, double> func)
        {
            return (double) ExecFuncForReturnStruct<double>();
        }

        public Boolean ToBoolean()
        {
            return (Boolean) ExecFuncForReturnStruct<Boolean>();
        }

        public T To<T>() where T : class
        {
            return (T) ExecFuncForReturnClass<T>();
        }

        public T ToStruct<T>() where T : struct
        {
            return DynamicJson.DeserializeObject<T>(_response.Content);
        }

        private object ExecFuncForReturnStruct<T>() where T : struct
        {
            return DynamicJson.DeserializeObject<T>(_response.Content);
        }

        private object ExecFuncForReturnClass<T>() where T : class
        {
            if (typeof(T) == typeof(string))
            {
                return _response.Content;
            }

            return DynamicJson.DeserializeObject<T>(_response.Content);
        }
    }
}