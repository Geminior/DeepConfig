namespace DeepConfigEditor.Services
{
    using System;
    using System.Diagnostics;
    using Caliburn.Micro;

    public class Logger : ILog, ILogger
    {
        private NLog.Logger _logger;

        public Logger(NLog.Logger logger)
        {
            _logger = logger;
        }

        public static ILogger Instance
        {
            get;
            set;
        }

        public void Error(Exception exception)
        {
            Error("[ERROR] ", exception);
        }

        public void Error(string errorMessage, Exception exception)
        {
            _logger.ErrorException(errorMessage, exception);
        }

        public void Info(string format, params object[] args)
        {
            _logger.Info(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            _logger.Warn(format, args);
        }
                
        public void Debug(string format, params object[] args)
        {
            DoDebug(format, args);
        }

        [Conditional("DEBUG")]
        private void DoDebug(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }
    }
}
