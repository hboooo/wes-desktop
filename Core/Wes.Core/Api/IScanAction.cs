namespace Wes.Core.Base
{
    public interface IScanAction
    {
        void SetContext(object obj);

        void BeginScan(string val);
    }
}