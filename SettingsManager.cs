using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class SavedSettingsType
    {
        public Dictionary<string, TagsStorage> TagStorages { get; set; }
    }

    public class SettingsManager
    {
        private const char SettingsSeparator = ';';
        private static Dictionary<string, TagsStorage> _storages;
        private const string SettingsFileName = "mb_tags-panel.Settings.json";
        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly Logger _logger;

        public static Dictionary<string, TagsStorage> TagsStorages { get; set; }

        public SettingsManager(MusicBeeApiInterface mbApiInterface, Logger log)
        {
            _mbApiInterface = mbApiInterface;
            _logger = log;
            TagsStorages = new Dictionary<string, TagsStorage>();
        }

        public void LoadSettingsWithFallback()
        {
            LoadSettings();

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

            using (var streamReader = new StreamReader(filename, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                TagsStorages = serializer.Deserialize<Dictionary<string, TagsStorage>>(jsonReader);
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
            return Path.Combine(persistentStoragePath, SettingsFileName);
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

        public static TagsStorage RetrieveTagsStorageByTagName(string tagName)
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
            SettingsManager other = (SettingsManager)this.MemberwiseClone();
            _storages = JsonConvert.DeserializeObject<Dictionary<string, TagsStorage>>(JsonConvert.SerializeObject(TagsStorages));
            return other;
        }

       
    }
}
