using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class ChecklistBoxPanel : UserControl
    {
        private readonly MusicBeeApiInterface mbApiInterface;
        private ItemCheckEventHandler eventHandler;
        private readonly UIManager controlStyle;
        private readonly TagsStorage tagsStorage;

        private const int PaddingWidth = 5;

        public ChecklistBoxPanel(MusicBeeApiInterface mbApiInterface, string tagName, Dictionary<string, CheckState> data = null)
        {
            this.mbApiInterface = mbApiInterface;
            controlStyle = new UIManager(mbApiInterface);
            tagsStorage = SettingsStorage.GetTagsStorage(tagName);

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

            // Sort the tags based on the settings
            var sortedTags = tagsStorage.Sorted ? data.Keys.OrderBy(tag => tag).ToList() : data.Keys.ToList();

            foreach (var tag in sortedTags)
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
            controlStyle.StyleControl(checkedListBoxWithTags);
            controlStyle.StyleControl(this);
        }

        public void RegisterItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            checkedListBoxWithTags.ItemCheck -= eventHandler;
            checkedListBoxWithTags.ItemCheck += eventHandler;
        }

        public void UnregisterItemCheckEventHandler()
        {
            checkedListBoxWithTags.ItemCheck -= eventHandler;
            eventHandler = null;
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
