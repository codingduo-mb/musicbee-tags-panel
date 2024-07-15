using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class ChecklistBoxPanel : UserControl
    {
        private const int PaddingWidth = 5;

        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly UIManager _controlStyle;
        private readonly TagsStorage _tagsStorage;

        public ChecklistBoxPanel(MusicBeeApiInterface mbApiInterface, string tagName, Dictionary<string, CheckState> data = null)
        {
            _mbApiInterface = mbApiInterface;
            _controlStyle = new UIManager(mbApiInterface);
            _tagsStorage = SettingsManager.GetTagsStorage(tagName);

            InitializeComponent();

            if (data != null)
            {
                PopulateChecklistBoxesFromData(data);
            }

            StylePanel();
        }

        public void PopulateChecklistBoxesFromData(Dictionary<string, CheckState> data)
        {
            checkedListBoxWithTags.BeginUpdate();
            checkedListBoxWithTags.Items.Clear();

            // Retrieve tags from settings and sort them if necessary.
            var tagsFromSettings = _tagsStorage.GetTags().Keys;
            var sortedTagsFromSettings = _tagsStorage.Sorted ? tagsFromSettings.OrderBy(tag => tag).ToList() : tagsFromSettings.ToList();

            // Add tags from settings to the checklist box.
            foreach (var tag in sortedTagsFromSettings)
            {
                if (data.ContainsKey(tag))
                {
                    checkedListBoxWithTags.Items.Add(tag, data[tag]);
                }
            }

            // Now, handle tags not in settings. Filter out tags already added.
            var additionalTags = data.Keys.Except(tagsFromSettings).OrderBy(tag => tag); // Ensure additional tags are sorted alphabetically.

            // Add the additional tags to the checklist box.
            foreach (var tag in additionalTags)
            {
                checkedListBoxWithTags.Items.Add(tag, data[tag]);
            }

            checkedListBoxWithTags.ColumnWidth = CalculateMaxStringPixelWidth(data.Keys) + PaddingWidth;
            checkedListBoxWithTags.EndUpdate();
        }

        private int CalculateMaxStringPixelWidth(IEnumerable<string> strings)
        {
            var longestString = strings.Any() ? strings.Max(str => str.Length) : 0;
            return TextRenderer.MeasureText(new string('M', longestString), checkedListBoxWithTags.Font).Width;
        }

        private void StylePanel()
        {
            _controlStyle.StyleControl(checkedListBoxWithTags);
            _controlStyle.StyleControl(this);
        }

        public void RegisterItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            checkedListBoxWithTags.ItemCheck -= eventHandler;
            checkedListBoxWithTags.ItemCheck += eventHandler;
        }

        public void UnregisterItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            checkedListBoxWithTags.ItemCheck -= eventHandler;
        }

        private void CheckedListBox1_KeyUp(object sender, KeyEventArgs e)
        {
            checkedListBoxWithTags.CheckOnClick = true;
        }

        private void CheckedListBox1_KeyDown(object sender, KeyEventArgs e)
        {
            checkedListBoxWithTags.CheckOnClick = false;
        }
    }
}
