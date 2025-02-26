using System;
using System.IO;
using System.Text;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        None = 99
    }

    public class Logger : IDisposable
    {
        private const string _logFileName = "mb_tags-panel.log";
        private readonly MusicBeeApiInterface _musicBeeApiInterface;
        private readonly FileInfo _fileInfo;
        private readonly StreamWriter _writer;
        private bool _disposed = false; // To detect redundant calls

        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;

        public Logger(MusicBeeApiInterface musicBeeApiInterface)
        {
            _musicBeeApiInterface = musicBeeApiInterface;
            _fileInfo = new FileInfo(GetLogFilePath());

            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(_fileInfo.FullName));

            _writer = new StreamWriter(_fileInfo.FullName, true, Encoding.UTF8);
        }

        private void WriteLog(LogLevel level, string message, params object[] args)
        {
            if (_disposed) throw new ObjectDisposedException("Logger");

            // Skip if below minimum log level
            if (level < MinimumLogLevel)
                return;

            try
            {
                var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{level.ToString().ToUpperInvariant()}] {formattedMessage}";
                _writer.WriteLine(logEntry);
                _writer.Flush();
            }
            catch (Exception ex)
            {
                // Avoid crashing if logging fails
                try
                {
                    _writer.WriteLine($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [ERROR] Logging error: {ex.Message}");
                    _writer.Flush();
                }
                catch
                {
                    // Last-resort fallback if we can't even log the error
                }
            }
        }

        public void Debug(string message, params object[] args) => WriteLog(LogLevel.Debug, message, args);

        public void Info(string message, params object[] args) => WriteLog(LogLevel.Info, message, args);

        public void Warn(string message, params object[] args) => WriteLog(LogLevel.Warning, message, args);

        public void Error(string message, params object[] args) => WriteLog(LogLevel.Error, message, args);

        public void Error(Exception ex, string message = null)
        {
            var errorMessage = string.IsNullOrEmpty(message)
                ? $"Exception: {ex.Message}"
                : $"{message}: {ex.Message}";

            WriteLog(LogLevel.Error, $"{errorMessage}{Environment.NewLine}Stack trace: {ex.StackTrace}");
        }

        public string GetLogFilePath() => Path.Combine(_musicBeeApiInterface.Setting_GetPersistentStoragePath(), _logFileName);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _writer?.Dispose();
            }

            _disposed = true;
        }

        // Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Logger()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}