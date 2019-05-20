using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Wes.Wrapper
{
    public class RestUrl
    {

#if DEBUG
        public static readonly String WmsServerUrl = ConfigurationManager.AppSettings["WMS_DEBUG_REST_SERVER_URL"];
#else
        public static readonly String WmsServerUrl = ConfigurationManager.AppSettings["WMS_REST_SERVER_URL"];
#endif
        public static readonly String WesServerUrl = ConfigurationManager.AppSettings["WES_SERVER_URL"];
        public static readonly String SidServerUrl = ConfigurationManager.AppSettings["SID_SERVER"];

        private String _url;

        public static RestUrl NewInstance()
        {
            return new RestUrl();
        }

        public RestUrl In(RestUrlType shut)
        {
            switch (shut)
            {
                case RestUrlType.WmsServer:
                    _url = WmsServerUrl;
                    break;
                case RestUrlType.WesServer:
                    _url = WesServerUrl;
                    break;
                case RestUrlType.SidServer:
                    _url = SidServerUrl;
                    break;
            }

            return this;
        }

        public RestUrl In(String url)
        {
            this._url = url;
            return this;
        }

        public override String ToString()
        {
            return _url;
        }
    }

    public enum RestUrlType
    {
        WmsServer,
        WesServer,
        SidServer,
    }
}