using System;

namespace Wes.Desktop.Windows.Printer
{
    public class ModalItemValue
    {
        public ModalItemValue(string message, int timeout = int.MaxValue, Action<bool> callback = null)
        {
            this.Message = message;
            this.Timeout = timeout;
            this.ModalWindowCallback = callback;
        }

        public int Timeout { get; set; }

        public string Message { get; set; }

        public Action<bool> ModalWindowCallback { get; set; }
    }
}
