using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class TagsStorage
    {
        private string _tagMetaDataType;
        private Dictionary<string, int> _tagList = new Dictionary<string, int>();

        public bool ShowTagsNotInList { get; set; }

        // Add the Sorted property
        public bool Sorted { get; set; }

        public void Clear()
        {
            _tagList.Clear();
        }

        // Modify GetTags to return tags based on the Sorted property
        public Dictionary<string, int> GetTags()
        {
            if (Sorted)
            {
                // Return tags sorted alphabetically
                return _tagList.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
            }
            else
            {
                // Return tags in their original order
                return _tagList.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        public string GetTagName()
        {
            return _tagMetaDataType;
        }

        public MetaDataType GetMetaDataType()
        {
            return Enum.TryParse(_tagMetaDataType, true, out MetaDataType result) ? result : default;
        }

        public void SwapElement(string key, int newIndex)
        {
            if (_tagList.ContainsKey(key))
            {
                _tagList[key] = newIndex;
            }
        }

        public string MetaDataType
        {
            get => _tagMetaDataType;
            set => _tagMetaDataType = value;
        }

        public Dictionary<string, int> TagList
        {
            get => _tagList;
            set => _tagList = value ?? new Dictionary<string, int>();
        }
    }
}