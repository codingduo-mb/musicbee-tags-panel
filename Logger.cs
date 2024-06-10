using System;
using System.IO;
using System.Text;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class Logger : IDisposable
    {
        private const string LOG_FILE_NAME = "mb_tags-panel.log";

        private readonly MusicBeeApiInterface musicBeeApiInterface;

        private readonly FileInfo fileInfo;
        private StreamWriter writer;

        public Logger(MusicBeeApiInterface musicBeeApiInterface)
        {
            this.musicBeeApiInterface = musicBeeApiInterface;
            fileInfo = new FileInfo(GetLogFilePath());
            writer = new StreamWriter(fileInfo.FullName, true, Encoding.UTF8);
        }

        private void Write(string type, string message, params object[] args)
        {
            DateTime utcTime = DateTime.UtcNow;
            writer.WriteLine($"{utcTime:dd/MM/yyyy HH:mm:ss} [{type.ToUpper()}] {message}", args);
            writer.Flush();
        }

        public void Dispose()
        {
            writer?.Dispose();
            writer = null;
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
            return Path.Combine(musicBeeApiInterface.Setting_GetPersistentStoragePath(), LOG_FILE_NAME);
        }
    }
}
