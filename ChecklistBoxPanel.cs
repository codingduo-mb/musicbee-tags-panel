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
        private readonly Style controlStyle;

        private const int PaddingWidth = 5;

        public ChecklistBoxPanel(MusicBeeApiInterface mbApiInterface, Dictionary<string, CheckState> data = null)
        {
            this.mbApiInterface = mbApiInterface;
            controlStyle = new Style(mbApiInterface);

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

            foreach (var entry in data)
            {
                checkedListBoxWithTags.Items.Add(entry.Key, entry.Value);
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
