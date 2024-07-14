using System;
using System.IO;
using System.Text;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class Logger : IDisposable
    {
        private const string LogFileName = "mb_tags-panel.log";
        private readonly MusicBeeApiInterface _musicBeeApiInterface;
        private readonly FileInfo _fileInfo;
        private StreamWriter _writer;

        public Logger(MusicBeeApiInterface musicBeeApiInterface)
        {
            _musicBeeApiInterface = musicBeeApiInterface;
            _fileInfo = new FileInfo(GetLogFilePath());
            _writer = new StreamWriter(_fileInfo.FullName, true, Encoding.UTF8);
        }

        private void WriteLog(string type, string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            var logEntry = $"{DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} [{type.ToUpper()}] {formattedMessage}";
            _writer.WriteLine(logEntry);
            _writer.Flush();
        }

        public void Debug(string message, params object[] args) => WriteLog("debug", message, args);

        public void Info(string message, params object[] args) => WriteLog("info", message, args);

        public void Warn(string message, params object[] args) => WriteLog("warn", message, args);

        public void Error(string message, params object[] args) => WriteLog("error", message, args);

        public string GetLogFilePath() => Path.Combine(_musicBeeApiInterface.Setting_GetPersistentStoragePath(), LogFileName);

        public void Dispose()
        {
            _writer?.Dispose();
            _writer = null;
        }
    }
}
