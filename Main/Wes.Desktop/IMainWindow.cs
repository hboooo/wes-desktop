using Wes.Core.Service;

namespace Wes.Desktop
{
    [WSService("Wes.Desktop", Implementation = typeof(MainWindow))]
    public interface IMainWindow
    {
        /// <summary>
        /// 关闭窗口
        /// </summary>
        void Close();
        /// <summary>
        /// 以非模态显示窗口
        /// </summary>
        void Show();
    }
}
