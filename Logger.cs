using System;
using System.IO;
using System.Text;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class Logger : IDisposable
    {
        private const string LOG_FILE_NAME = "mb_tags-panel.log";

        private readonly MusicBeeApiInterface _musicBeeApiInterface;

        private readonly FileInfo _fileInfo;
        private StreamWriter _writer;

        public Logger(MusicBeeApiInterface musicBeeApiInterface)
        {
            this._musicBeeApiInterface = musicBeeApiInterface;
            _fileInfo = new FileInfo(GetLogFilePath());
            _writer = new StreamWriter(_fileInfo.FullName, true, Encoding.UTF8);
        }

        private void Write(string type, string message, params object[] args)
        {
            DateTime utcTime = DateTime.UtcNow;
            _writer.WriteLine($"{utcTime:dd/MM/yyyy HH:mm:ss} [{type.ToUpper()}] {message}", args);
            _writer.Flush();
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _writer = null;
        }

        public void Debug(string message, params object[] args)
        {
            Write("debug", message, args);
        }

        public void Info(string message, params object[] args)
        {
            Write("info", message, args);
        }

        public void Warn(string message, params object[] args)
        {
            Write("warn", message, args);
        }

        public void Error(string message, params object[] args)
        {
            Write("error", message, args);
        }

        public string GetLogFilePath()
        {
            return Path.Combine(_musicBeeApiInterface.Setting_GetPersistentStoragePath(), LOG_FILE_NAME);
        }
    }
}
