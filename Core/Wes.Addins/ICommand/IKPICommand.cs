using System.Collections.Generic;

namespace Wes.Addins.ICommand
{
    public interface IKPICommand
    {
        HashSet<long> ActionHandlerId { get; set; }

        object Initialize(object obj);

        object Execute(object obj);
    }
}
