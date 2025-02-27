using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            logInfo = log4net.LogManager.GetLogger("LogInfo");
        }
        public void OnLogAdded(string context)
        {
            LogAdded?.Invoke(context);
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

        }
    }

    //public delegate void OutputLog(string log);
}
