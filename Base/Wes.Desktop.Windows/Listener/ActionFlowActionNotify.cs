using Wes.Core;
using Wes.Flow;
using Wes.Utilities.Exception;
using Wes.Wrapper;

namespace Wes.Desktop.Windows.Listener
{
    public class ActionFlowActionNotify : IActionNotity
    {
        public void Execute(params object[] objs)
        {
            if (objs == null || objs.Length == 0)
            {
                throw new WesException("當前作業單號無法獲取客戶編碼,請確認");
            }

            ActionFlowEventArgs args = objs[0] as ActionFlowEventArgs;

            if (args.Type == ActionNotityType.ExceptionAction && args.Ex != null)
            {
                WesModernDialog.ShowMessage(new WesException(args.Ex));
            }
            else if (args.Type == ActionNotityType.FirstAction)
            {
                dynamic customerObj = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, ScriptID.GET_ENDCUSTOMER_BY_WORKNO, false)
                .AddJsonBody("workNo", args.ScanValue)
                .Execute()
                .To<object>();
                string endCustomer = string.Empty;
                if ((int)customerObj.errorCode == 0)
                {
                    endCustomer = customerObj.customer.ToString();
                }

                string message = string.Empty;
                if (!WesDesktop.Instance.AddIn.IsCurrentEndCustomer(endCustomer, out message))
                {
                    WesDesktopSounds.Failed();
                    WesModernDialog.ShowWesMessage(message);
                    args.Handled = true;
                }
            }
        }
    }
}
