using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Wes.Utilities.Exception;

namespace Wes.Utilities
{
    /// <summary>
    /// 系统日志输出
    /// ERROR 以上级别会上传至ElasticSearch，修改级别请在Wes.Utilities app.config中设置
    /// </summary>
    public sealed class LoggingService
    {
        /// <summary>
        /// 堆棧向上層級
        /// </summary>
        private static int _traceLevel = 4;

        private static log4net.ILog _Logger4net = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void InitializeLogService()
        {
#if DEBUG
            log4net.Config.XmlConfigurator.ConfigureAndWatch(
                            new System.IO.FileInfo("Wes.Utilities.Debug.config"));
#else
            log4net.Config.XmlConfigurator.ConfigureAndWatch(
                new System.IO.FileInfo("Wes.Utilities.dll.config"));
#endif
        }

        #region 
        public static bool IsDebugEnabled
        {
            get { return _Logger4net.IsDebugEnabled; }
        }
        public static bool IsInfoEnabled
        {
            get { return _Logger4net.IsInfoEnabled; }
        }
        public static bool IsWarnEnabled
        {
            get { return _Logger4net.IsWarnEnabled; }
        }
        public static bool IsErrorEnabled
        {
            get { return _Logger4net.IsErrorEnabled; }
        }
        public static bool IsFatalEnabled
        {
            get { return _Logger4net.IsFatalEnabled; }
        }
        #endregion

        #region Debug

        public static void Debug(string message)
        {
            if (IsDebugEnabled)
            {
                Log(LogLevel.Debug, message);
            }
        }

        public static void Debug(System.Exception exception)
        {
            if (IsDebugEnabled)
            {
                Log(LogLevel.Debug, exception.Message, exception);
            }
        }

        public static void Debug(string message, System.Exception exception)
        {
            if (IsDebugEnabled)
            {
                Log(LogLevel.Debug, message, exception);
            }
        }

        public static void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Log(LogLevel.Debug, format, args);
            }
        }

        public static void DebugFormat(string format, System.Exception exception, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Log(LogLevel.Debug, string.Format(format, args), exception);
            }
        }
        #endregion

        #region Info
        public static void Info(string message)
        {
            if (IsInfoEnabled)
            {
                Log(LogLevel.Info, message);
            }
        }

        public static void Info(System.Exception exception)
        {
            if (IsInfoEnabled)
            {
                Log(LogLevel.Info, exception.Message, exception);
            }
        }

        public static void Info(string message, System.Exception exception)
        {
            if (IsInfoEnabled)
            {
                Log(LogLevel.Info, message, exception);
            }
        }

        public static void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Log(LogLevel.Info, format, args);
            }
        }

        public static void InfoFormat(string format, System.Exception exception, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Log(LogLevel.Info, string.Format(format, args), exception);
            }
        }
        #endregion

        #region Warn

        public static void Warn(string message)
        {
            if (IsWarnEnabled)
            {
                Log(LogLevel.Warn, message);
            }
        }

        public static void Warn(System.Exception exception)
        {
            if (IsWarnEnabled)
            {
                Log(LogLevel.Warn, exception.Message, exception);
            }
        }

        public static void Warn(string message, System.Exception exception)
        {
            if (IsWarnEnabled)
            {
                Log(LogLevel.Warn, message, exception);
            }
        }

        public static void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Log(LogLevel.Warn, format, args);
            }
        }

        public static void WarnFormat(string format, System.Exception exception, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Log(LogLevel.Warn, string.Format(format, args), exception);
            }
        }
        #endregion

        #region Error

        public static void Error(string message)
        {
            if (IsErrorEnabled)
            {
                Log(LogLevel.Error, message);
            }
        }

        public static void Error(System.Exception exception)
        {
            if (IsErrorEnabled)
            {
                var exp = WesException.Error(exception);
                Log(LogLevel.Error, exp.Message, exp);
            }
        }

        public static void Error(string message, System.Exception exception)
        {
            if (IsErrorEnabled)
            {
                Log(LogLevel.Error, message, WesException.Error(exception));
            }
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Log(LogLevel.Error, format, args);
            }
        }

        public static void ErrorFormat(System.Exception exception, string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Log(LogLevel.Error, string.Format(format, args), WesException.Error(exception));
            }
        }
        #endregion

        #region Fatal 

        public static void Fatal(string message)
        {
            if (IsFatalEnabled)
            {
                Log(LogLevel.Fatal, message);
            }
        }

        public static void Fatal(System.Exception exception)
        {
            if (IsFatalEnabled)
            {
                var exp = WesException.Error(exception);
                Log(LogLevel.Fatal, exp.Message, exp);
            }
        }

        public static void Fatal(string message, System.Exception exception)
        {
            if (IsFatalEnabled)
            {
                Log(LogLevel.Fatal, message, WesException.Error(exception));
            }
        }

        public static void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Log(LogLevel.Fatal, format, args);
            }
        }

        public static void FatalFormat(System.Exception exception, string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Log(LogLevel.Fatal, string.Format(format, args), WesException.Error(exception));
            }
        }
        #endregion

        #region Log4net
        /// <summary>
        /// 格式化输出异常信息
        /// </summary>
        /// <param name="level"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        private static void Log(LogLevel level, string format, params object[] args)
        {
            Type type = Utils.GetMethodInvokeTypeLevel(_traceLevel);
            if (type != null)
                _Logger4net = log4net.LogManager.GetLogger(type);
            else
                _Logger4net = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            switch (level)
            {
                case LogLevel.Debug:
                    _Logger4net.DebugFormat(format, args);
                    break;
                case LogLevel.Info:
                    _Logger4net.InfoFormat(format, args);
                    break;
                case LogLevel.Warn:
                    _Logger4net.WarnFormat(format, args);
                    break;
                case LogLevel.Error:
                    _Logger4net.ErrorFormat(format, args);
                    break;
                case LogLevel.Fatal:
                    _Logger4net.FatalFormat(format, args);
                    break;
            }
        }

        /// <summary>
        /// 输出普通日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        private static void Log(LogLevel level, string message, System.Exception exception)
        {
            Type type = Utils.GetMethodInvokeTypeLevel(_traceLevel);
            if (type != null)
                _Logger4net = log4net.LogManager.GetLogger(type);
            else
                _Logger4net = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            if (exception.Data != null) exception.Data.Clear();   //elastic无法反序列化Data,清理数据
            switch (level)
            {
                case LogLevel.Debug:
                    _Logger4net.Debug(message, exception);
                    break;
                case LogLevel.Info:
                    _Logger4net.Info(message, exception);
                    break;
                case LogLevel.Warn:
                    _Logger4net.Warn(message, exception);
                    break;
                case LogLevel.Error:
                    _Logger4net.Error(message, exception);
                    break;
                case LogLevel.Fatal:
                    _Logger4net.Fatal(message, exception);
                    break;
            }
        }
        #endregion
    }


    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }
}
