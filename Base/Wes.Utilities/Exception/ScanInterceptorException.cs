using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Wes.Utilities.Exception
{
    public class ScanInterceptorException : WesException
    {
        public ScanInterceptorException()
        {
        }

        public ScanInterceptorException(string message) : base(message)
        {
        }

        public ScanInterceptorException(string message, object data) : base(message, data)
        {
        }

        public ScanInterceptorException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected ScanInterceptorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}