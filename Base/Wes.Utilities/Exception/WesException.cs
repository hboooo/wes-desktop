using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Wes.Utilities.Exception
{
    public class WesException : global::System.Exception
    {
        public WesException()
        {
            AddSystemInfo();
        }

        public WesException(string message) : base(message)
        {
            AddSystemInfo();
        }

        public WesException(string message, dynamic data) : base(message)
        {
            SetData(data);
        }

        public WesException(global::System.Exception innerException) : base("非预期异常", innerException)
        {
            AddSystemInfo();
        }

        public WesException(string message, dynamic data, global::System.Exception innerException) : base(message, innerException)
        {
            SetData(data);
        }

        public WesException(dynamic data, global::System.Exception innerException) : base("非预期异常", innerException)
        {
            SetData(data);
        }

        public WesException(string message, global::System.Exception innerException) : base(message, innerException)
        {
            AddSystemInfo();
        }

        protected WesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            AddSystemInfo();
        }

        public static WesException Error(global::System.Exception exception)
        {
            return new WesException(exception);
        }

        public void AddCustomerInfo(string key, string value)
        {
            if (_data == null || _data.Count == 0)
            {
                _data = new Dictionary<string, string>();
            }
            if (!string.IsNullOrEmpty(value))
            {
                _data[key] = value;
            }
        }

        protected void SetData(dynamic data)
        {
            try
            {
                AddSystemInfo();
                if (data != null)
                {
                    String value = DynamicJson.SerializeObject(data);
                    Dictionary<string, object> tempDic = DynamicJson.DeserializeObject<Dictionary<string, object>>(value);
                    foreach (var item in tempDic)
                    {
                        AddCustomerInfo(item.Key, DynamicJson.SerializeObject(item.Value));
                    }
                }
            }
            catch { }
        }

        protected void AddSystemInfo()
        {
            if (!string.IsNullOrEmpty(WesApp.SystemInfomation))
            {
                int index = 10;
                Dictionary<string, object> tempDic = DynamicJson.DeserializeObject<Dictionary<string, object>>(WesApp.SystemInfomation);
                if (tempDic != null)
                {
                    foreach (var item in tempDic)
                    {
                        if (item.Value != null)
                        {
                            AddCustomerInfo($"0{index}_{item.Key}", item.Value.ToString());
                            index++;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(WesApp.CurrentWorkNo))
            {
                AddCustomerInfo($"100_wrokNo", WesApp.CurrentWorkNo);
            }
        }

        public long MessageId { get; set; }
        public long MessageCode { get; set; }
        public long RespStatus { get; set; }

        protected IDictionary _data = null;

        public override IDictionary Data => _data;
    }
}