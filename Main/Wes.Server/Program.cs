using System;
using Wes.Print;
using Wes.Utilities;

namespace Wes.Server
{
    class Program
    {
        [STAThread()]
        static void Main(string[] args)
        {
            string port = null;
            if (args != null && args.Length > 0)
            {
                port = args[0];
            }
            Run(port);
        }

        static App app;

        static void Run(string port)
        {
            try
            {
                bool run = false;
                System.Threading.Mutex mutex = new System.Threading.Mutex(true, WesApp.WES_SERVER, out run);
                if (run)
                {
                    mutex.ReleaseMutex();
                    app = new App();
                    Utils.KillProcessRemaining(WesApp.WES_SERVER, 1);
                    LoggingService.InitializeLogService();
                    var init = WesPrint.Engine;
                    MessageQueueWindow mainWindow = new MessageQueueWindow(port);
                    app.Run();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }
    }
}
