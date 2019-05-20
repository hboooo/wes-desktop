using System;

namespace Wes.Print
{
    public interface ILabelPrintTemplateDownload
    {
        bool Download(string name, ref string filename);
        bool DownloadFile(string name, ref string filename);
        void DownloadFileAsync(string name, Action<string, bool> action);
    }
}
