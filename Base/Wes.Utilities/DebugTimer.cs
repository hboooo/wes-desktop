using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Wes.Utilities
{
    public static class DebugTimer
    {
        [ThreadStatic]
        static Stack<Stopwatch> stopWatches;

        [Conditional("DEBUG")]
        public static void Start()
        {
            if (stopWatches == null)
            {
                stopWatches = new Stack<Stopwatch>();
            }
            stopWatches.Push(Stopwatch.StartNew());
        }

        [Conditional("DEBUG")]
        public static void Stop(string desc)
        {
            Stopwatch watch = stopWatches.Pop();
            watch.Stop();
            LoggingService.Debug("[" + desc + "] took " + (watch.ElapsedMilliseconds) + " ms");
        }

        /// <summary>
        /// Starts a new stopwatch and stops it when the returned object is disposed.
        /// 启动一个定时器，当对象释放时结束定时器
        /// </summary>
        /// <returns>
        /// 返回停止计时器对象(in debug builds); or null (in release builds).
        /// </returns>
        public static IDisposable Time(string text)
        {
#if DEBUG
            Stopwatch watch = Stopwatch.StartNew();
            return new CallbackOnDispose(
                delegate
                {
                    watch.Stop();
                    LoggingService.Debug("[" + text + "] took " + (watch.ElapsedMilliseconds) + " ms");
                }
            );
#else
			return null;
#endif
        }

        public static IDisposable InfoTime(string text)
        {
            Stopwatch watch = Stopwatch.StartNew();
            return new CallbackOnDispose(
                delegate
                {
                    watch.Stop();
                    LoggingService.Info("[" + text + "] took " + (watch.ElapsedMilliseconds) + " ms");
                }
            );
        }
    }
}
