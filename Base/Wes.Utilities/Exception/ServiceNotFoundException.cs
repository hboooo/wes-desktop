using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Wes.Utilities.Exception
{
    [Serializable()]
    public class ServiceNotFoundException : WesException
    {
        public ServiceNotFoundException() : base()
        {
        }

        public ServiceNotFoundException(Type serviceType) : base("Required service not found: " + serviceType.FullName)
        {
        }

        public ServiceNotFoundException(string message) : base(message)
        {
        }

        public ServiceNotFoundException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected ServiceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
