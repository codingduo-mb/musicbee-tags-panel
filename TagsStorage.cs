using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class TagsStorage
    {
        private string metaDataType;
        private SortedDictionary<string, int> tagList = new SortedDictionary<string, int>();
        private bool sorted = true;

        private bool enableAlphabeticalTagSort;

        public void Clear()
        {
            tagList.Clear();
        }

        public Dictionary<string, CheckState> GetTags()
        {
            return tagList.ToDictionary(item => item.Key, item => CheckState.Unchecked);
        }

        public string GetTagName()
        {
            return metaDataType;
        }

        public MetaDataType GetMetaDataType()
        {
            return Enum.TryParse(metaDataType, true, out MetaDataType result) ? result : default;
        }

        public bool EnableAlphabeticalTagSort
        {
            get { return enableAlphabeticalTagSort; }
            set { enableAlphabeticalTagSort = value; }
        }

        public void Sort()
        {
            if (!sorted && enableAlphabeticalTagSort)
            {
                var sortedTagList = tagList.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.Value);
                tagList = new SortedDictionary<string, int>(sortedTagList);
                sorted = true;
            }
        }

        public void SortByIndex()
        {
            if (!enableAlphabeticalTagSort)
            {
                var sortedTagList = tagList.OrderBy(item => item.Value).ToDictionary(item => item.Key, item => item.Value);
                tagList = new SortedDictionary<string, int>(sortedTagList);
            }
        }

        public void SwapElement(string key, int position)
        {
            if (tagList.TryGetValue(key, out int oldPosition))
            {
                tagList[key] = position;

                var item = tagList.FirstOrDefault(x => x.Value == position && x.Key != key);
                if (!item.Equals(default(KeyValuePair<string, int>)))
                {
                    tagList[item.Key] = oldPosition;
                }
            }
        }

        public bool Sorted
        {
            get { return sorted; }
            set
            {
                if (value != sorted)
                {
                    sorted = value;
                    Sort();
                }
            }
        }

        public string MetaDataType
        {
            get { return metaDataType; }
            set { metaDataType = value; }
        }

        public SortedDictionary<string, int> TagList
        {
            get { return tagList; }
            set { tagList = value; }
        }
    }
}
