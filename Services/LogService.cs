using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
    public class LogService
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
        public void Info(string context)
        {
            //logs.Enqueue(new LogModel(context, LPLogLevel.Info));
            if (logInfo.IsInfoEnabled)
            {
                logInfo.Info(context);
            }
            //log4net.LogManager.
            OnLogAdded(context);
        }

        public void Debug(string content)
        {
            if (logInfo.IsDebugEnabled)
            {
                logInfo.Debug(content);
            }
            OnLogAdded(content);
        }

        public void Warn(string type, string formName, string info)
        {
            if (logInfo.IsWarnEnabled)
                logInfo.Warn(string.Format("【{0}：{2}】{1}", type, info, formName));

            OnLogAdded(info);
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="info"></param>
        public void Warn(string info)
        {
            if (logInfo.IsWarnEnabled)
                logInfo.Warn(info);
            OnLogAdded(info);
        }
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ex"></param>
        public void Error(string info, Exception ex)
        {
            if (logInfo.IsErrorEnabled)
            {
                if (null != ex)
                    logInfo.Error(info, ex);
                else
                {
                    logInfo.Error(info);
                }
            }
            OnLogAdded(info);
        }
    }
}
