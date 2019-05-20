using System;

namespace Wes.Server.Listener
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestUrlAttribute : Attribute
    {
        public string Url { get; set; }

        public string Description { get; set; }
    }
}
