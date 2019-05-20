using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Wes.Utilities
{
    public class WesApp : Application
    {
        public static string SystemInfomation = string.Empty;

        public static string CurrentWorkNo = string.Empty;

        public static string GeneralAddIn = "General";


        /// <summary>
        /// Wes.Launcher
        /// </summary>
        public static readonly string LAUNCHER_NAME = "wes.launcher";
        /// <summary>
        /// bartender
        /// </summary>
        public static readonly string BARTENDER = "bartend";
        /// <summary>
        /// Wes.Server
        /// </summary>
        public static readonly string WES_SERVER = "wes.server";

        public static readonly string WES_DESKTOP = "wes.desktop";


        private static DispatcherOperationCallback exitFrameCallback = new DispatcherOperationCallback(ExitFrame);
        /// <summary>
        /// 页面刷新
        /// </summary>
        public static void DoEvents()
        {
            DispatcherFrame nestedFrame = new DispatcherFrame();
            DispatcherOperation exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, exitFrameCallback, nestedFrame);
            Dispatcher.PushFrame(nestedFrame);
            if (exitOperation.Status != DispatcherOperationStatus.Completed)
            {
                exitOperation.Abort();
            }
        }

        private static Object ExitFrame(Object state)
        {
            DispatcherFrame frame = state as DispatcherFrame;
            frame.Continue = false;
            return null;
        }

        public static void UiActionInvoke(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// 保持页面刷新
        /// </summary>
        /// <param name="isFinished"></param>
        /// <param name="timeout"></param>
        public static void UiThreadAlive(ref bool isFinished, int timeout = 1000)
        {
            int sleepspan = 10;
            int i = 0;
            while (isFinished == false && (i + 1) * sleepspan < timeout)
            {
                Thread.Sleep(sleepspan);
                DoEvents();
                i++;
            }
        }

        /// <summary>
        /// 等待线程池所有线程执行完毕
        /// </summary>
        public static void WaitThreadPool()
        {
            int maxWorkerThreads;
            int workerThreads;
            int portThreads;
            while (true)
            {
                /*
                 GetAvailableThreads()：检索由 GetMaxThreads 返回的线程池线程的最大数目和当前活动数目之间的差值。
                 而GetMaxThreads 检索可以同时处于活动状态的线程池请求的数目。
                 通过最大数目减可用数目就可以得到当前活动线程的数目，如果为零，那就说明没有活动线程，说明所有线程运行完毕。
                 */
                ThreadPool.GetMaxThreads(out maxWorkerThreads, out portThreads);
                ThreadPool.GetAvailableThreads(out workerThreads, out portThreads);
                if (maxWorkerThreads - workerThreads == 0)
                {
                    //LoggingService.Debug("Thread Finished!");
                    break;
                }
                Thread.Sleep(5);
                DoEvents();
            }

        }
    }
}
