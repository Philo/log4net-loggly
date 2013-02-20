using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ServiceStack.Text;
using log4net.Core;

namespace log4net.loggly
{
    public class LogglyFormatter : ILogglyFormatter
    {
        private Process _currentProcess;

        public LogglyFormatter()
        {
            _currentProcess = Process.GetCurrentProcess();
        }

        public virtual void AppendAdditionalLoggingInformation(ILogglyAppenderConfig config, LoggingEvent loggingEvent)
        {
        }

        public virtual string ToJson(LoggingEvent loggingEvent)
        {
            return PreParse(loggingEvent).ToJson();
        }

        public virtual string ToJson(IEnumerable<LoggingEvent> loggingEvents)
        {
            return loggingEvents.Select(PreParse).ToJson();
        }

        private object PreParse(LoggingEvent loggingEvent)
        {
            var logItems = new Dictionary<string, object>
		                       {
		                           {"level", loggingEvent.Level.DisplayName},
		                           {"timestamp", loggingEvent.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz")},
		                           {"machine", Environment.MachineName},
		                           {"process", _currentProcess.ProcessName},
		                           {"thread", loggingEvent.ThreadName},
                                   {"identity", string.IsNullOrWhiteSpace(loggingEvent.Identity) ? null : loggingEvent.Identity},
                                   {"username", loggingEvent.UserName},
                                   {"logger", loggingEvent.LoggerName},
		                           {"message", loggingEvent.RenderedMessage}
		                       };

            var exceptionString = loggingEvent.GetExceptionString();
            if (!string.IsNullOrWhiteSpace(exceptionString))
            {
                logItems.Add("exception", exceptionString);
            }

            var properties = GetProperties(loggingEvent);
            return (properties == null ? logItems : logItems.Concat(properties)).ToDictionary(x => x.Key, y => y.Value);
        }

        private static IEnumerable<KeyValuePair<string, object>> GetProperties(LoggingEvent loggingEvent)
        {
            var keys = loggingEvent.GetProperties().GetKeys();
            var props = keys.Select(x => new KeyValuePair<string, object>(x, loggingEvent.GetProperties()[x])).ToDictionary(x => x.Key, y => y.Value);

            return props.Any() ? props : null;

        }
    }
}