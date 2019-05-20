using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wes.Print;
using Wes.Utilities;
using System.Linq;
using System.IO;
using System.Windows.Media.Imaging;

namespace Wes.Desktop.Windows.Printer
{
    /// <summary>
    /// 一次打印多个标签
    /// 同时可能会有多个防呆设备与打印机一一对应,因此要检测多个打印机标签都被取走
    /// </summary>
    public class MutiPrintModalWindow : PrintModalWindowBase, IPrintModalWindow
    {
        public List<string> Printers { get; set; }
        public ModalItemValue ItemValue { get; set; }
        public List<PrintTemplateModel> Templates { get; set; }
        public List<PrintTemplateModel> PrintedTemplates { get; set; }

        public void ShowModalWindow()
        {
            if (Templates == null || Templates.Count == 0) return;
            if (Printers == null || Printers.Count == 0) return;
            if (ItemValue == null) return;

            int printerCount = Printers.Distinct().Count();
            HashSet<string> md5Strs = new HashSet<string>();
            OpenSerialPortByStep(Printers, md5Strs, 0, () =>
            {
                //串口全部正常打開時,才顯示遮蓋提示層
                if (md5Strs.Count > 0)
                {
                    base.ShowMaskModal(ItemValue.Message, GetCountString(), () =>
                    {
                        MaskCloseCallback(ItemValue.ModalWindowCallback, false);  //按下ESC时,关闭窗体
                    });

                    if (ItemValue.Timeout != int.MaxValue)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            //等待Timeout时间后关闭窗体
                            bool isFinished = false;
                            WesApp.UiThreadAlive(ref isFinished, ItemValue.Timeout * 1000);
                            MaskCloseCallback(ItemValue.ModalWindowCallback, false);
                        });
                    }
                }
            });
        }

        /// <summary>
        /// 依次打開相關串口
        /// 有成功打開的串口才打開遮蓋層
        /// </summary>
        /// <param name="printers"></param>
        /// <param name="md5Strs"></param>
        /// <param name="index"></param>
        /// <param name="callback"></param>
        private void OpenSerialPortByStep(List<string> printers, HashSet<string> md5Strs, int index, Action callback)
        {
            if (printers == null || printers.Count == 0 || index >= printers.Count)
            {
                callback();
                return;
            }

            string item = printers[index];
            string md5 = item.GetStringMd5String();
            SerialPortManager.OpenSerialPort(md5,
                (openResult, com) =>
                {
                    //成功打开串口添加到打开记录中
                    if (openResult) md5Strs.Add(com);

                    OpenSerialPortByStep(printers, md5Strs, ++index, callback);
                },
                (com) =>
                {
                    //收到串口数据后移除当前串口
                    md5Strs.Remove(com);
                    //收到全部串口数据时,关闭窗体
                    if (md5Strs.Count == 0) MaskCloseCallback(ItemValue.ModalWindowCallback, true);
                }
             );
        }

        public void UpdateLabelCount()
        {
            base.UpdateLabelCount(GetCountString());
        }

        private string GetCountString()
        {
            int remainingCount = this.Templates.Count - (PrintedTemplates == null ? 0 : PrintedTemplates.Count);
            return (remainingCount >= 0 == true ? remainingCount : 0).ToString();
        }

        public void UpdateLabelImage(string name)
        {
            string md5 = name.GetStringMd5String();
            string path = Path.Combine(AppPath.LabelTemplateImagePath, md5 + ".png");
            if (File.Exists(path))
            {
                BitmapImage bitmapImage = Utils.GetBitmapImage(path);
                if (bitmapImage != null) base.UpdateLabelImage(bitmapImage);
            }
        }
    }
}
