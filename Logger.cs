// This is an open source non-commercial project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.IO;
using System.Text;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class Logger : IDisposable
    {
        private const string _logFileName = "mb_tags-panel.log";
        private readonly MusicBeeApiInterface _musicBeeApiInterface;
        private readonly FileInfo _fileInfo;
        private readonly StreamWriter _writer;
        private bool _disposed = false; // To detect redundant calls

        public Logger(MusicBeeApiInterface musicBeeApiInterface)
        {
            _musicBeeApiInterface = musicBeeApiInterface;
            _fileInfo = new FileInfo(GetLogFilePath());
            _writer = new StreamWriter(_fileInfo.FullName, true, Encoding.UTF8);
        }

        private void WriteLog(string type, string message, params object[] args)
        {
            if (_disposed) throw new ObjectDisposedException("Logger");

            var formattedMessage = string.Format(message, args);
            var logEntry = $"{DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} [{type.ToUpperInvariant()}] {formattedMessage}";
            _writer.WriteLine(logEntry);
            _writer.Flush();
        }

        public void Debug(string message, params object[] args) => WriteLog("debug", message, args);

        public void Info(string message, params object[] args) => WriteLog("info", message, args);

        public void Warn(string message, params object[] args) => WriteLog("warn", message, args);

        public void Error(string message, params object[] args) => WriteLog("error", message, args);

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