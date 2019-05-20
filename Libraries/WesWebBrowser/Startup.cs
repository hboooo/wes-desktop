using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WesWebBrowser
{
    static class Startup
    {
        static Dictionary<string, string> _parameters = new Dictionary<string, string>();

        public static Dictionary<string, string> Parameters
        {
            get { return _parameters; }
        }

        static App _app;

        [STAThread]
        public static void Main(string[] args)
        {
            InitializeCommand();
            try
            {
                BuildCommandLineArgs(args);
                _app = new App();
                _app.Run(new MainWindow());
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
        }

        private static void InitializeCommand()
        {
            _parameters[Commands.TitleCommand] = "";
            _parameters[Commands.UrlCommand] = "";
        }

        public static void BuildCommandLineArgs(string[] args)
        {
            if (args == null || args.Length == 0) return;
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (_parameters.ContainsKey(args[i]) && (i + 1 < args.Length))
                    {
                        _parameters[args[i]] = args[i + 1];
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
        }
    }

    public struct Commands
    {
        public const string TitleCommand = "/title";
        public const string UrlCommand = "/uri";
    }
}
