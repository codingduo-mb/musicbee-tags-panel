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
            var stateOfSelection = new Dictionary<string, int>();
            int numberOfSelectedFiles = fileNames.Length;

            foreach (var filename in fileNames)
            {
                string[] tagsFromFile = ReadTagsFromFile(filename, tagsStorage.GetMetaDataType());
                foreach (var tag in tagsFromFile)
                {
                    if (stateOfSelection.ContainsKey(tag))
                    {
                        stateOfSelection[tag]++;
                    }
                    else
                    {
                        stateOfSelection[tag] = 1;
                    }
                }
            }

            return stateOfSelection.ToDictionary(entry => entry.Key, entry => entry.Value == numberOfSelectedFiles ? CheckState.Checked : CheckState.Indeterminate);
        }

        public string SortTagsAlphabetical(string tags)
        {
            var tagsWithoutDuplicates = new SortedSet<string>(tags.Split(SEPARATOR));
            return string.Join(SEPARATOR.ToString(), tagsWithoutDuplicates);
        }

        public string RemoveTag(string selectedTag, string fileUrl, MetaDataType metaDataType)
        {
            var tagList = new HashSet<string>(GetTags(fileUrl, metaDataType).Split(new[] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries));
            tagList.Remove(selectedTag.Trim());
            return string.Join(SEPARATOR.ToString(), tagList);
        }

        public string AddTag(string selectedTag, string fileUrl, MetaDataType metaDataType)
        {
            var tagList = new HashSet<string>(GetTags(fileUrl, metaDataType).Split(new[] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries));
            tagList.Add(selectedTag.Trim());
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
            tags = tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).ToArray();
            return string.Join(SEPARATOR.ToString(), tags).TrimStart(SEPARATOR);
        }

        public void SetTagsInFile(string[] fileUrls, CheckState selected, string selectedTag, MetaDataType metaDataType)
        {
            foreach (string fileUrl in fileUrls)
            {
                string tagsFromFile = selected == CheckState.Checked ? AddTag(selectedTag, fileUrl, metaDataType) : RemoveTag(selectedTag, fileUrl, metaDataType);
                string sortedTags = SettingsStorage.GetTagsStorage(metaDataType.ToString()).Sorted ? SortTagsAlphabetical(tagsFromFile) : tagsFromFile;

                mbApiInterface.Library_SetFileTag(fileUrl, metaDataType, sortedTags);
                mbApiInterface.Library_CommitTagsToFile(fileUrl);
            }
            mbApiInterface.MB_SetBackgroundTaskMessage("Added tags to file");
        }

        public string[] ReadTagsFromFile(string filename, MetaDataType metaDataField)
        {
            if (string.IsNullOrEmpty(filename) || metaDataField == 0)
            {
                return Array.Empty<string>();
            }

            string filetagMetaDataFields = mbApiInterface.Library_GetFileTag(filename, metaDataField);
            return filetagMetaDataFields.Split(SEPARATOR).Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToArray();
        }

        public Dictionary<string, CheckState> UpdateTagsFromFile(string sourceFileUrl, MetaDataType metaDataType)
        {
            return ReadTagsFromFile(sourceFileUrl, metaDataType).ToDictionary(tag => tag, _ => CheckState.Checked);
        }
    }
}
