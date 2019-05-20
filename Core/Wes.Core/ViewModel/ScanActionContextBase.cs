using Wes.Core.Api;

namespace Wes.Core.ViewModel
{
    public abstract class ScanActionContextBase<T, A> : ScanActionBase<T, A>
        where T : struct
        where A : IScanActionContext
    {
        
    }
}