using System.Collections.Generic;
using Wes.Server.Listener;

namespace Wes.Server.Controller
{
    public class BaseController
    {
        public RequestParams Params { get; set; }

        protected bool IsParameterValid(Dictionary<string, string> query, List<string> keys)
        {
            if (keys == null || keys.Count == 0)
                return false;
            if (query == null || query.Count == 0)
                return false;

            foreach (var item in keys)
            {
                if (!query.ContainsKey(item) || string.IsNullOrEmpty(query[item]))
                {
                    ShowBalloonTip($"{Params.RequestDesc}[uri:{Params.RawUrl}]请求参数错误");
                    return false;
                }
            }
            return true;
        }

        protected void ShowBalloonTip(string message)
        {
            PopupService.ShowBalloonTip(message);
        }

        protected void ShowPrintResult(Wes.Print.ErrorCode error)
        {
            if (error == Wes.Print.ErrorCode.Success)
                ShowBalloonTip($"{Params.RequestDesc}[uri:{Params.RawUrl}]打印成功");
            else
                ShowBalloonTip($"{Params.RequestDesc}[uri:{Params.RawUrl}]打印失败，请联系管理员");
        }
    }
}
