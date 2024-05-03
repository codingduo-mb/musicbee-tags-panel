using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class TagsManipulation
    {
        public const char SEPARATOR = ';';
        private readonly MusicBeeApiInterface mbApiInterface;
        private readonly SettingsStorage settingsStorage;

        public TagsManipulation(MusicBeeApiInterface mbApiInterface, SettingsStorage settingsStorage)
        {
            this.mbApiInterface = mbApiInterface;
            this.settingsStorage = settingsStorage;
        }

        public Dictionary<string, CheckState> CombineTagLists(string[] fileNames, TagsStorage tagsStorage)
        {
            var tagList = new Dictionary<string, CheckState>();
            var stateOfSelection = new Dictionary<string, int>();
            int numberOfSelectedFiles = fileNames.Length;

            foreach (var filename in fileNames)
            {
                string[] tagsFromFile = ReadTagsFromFile(filename, tagsStorage.GetMetaDataType());
                foreach (var tag in tagsFromFile)
                {
                    stateOfSelection.TryGetValue(tag, out int count);
                    stateOfSelection[tag] = count + 1;
                }
            }

            foreach (var entry in stateOfSelection)
            {
                tagList.Add(entry.Key, entry.Value == numberOfSelectedFiles ? CheckState.Checked : CheckState.Indeterminate);
            }

            return tagList;
        }

        public string SortTagsAlphabetical(string tags)
        {
            var tagsWithoutDuplicates = new SortedSet<string>(tags.Split(SEPARATOR));
            return string.Join(SEPARATOR.ToString(), tagsWithoutDuplicates);
        }

        public string RemoveTag(string selectedTag, string fileUrl, MetaDataType metaDataType)
        {
            var tagList = new HashSet<string>(GetTags(fileUrl, metaDataType).Split(SEPARATOR));
            tagList.Remove(selectedTag);
            return string.Join(SEPARATOR.ToString(), tagList);
        }

        public string AddTag(string selectedTag, string fileUrl, MetaDataType metaDataType)
        {
            var tagList = new HashSet<string>(GetTags(fileUrl, metaDataType).Split(SEPARATOR));
            tagList.Add(selectedTag);
            return string.Join(SEPARATOR.ToString(), tagList);
        }

        public bool IsTagAvailable(string tagName, string fileUrl, MetaDataType metaDataType)
        {
            var tagList = new HashSet<string>(GetTags(fileUrl, metaDataType).Split(SEPARATOR));
            return tagList.Contains(tagName);
        }

        public string GetTags(string fileUrl, MetaDataType metaDataType)
        {
            string[] tags = ReadTagsFromFile(fileUrl, metaDataType);
            return string.Join(SEPARATOR.ToString(), tags).Trim();
        }

        public void SetTagsInFile(string[] fileUrls, CheckState selected, string selectedTag, MetaDataType metaDataType)
        {
            foreach (string fileUrl in fileUrls)
            {
                string tagsFromFile = selected == CheckState.Checked ? AddTag(selectedTag, fileUrl, metaDataType) : RemoveTag(selectedTag, fileUrl, metaDataType);
                string sortedTags = SettingsStorage.GetTagsStorage(metaDataType.ToString()).Sorted ? SortTagsAlphabetical(tagsFromFile) : tagsFromFile;

                bool result = mbApiInterface.Library_SetFileTag(fileUrl, metaDataType, sortedTags);
                mbApiInterface.Library_CommitTagsToFile(fileUrl);
            }
            mbApiInterface.MB_SetBackgroundTaskMessage("Added tags to file");
        }

        public string[] ReadTagsFromFile(string filename, MetaDataType metaDataField)
        {
            var tags = new HashSet<string>();

            if (string.IsNullOrEmpty(filename) || filename.Length <= 0 || metaDataField == 0)
            {
                return tags.ToArray();
            }

            string filetagMetaDataFields = mbApiInterface.Library_GetFileTag(filename, metaDataField);
            string[] filetagMetaDataFieldsParts = filetagMetaDataFields.Split(SEPARATOR);
            foreach (string tag in filetagMetaDataFieldsParts.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                tags.Add(tag.Trim());
            }

            return tags.ToArray();
        }

        public Dictionary<string, CheckState> UpdateTagsFromFile(string sourceFileUrl, MetaDataType metaDataType)
        {
            string[] tagParts = ReadTagsFromFile(sourceFileUrl, metaDataType);

            return tagParts.ToDictionary(tag => tag, _ => CheckState.Checked);
        }
    }
}
