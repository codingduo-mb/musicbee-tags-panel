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
        private Style style;

        public ChecklistBoxPanel(MusicBeeApiInterface mbApiInterface, Dictionary<String, CheckState> data = null)
        {
            this.mbApiInterface = mbApiInterface;
            style = new Style(mbApiInterface);

            InitializeComponent();

            if (data != null)
            {
                AddDataSource(data);
            }

            StylePanel();
        }

        public void AddDataSource(Dictionary<String, CheckState> data)
        {
            if (data == null)
            {
                return;
            }

            checkedListBoxWithTags.BeginUpdate();
            checkedListBoxWithTags.Items.Clear();

            foreach (var entry in data)
            {
                checkedListBoxWithTags.Items.Add(entry.Key, entry.Value);
            }

            checkedListBoxWithTags.ColumnWidth = GetLongestStringWidth(data.Keys) + 5;
            checkedListBoxWithTags.EndUpdate();
        }

        private int GetLongestStringWidth(IEnumerable<string> strings)
        {
            return strings.Any() ? TextRenderer.MeasureText(new string('M', strings.Max(str => str.Length)), checkedListBoxWithTags.Font).Width : 0;
        }

        private void StylePanel()
        {
            style.StyleControl(checkedListBoxWithTags);
            style.StyleControl(this);
        }

        public void AddItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            this.eventHandler = eventHandler;
            checkedListBoxWithTags.ItemCheck += eventHandler;
        }

        public void RemoveItemCheckEventHandler()
        {
            if (this.eventHandler != null)
            {
                checkedListBoxWithTags.ItemCheck -= this.eventHandler;
                this.eventHandler = null;
            }
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
