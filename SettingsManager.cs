// This is an open source non-commercial project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class SettingsManager
    {
        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly Logger _logger;

        public Dictionary<string, TagsStorage> TagsStorages { get; private set; }

        public SettingsManager(MusicBeeApiInterface mbApiInterface, Logger log)
        {
            _mbApiInterface = mbApiInterface;
            _logger = log;
            TagsStorages = new Dictionary<string, TagsStorage>();
        }

        public void LoadSettingsWithFallback()
        {
            LoadSettings();

            // Ensure TagsStorages is populated with the correct key (tag name)
            if (TagsStorages != null)
            {
                TagsStorages = TagsStorages.ToDictionary(storage => storage.Value.GetTagName(), storage => storage.Value);
            }
        }

        private void LoadSettings()
        {
            string filename = GetSettingsPath();

            if (!File.Exists(filename) || new FileInfo(filename).Length == 0)
            {
                // Create a default settings file
                InitializeEmptySettingsFile(filename);
                return;
            }

            try
            {
                string jsonContent = File.ReadAllText(filename, Encoding.UTF8);
                // Optional: Validate jsonContent here before deserialization
                // For example, check if jsonContent is a valid JSON structure for TagsStorages

                var serializer = new JsonSerializer();
                using (var stringReader = new StringReader(jsonContent))
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    TagsStorages = serializer.Deserialize<Dictionary<string, TagsStorage>>(jsonReader);
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.Error($"Failed to deserialize settings: {jsonEx.Message}");
                // Handle or log the error, e.g., initialize with default settings
                TagsStorages = new Dictionary<string, TagsStorage>();
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred while loading settings: {ex.Message}");
                // Handle or log the error, e.g., initialize with default settings
                TagsStorages = new Dictionary<string, TagsStorage>();
            }
        }

        private void InitializeEmptySettingsFile(string filename)
        {
            var defaultSettings = new Dictionary<string, TagsStorage>();

            var json = JsonConvert.SerializeObject(defaultSettings);

            // Write JSON directly to the file without creating a string first
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                writer.Write(json);
            }

            _logger.Info($"{nameof(InitializeEmptySettingsFile)} executed");
        }

        public string GetSettingsPath()
        {
            string persistentStoragePath = _mbApiInterface.Setting_GetPersistentStoragePath();
            return Path.Combine(persistentStoragePath, Messages.SettingsFileName);
        }

        public void SaveAllSettings()
        {
            string settingsPath = GetSettingsPath();

            var json = JsonConvert.SerializeObject(TagsStorages);
            using (var writer = new StreamWriter(settingsPath, false, Encoding.UTF8))
            {
                writer.Write(json);
            }

            _mbApiInterface.MB_SetBackgroundTaskMessage("Settings saved");
        }

        public TagsStorage RetrieveFirstTagsStorage()
        {
            return TagsStorages.FirstOrDefault().Value;
        }

        public TagsStorage RetrieveTagsStorageByTagName(string tagName)
        {
            if (!TagsStorages.TryGetValue(tagName, out TagsStorage tagStorage))
            {
                tagStorage = new TagsStorage
                {
                    MetaDataType = tagName
                };
                TagsStorages.Add(tagName, tagStorage);
            }

            return tagStorage;
        }

        public void SetTagsStorage(TagsStorage tagsStorage)
        {
            TagsStorages[tagsStorage.GetTagName()] = tagsStorage;
        }

        public TagsStorage GetFirstOne()
        {
            return TagsStorages.Values.FirstOrDefault();
        }

        public void RemoveTagStorage(string tagName)
        {
            TagsStorages.Remove(tagName);
        }

        public SettingsManager DeepCopy()
        {
            // Use a deep copy to avoid modifying the original settings
            var other = new SettingsManager(_mbApiInterface, _logger);
            other.TagsStorages = JsonConvert.DeserializeObject<Dictionary<string, TagsStorage>>(JsonConvert.SerializeObject(TagsStorages));
            return other;
        }
    }
}