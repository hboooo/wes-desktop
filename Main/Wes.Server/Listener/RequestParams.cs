using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Wes.Server.Listener
{
    public class RequestParams : NotifyPropertyChanged
    {
        private string _rawUrl = string.Empty;
        public string RawUrl
        {
            get { return _rawUrl; }
            set
            {
                _rawUrl = value;
                BuildUri(_rawUrl);
            }
        }

        public string PathAndQuery { get; set; }

        private NameValueCollection _parameters;
        public NameValueCollection Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                BuildParams(_parameters);
            }
        }

        public int HashCode { get; set; }

        public DateTime RequestTime { get; set; }

        public string Method { get; set; }

        public string Module { get; private set; }

        public string RequestUri { get; private set; }

        public Dictionary<string, string> QueryParameters { get; private set; }

        public RequestCode ErrorCode { get; set; }

        private RequestUrlAttribute _attribute;
        public RequestUrlAttribute Attribute {
            get { return _attribute; }
            set
            {
                if (_attribute != value)
                {
                    _attribute = value;
                    OnPropertyChanged(nameof(RequestDesc));
                }
            }
        }

        private bool _isHandling = false;
        public bool IsHandling
        {
            get { return _isHandling; }
            set
            {
                if (_isHandling != value)
                {
                    _isHandling = value;
                    OnPropertyChanged(nameof(IsHandling));
                    OnPropertyChanged(nameof(HandlingMessage));
                }
            }
        }
        public string RequestDesc
        {
            get
            {
                if (Attribute != null)
                {
                    return Attribute.Description;
                }
                return null;
            }
        }
        public string RequestTimeString
        {
            get
            {
                return RequestTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        public string HandlingMessage
        {
            get
            {
                if (IsHandling == true)
                {
                    return "正在处理";
                }
                else
                {
                    return "等待中";
                }
            }
        }

        private void BuildUri(string url)
        {
            if (string.IsNullOrEmpty(url)
                || string.Compare(url, "/favicon.ico", true) == 0
                || string.Compare(url, "/") == 0)
            {
                ErrorCode = RequestCode.IgnoreRequest;
                return;
            }

            string[] urls = url.Replace("//", "/").TrimStart('/').Split('/');
            if (urls == null || urls.Length != 2)
            {
                ErrorCode = RequestCode.NotImplemented;
                return;
            }
            Module = urls[0];
            RequestUri = urls[1];
        }

        private void BuildParams(NameValueCollection nameValues)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            if (_parameters != null)
            {
                foreach (string key in _parameters.AllKeys)
                {
                    values[key] = _parameters[key];
                }
            }
            QueryParameters = values;
        }
    }

    public enum RequestCode
    {
        IgnoreRequest = 100,
        NotImplemented = 101,
    }
}
