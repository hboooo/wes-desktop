using Wes.Wrapper;

namespace Wes.Desktop.Windows
{
    public class MasterAuthorService
    {
        /// <summary>
        /// 權限驗證
        /// </summary>
        /// <param name="type">權限類型</param>
        /// <returns></returns>
        public static bool Authorization(VerificationType type = VerificationType.Print)
        {

            bool authored = false;
            switch (type)
            {
                case VerificationType.Print:
                    authored = WesDesktop.Instance.Authority.IsPrint == 1 ? true : false;
                    break;
                case VerificationType.Delete:
                    authored = WesDesktop.Instance.Authority.IsDelete == 1 ? true : false;
                    break;
                case VerificationType.NotCheckLotNo:
                    authored = WesDesktop.Instance.Authority.IsNotCheckLogNo == 1 ? true : false;
                    break;
                default:
                    break;
            }
            if (authored)
            {
                return true;
            }
            else
            {
                MasterAuthorWindow authorWindow = new MasterAuthorWindow(type);
                bool? res = authorWindow.ShowDialog();
                if (res == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
