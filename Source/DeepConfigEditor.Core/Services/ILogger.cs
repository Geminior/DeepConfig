namespace DeepConfigEditor.Services
{
    using System;
    using System.Diagnostics;

    public interface ILogger
    {
        void Debug(string format, params object[] args);

        void Error(Exception exception);

        void Error(string errorMessage, Exception exception);

        void Info(string format, params object[] args);

        void Warn(string format, params object[] args);
    }
}
