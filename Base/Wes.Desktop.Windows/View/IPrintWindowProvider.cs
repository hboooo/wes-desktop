using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core.Service;

namespace Wes.Desktop.Windows.View
{
    [WSService("Wes.Desktop.Windows.View", Implementation = typeof(PrintModalWindowProvider))]
    public interface IPrintWindowProvider
    {
        /// <summary>
        /// 初始化打印模块
        /// </summary>
        void Initialize();
    }
}
