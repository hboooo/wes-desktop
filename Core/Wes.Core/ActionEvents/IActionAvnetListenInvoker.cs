using System;

namespace Wes.Core
{
    [Obsolete("请使用IActionListenInvoker")]
    public interface IActionAvnetListenInvoker
    {
        void Invoker(ActionDefinition obj);
    }
}
