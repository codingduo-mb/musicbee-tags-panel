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

    public class SettingsStorage
    {
        //checke
        private const char SettingsSeparator = ';';
        private static Dictionary<string, TagsStorage> storages;
        private const string SettingsFileName = "mb_tags-panel.Settings.json";
        private readonly MusicBeeApiInterface mbApiInterface;
        private readonly Logger log;

        public static Dictionary<string, TagsStorage> TagsStorages { get; set; }

        public SettingsStorage(MusicBeeApiInterface mbApiInterface, Logger log)
        {
            this.mbApiInterface = mbApiInterface;
            this.log = log;
            TagsStorages = new Dictionary<string, TagsStorage>();
        }

        public void LoadSettingsWithFallback()
        {
            LoadSettings();

            if (TagsStorages == null)
                return;

            TagsStorages = TagsStorages.ToDictionary(storage => storage.Value.GetTagName(), storage => storage.Value);
        }

        private void LoadSettings()
        {
            string filename = GetSettingsPath();

            if (!File.Exists(filename) || new FileInfo(filename).Length == 0)
            {
                // Create a default settings file
                CreateDefaultSettingsFile(filename);
            }

            using (var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None))
            using (var file = new StreamReader(stream, Encoding.UTF8))
            {
                JsonSerializer serializer = new JsonSerializer();
                TagsStorages = (Dictionary<string, TagsStorage>)serializer.Deserialize(file, typeof(Dictionary<string, TagsStorage>));
            }
        }

        private void CreateDefaultSettingsFile(string filename)
        {
            // Define default settings
            var defaultSettings = new Dictionary<string, TagsStorage>();

            // Serialize default settings to JSON
            var serializer = new JsonSerializer();
            using (var writer = new StreamWriter(filename))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                serializer.Serialize(jsonWriter, defaultSettings);
            }
            log.Info($"{nameof(CreateDefaultSettingsFile)} executed");
        }

        public string GetSettingsPath()
        {
            return Path.Combine(mbApiInterface.Setting_GetPersistentStoragePath(), SettingsFileName);
        }

        public void SaveAllSettings()
        {
            string settingsPath = GetSettingsPath();

            using (var file = new StreamWriter(settingsPath, false, Encoding.UTF8))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, TagsStorages);
            }
            mbApiInterface.MB_SetBackgroundTaskMessage("Settings saved");
        }

        public TagsStorage GetFirstTagsStorage()
        {
            return TagsStorages.FirstOrDefault().Value;
        }

        public static TagsStorage GetTagsStorage(string tagName)
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

        public SettingsStorage DeepCopy()
        {
            SettingsStorage other = (SettingsStorage)this.MemberwiseClone();
            storages = JsonConvert.DeserializeObject<Dictionary<string, TagsStorage>>(JsonConvert.SerializeObject(TagsStorages));
            return other;
        }

       
    }
}
