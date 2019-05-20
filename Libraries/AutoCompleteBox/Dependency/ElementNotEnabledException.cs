using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class ElementNotEnabledException : Exception
    {
        // System.Windows.Automation.ElementNotEnabledException
        public ElementNotEnabledException() : base("ElementNotEnabled")
        {
            HResult = -2147220992;
        }
    }
}
