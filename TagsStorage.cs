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
        private SortedDictionary<string, int> _tagList = new SortedDictionary<string, int>();
        private bool _isAlphabeticallySorted = true;

        private bool _enableAlphabeticalSorting;
        public bool ShowTagsNotInList { get; set; }

        public void Clear()
        {
            _tagList.Clear();
        }

        public Dictionary<string, CheckState> GetTags()
        {
            return _tagList.ToDictionary(item => item.Key, item => CheckState.Unchecked);
        }

        public string GetTagName()
        {
            return _tagMetaDataType;
        }

        public MetaDataType GetMetaDataType()
        {
            return Enum.TryParse(_tagMetaDataType, true, out MetaDataType result) ? result : default;
        }

        public bool EnableAlphabeticalTagSort
        {
            get { return _enableAlphabeticalSorting; }
            set { _enableAlphabeticalSorting = value; }
        }

        public void Sort()
        {
            if (!_isAlphabeticallySorted && _enableAlphabeticalSorting)
            {
                var sortedTagList = _tagList.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.Value);
                _tagList = new SortedDictionary<string, int>(sortedTagList);
                _isAlphabeticallySorted = true;
            }
        }

        public void SortByIndex()
        {
            if (!_enableAlphabeticalSorting)
            {
                var sortedTagList = _tagList.OrderBy(item => item.Value).ToDictionary(item => item.Key, item => item.Value);
                _tagList = new SortedDictionary<string, int>(sortedTagList);
            }
        }

        public void SwapElement(string key, int position)
        {
            if (_tagList.TryGetValue(key, out int oldPosition))
            {
                _tagList[key] = position;

                var items = _tagList.Where(x => x.Value == position && x.Key != key).ToList();
                foreach (var item in items)
                {
                    _tagList[item.Key] = oldPosition;
                }
            }
        }

        public bool Sorted
        {
            get { return _isAlphabeticallySorted; }
            set
            {
                if (value != _isAlphabeticallySorted)
                {
                    _isAlphabeticallySorted = value;
                    Sort();
                }
            }
        }

        public string MetaDataType
        {
            get { return _tagMetaDataType; }
            set { _tagMetaDataType = value; }
        }

        public SortedDictionary<string, int> TagList
        {
            get { return _tagList; }
            set { _tagList = value; }
        }
    }
}
