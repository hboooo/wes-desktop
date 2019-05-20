using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Wes.Utilities.Exception
{
    public class WesRestException : WesException
    {
        public WesRestException()
        {
        }

        public WesRestException(string message) : base(message)
        {
        }

        public WesRestException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        public WesRestException(string message, object data) : base(message, data)
        {
        }

        protected WesRestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
