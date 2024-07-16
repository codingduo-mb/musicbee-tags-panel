using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class TagListPanel : UserControl
    {
        private const int PaddingWidth = 5;

        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly UIManager _controlStyle;
        private readonly TagsStorage _tagsStorage;

        // Add SettingsManager parameter to the constructor
        public TagListPanel(MusicBeeApiInterface mbApiInterface, SettingsManager settingsManager, string tagName, Dictionary<string, CheckState> data = null)
        {
            _mbApiInterface = mbApiInterface;
            _controlStyle = new UIManager(mbApiInterface, new Dictionary<string, TagListPanel>(), new string[0], null);
            // Use the provided SettingsManager instance to retrieve TagsStorage
            _tagsStorage = settingsManager.RetrieveTagsStorageByTagName(tagName);

            InitializeComponent();

            if (data != null)
            {
                PopulateChecklistBoxesFromData(data);
            }

            StylePanel();
        }
        public void PopulateChecklistBoxesFromData(Dictionary<string, CheckState> data)
        {
            CheckedListBoxWithTags.BeginUpdate();
            CheckedListBoxWithTags.Items.Clear();

            // Retrieve tags from settings and sort them if necessary.
            var tagsFromSettings = _tagsStorage.GetTags().Keys;
            var sortedTagsFromSettings = _tagsStorage.Sorted ? tagsFromSettings.OrderBy(tag => tag).ToList() : tagsFromSettings.ToList();

            // Add tags from settings to the checklist box.
            foreach (var tag in sortedTagsFromSettings)
            {
                if (data.ContainsKey(tag))
                {
                    CheckedListBoxWithTags.Items.Add(tag, data[tag]);
                }
            }

            // Now, handle tags not in settings. Filter out tags already added.
            var additionalTags = data.Keys.Except(tagsFromSettings).OrderBy(tag => tag); // Ensure additional tags are sorted alphabetically.

            // Add the additional tags to the checklist box.
            foreach (var tag in additionalTags)
            {
                CheckedListBoxWithTags.Items.Add(tag, data[tag]);
            }

            CheckedListBoxWithTags.ColumnWidth = CalculateMaxStringPixelWidth(data.Keys) + PaddingWidth;
            CheckedListBoxWithTags.EndUpdate();
        }

        private int CalculateMaxStringPixelWidth(IEnumerable<string> strings)
        {
            var longestString = strings.Any() ? strings.Max(str => str.Length) : 0;
            return TextRenderer.MeasureText(new string('M', longestString), CheckedListBoxWithTags.Font).Width;
        }

        private void StylePanel()
        {
            _controlStyle.ApplySkinStyleToControl(CheckedListBoxWithTags);
            _controlStyle.ApplySkinStyleToControl(this);
        }

        public void RegisterItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            CheckedListBoxWithTags.ItemCheck -= eventHandler;
            CheckedListBoxWithTags.ItemCheck += eventHandler;
        }

        public void UnregisterItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            CheckedListBoxWithTags.ItemCheck -= eventHandler;
        }

        private void CheckedListBox1_KeyUp(object sender, KeyEventArgs e)
        {
            CheckedListBoxWithTags.CheckOnClick = true;
        }

        private void CheckedListBox1_KeyDown(object sender, KeyEventArgs e)
        {
            CheckedListBoxWithTags.CheckOnClick = false;
        }
    }
}
