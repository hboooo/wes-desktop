using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Controls
{
    internal interface IUpdateVisualState
    {
        void UpdateVisualState(bool useTransitions);
    }
}
