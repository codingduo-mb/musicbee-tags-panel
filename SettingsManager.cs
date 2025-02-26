using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    /// <summary>
    /// Manages application settings, including loading, saving, and accessing TagsStorage objects.
    /// </summary>
    public class SettingsManager
    {
        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly Logger _logger;
        private readonly JsonSerializerSettings _serializerSettings;

        /// <summary>
        /// Gets the dictionary of tag storages, indexed by tag name.
        /// </summary>
        public Dictionary<string, TagsStorage> TagsStorages { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsManager"/> class.
        /// </summary>
        /// <param name="mbApiInterface">The MusicBee API interface.</param>
        /// <param name="log">The logger instance.</param>
        public SettingsManager(MusicBeeApiInterface mbApiInterface, Logger log)
        {
            // MusicBeeApiInterface is a value type (struct), so we can't check it against null
            _mbApiInterface = mbApiInterface;

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            _logger = log;
            TagsStorages = new Dictionary<string, TagsStorage>(StringComparer.OrdinalIgnoreCase);

            // Configure serializer settings once
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        /// <summary>
        /// Loads settings and ensures TagsStorages dictionary uses the correct keys.
        /// </summary>
        public void LoadSettingsWithFallback()
        {
            LoadSettings();

            // Ensure TagsStorages is properly initialized with tag names as keys
            if (TagsStorages != null)
            {
                TagsStorages = TagsStorages.ToDictionary(
                    storage => storage.Value.GetTagName(),
                    storage => storage.Value,
                    StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Loads settings from the settings file.
        /// </summary>
        private void LoadSettings()
        {
            string filename = GetSettingsPath();

            if (!File.Exists(filename) || new FileInfo(filename).Length == 0)
            {
                _logger.Info($"Settings file not found or empty. Creating default settings.");
                InitializeEmptySettingsFile(filename);
                return;
            }

            try
            {
                string jsonContent = File.ReadAllText(filename, Encoding.UTF8);

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    _logger.Warn("Settings file is empty. Creating default settings.");
                    InitializeEmptySettingsFile(filename);
                    return;
                }

                TagsStorages = JsonConvert.DeserializeObject<Dictionary<string, TagsStorage>>(
                    jsonContent, _serializerSettings) ?? new Dictionary<string, TagsStorage>();
            }
            catch (JsonException jsonEx)
            {
                _logger.Error($"Failed to deserialize settings: {jsonEx.Message}");
                BackupCorruptSettingsFile(filename);
                TagsStorages = new Dictionary<string, TagsStorage>();
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred while loading settings: {ex.Message}");
                BackupCorruptSettingsFile(filename);
                TagsStorages = new Dictionary<string, TagsStorage>();
            }
        }

        /// <summary>
        /// Creates a backup of a corrupted settings file.
        /// </summary>
        /// <param name="filename">The path to the settings file.</param>
        private void BackupCorruptSettingsFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    string backupFilename = $"{filename}.{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                    File.Copy(filename, backupFilename, true);
                    _logger.Info($"Created backup of corrupted settings file: {backupFilename}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to backup corrupted settings file: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates an empty settings file with default settings.
        /// </summary>
        /// <param name="filename">The path to the settings file.</param>
        private void InitializeEmptySettingsFile(string filename)
        {
            var defaultSettings = new Dictionary<string, TagsStorage>();
            TagsStorages = defaultSettings;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

                string json = JsonConvert.SerializeObject(defaultSettings, _serializerSettings);
                File.WriteAllText(filename, json, Encoding.UTF8);

                _logger.Info($"Initialized empty settings file at: {filename}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize settings file: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the path to the settings file.
        /// </summary>
        /// <returns>The full path to the settings file.</returns>
        public string GetSettingsPath()
        {
            string persistentStoragePath = _mbApiInterface.Setting_GetPersistentStoragePath();
            return Path.Combine(persistentStoragePath, Messages.SettingsFileName);
        }

        /// <summary>
        /// Saves all settings to the settings file.
        /// </summary>
        /// <returns>True if the settings were saved successfully; otherwise, false.</returns>
        public bool SaveAllSettings()
        {
            try
            {
                string settingsPath = GetSettingsPath();
                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));

                string json = JsonConvert.SerializeObject(TagsStorages, _serializerSettings);
                File.WriteAllText(settingsPath, json, Encoding.UTF8);

                _mbApiInterface.MB_SetBackgroundTaskMessage("Settings saved");
                _logger.Info("Settings saved successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to save settings: {ex.Message}");
                _mbApiInterface.MB_SetBackgroundTaskMessage("Error saving settings");
                return false;
            }
        }

        /// <summary>
        /// Retrieves the first TagsStorage object in the collection.
        /// </summary>
        /// <returns>The first TagsStorage object, or null if none exists.</returns>
        public TagsStorage RetrieveFirstTagsStorage()
        {
            return TagsStorages?.FirstOrDefault().Value;
        }

        /// <summary>
        /// Retrieves a TagsStorage object by its tag name. If it doesn't exist, a new one is created.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>The existing or newly created TagsStorage object.</returns>
        public TagsStorage RetrieveTagsStorageByTagName(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                _logger.Warn("Attempted to retrieve TagsStorage with null or empty tag name");
                tagName = "Default";
            }

            if (!TagsStorages.TryGetValue(tagName, out TagsStorage tagStorage))
            {
                tagStorage = new TagsStorage
                {
                    MetaDataType = tagName
                };
                TagsStorages.Add(tagName, tagStorage);
                _logger.Debug($"Created new TagsStorage for tag: {tagName}");
            }

            return tagStorage;
        }

        /// <summary>
        /// Sets a TagsStorage object in the collection.
        /// </summary>
        /// <param name="tagsStorage">The TagsStorage object to set.</param>
        public void SetTagsStorage(TagsStorage tagsStorage)
        {
            if (tagsStorage == null)
            {
                _logger.Warn("Attempted to set null TagsStorage");
                return;
            }

            string tagName = tagsStorage.GetTagName();
            if (string.IsNullOrWhiteSpace(tagName))
            {
                _logger.Warn("Attempted to set TagsStorage with null or empty tag name");
                return;
            }

            TagsStorages[tagName] = tagsStorage;
        }

        /// <summary>
        /// Gets the first TagsStorage object in the collection.
        /// </summary>
        /// <returns>The first TagsStorage object, or null if none exists.</returns>
        [Obsolete("Use RetrieveFirstTagsStorage instead")]
        public TagsStorage GetFirstOne()
        {
            return TagsStorages?.Values.FirstOrDefault();
        }

        /// <summary>
        /// Removes a TagsStorage object from the collection by its tag name.
        /// </summary>
        /// <param name="tagName">The name of the tag to remove.</param>
        /// <returns>True if the tag was removed successfully; otherwise, false.</returns>
        public bool RemoveTagStorage(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                _logger.Warn("Attempted to remove TagsStorage with null or empty tag name");
                return false;
            }

            bool removed = TagsStorages.Remove(tagName);
            if (removed)
            {
                _logger.Debug($"Removed TagsStorage for tag: {tagName}");
            }

            return removed;
        }

        /// <summary>
        /// Creates a deep copy of this SettingsManager instance.
        /// </summary>
        /// <returns>A new SettingsManager instance with copied settings.</returns>
        public SettingsManager DeepCopy()
        {
            var other = new SettingsManager(_mbApiInterface, _logger);
            string serialized = JsonConvert.SerializeObject(TagsStorages, _serializerSettings);
            other.TagsStorages = JsonConvert.DeserializeObject<Dictionary<string, TagsStorage>>(serialized, _serializerSettings)
                ?? new Dictionary<string, TagsStorage>();
            return other;
        }
    }
}