using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.Services
{
    public class LogService : ILogService
    {
        private readonly log4net.ILog logInfo;
        public event Action<string> LogAdded;
        public LogService()
        {
            logInfo = log4net.LogManager.GetLogger("LogTest");
        }
        public void OnLogAdded(string context)
        {
            LogAdded?.Invoke(context);
        }

        /**
         -Root FileAppender
         --Each Caller has own FileAppender
         */
        public log4net.ILog GetLogger([CallerMemberName] string name = "")
        {
            var repository = log4net.LogManager.CreateRepository(name);
            //获取日志对象
            log4net.ILog logger = log4net.LogManager.GetLogger(name, name);

            //log4net.GlobalContext.Properties["LogName"] = name;
            log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout(@"[%date] %m%n");
            layout.ActivateOptions();

            //配置日志【循环附加，累加】
            log4net.Appender.RollingFileAppender appender = new log4net.Appender.RollingFileAppender();

            appender.File = string.Format($"Log//{name}//");

            appender.ImmediateFlush = true;
            appender.MaxSizeRollBackups = 10;

            appender.StaticLogFileName = false;
            //appender.RollingStyle = RollingFileAppender.RollingMode.Composite;
            //appender.MaxFileSize = 10 * 1024 * 1024;
            appender.DatePattern = $"yyyyMMdd'.txt'";

            appender.LockingModel = new FileAppender.MinimalLock();

            appender.CountDirection = 0;
            appender.PreserveLogFileNameExtension = true;

            //appender.AddFilter(filter);

            appender.Layout = layout;
            appender.AppendToFile = true;
            appender.ActivateOptions();

            //配置缓存，增加日志效率
            //var aa = new BufferingForwardingAppender();
            //aa.AddAppender(appender);
            //aa.BufferSize = 500;
            //aa.Lossy = false;
            //aa.Fix = log4net.Core.FixFlags.None;
            //aa.ActivateOptions();
            log4net.Config.BasicConfigurator.Configure(repository, appender);
            return logger;
        }
        public void Log(string meesage)
        {
            if (logInfo.IsInfoEnabled)
            {
                logInfo.Info(meesage);
            }
            //log4net.LogManager.
            OnLogAdded(meesage);
        }
        public void Info(string meesage)
        {
            //logs.Enqueue(new LogModel(context, LPLogLevel.Info));
            if (logInfo.IsInfoEnabled)
            {
                logInfo.Info(meesage);
            }
            //log4net.LogManager.
            OnLogAdded(meesage);
        }

        public void Debug(string meesage)
        {
            if (logInfo.IsDebugEnabled)
            {
                logInfo.Debug(meesage);
            }
            OnLogAdded(meesage);
        }

        public void Warn(string type, string formName, string meesage)
        {
            if (logInfo.IsWarnEnabled)
                logInfo.Warn(string.Format("【{0}：{2}】{1}", type, meesage, formName));

            OnLogAdded(meesage);
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="meesage"></param>
        public void Warn(string meesage)
        {
            if (logInfo.IsWarnEnabled)
                logInfo.Warn(meesage);
            OnLogAdded(meesage);
        }
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="meesage"></param>
        /// <param name="ex"></param>
        public void Error(string meesage, Exception ex)
        {
            if (logInfo.IsErrorEnabled)
            {
                if (null != ex)
                    logInfo.Error(meesage, ex);
                else
                {
                    logInfo.Error(meesage);
                }
            }
            OnLogAdded(meesage);
        }

        public void Log(string signalInfo, Type signalType)
        {
            logInfo.Info(signalInfo);
        }
    }

    //public delegate void OutputLog(string log);
}
