using Wes.Core.Base;

namespace Wes.Core.Api
{
    public interface IScanActionContext : IScanAction
    {
        object getContext();
    }
}