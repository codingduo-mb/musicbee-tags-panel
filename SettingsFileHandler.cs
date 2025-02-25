using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MusicBeePlugin
{
    /// <summary>
    /// Handles the loading and initialization of settings files.
    /// </summary>
    public static class SettingsFileHandler
    {
        /// <summary>
        /// Loads the settings from the specified file.
        /// </summary>
        /// <param name="filename">The name of the file to load settings from.</param>
        /// <param name="logger">The logger to use for logging errors and information.</param>
        /// <returns>A dictionary of tag storages.</returns>
        public static Dictionary<string, TagsStorage> LoadSettings(string filename, Logger logger)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                logger.Error("Filename is null or whitespace.");
                return new Dictionary<string, TagsStorage>();
            }

            if (!File.Exists(filename) || new FileInfo(filename).Length == 0)
            {
                InitializeEmptySettingsFile(filename, logger);
                return new Dictionary<string, TagsStorage>();
            }

            try
            {
                string jsonContent = File.ReadAllText(filename, Encoding.UTF8);
                var serializer = new JsonSerializer();
                using (var stringReader = new StringReader(jsonContent))
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    return serializer.Deserialize<Dictionary<string, TagsStorage>>(jsonReader);
                }
            }
            catch (JsonException jsonEx)
            {
                logger.Error($"Failed to deserialize settings: {jsonEx.Message}");
                return new Dictionary<string, TagsStorage>();
            }
            catch (Exception ex)
            {
                logger.Error($"An unexpected error occurred while loading settings: {ex.Message}");
                return new Dictionary<string, TagsStorage>();
            }
        }

        /// <summary>
        /// Initializes an empty settings file with default settings.
        /// </summary>
        /// <param name="filename">The name of the file to initialize.</param>
        /// <param name="logger">The logger to use for logging errors and information.</param>
        public static void InitializeEmptySettingsFile(string filename, Logger logger)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                logger.Error("Filename is null or whitespace.");
                return;
            }

            var defaultSettings = new Dictionary<string, TagsStorage>();
            var json = JsonConvert.SerializeObject(defaultSettings);

            try
            {
                using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
                {
                    writer.Write(json);
                }

                logger.Info($"{nameof(InitializeEmptySettingsFile)} executed");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while initializing the settings file: {ex.Message}");
            }
        }
    }
}
