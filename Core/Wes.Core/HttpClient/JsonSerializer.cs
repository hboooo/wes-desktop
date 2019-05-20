using RestSharp.Serializers;
using Wes.Utilities;

namespace Wes.Core.HttpClient
{
    public class JsonSerializer : ISerializer
    {
        private string _contentType = "application/json";

        public string Serialize(object obj)
        {
            return Json.SerializeObject(obj);
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }

        public string ContentType
        {
            get { return this._contentType; }
            set { this._contentType = value; }
        }
    }
}