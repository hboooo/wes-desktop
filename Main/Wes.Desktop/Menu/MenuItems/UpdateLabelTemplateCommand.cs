using System;
using System.IO;
using System.Threading;
using System.Windows;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.View;
using Wes.Utilities;

namespace Wes.Desktop.Menu.MenuItems
{
    [ExportMenuCommand(Header = "UpdateLabelTemplate", Tooltip = "UpdateLabelTemplate", Order = 0x0002)]
    public class UpdateLabelTemplateCommand : CommandWrapper
    {
        public override void Execute(object parameter)
        {
            Wes.Print.WesPrint.Engine.ClearDocument(); 
            ProgressWindow progress = new ProgressWindow();
            Thread thread = new Thread(() =>
            {
                int updated = 0;
                Wes.Print.WesPrint.Engine.UpdateTemplates((total, file) =>
                {
                    LoggingService.InfoFormat("Update label, path:{0}", file);
                    updated++;
                    WesApp.UiActionInvoke(() =>
                    {
                        string name = Path.GetFileName(file);
                        progress.ProgressText = $"成功更新標籤：{name}";
                        progress.MaxValue = total;
                        progress.Value = updated;
                    });
                }, (files) =>
                {
                    WesApp.UiActionInvoke(() =>
                    {
                        progress.Close();
                        WesModernDialog.ShowWesMessage($"所有標籤更新成功，共更新{files.Count}個");
                    });
                });
            });
            thread.IsBackground = true;
            thread.Start();
            progress.ShowDialog();
        }
    }
}
