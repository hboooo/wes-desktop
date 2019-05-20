using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wes.Core.Base;
using Wes.Wrapper;

namespace Wes.Core.Tests.Rest
{
    [TestClass]
    public class RestApiTest
    {
        [TestMethod]
        public void Test()
        {

        }

        [TestMethod]
        public void Get()
        {
            JArray array = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/script/{isTran}/{sid}")
                .AddUrlSegment("isTran", "0")
                .AddUrlSegment("sid", "6402348645168975878")
                .Execute()
                .To<JArray>();
            foreach (dynamic item in array)
            {
                Console.Write(item.Apn);
            }
        }
    }
}