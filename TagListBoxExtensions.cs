// This is an open source non-commercial project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public static class TagListBoxExtensions
    {
        public static void MoveUp(this ListBox listBox) => MoveSelectedItem(listBox, -1);

        public static void MoveDown(this ListBox listBox) => MoveSelectedItem(listBox, 1);

        private static void MoveSelectedItem(ListBox listBox, int direction)
        {
            if (listBox == null || listBox.SelectedItems.Count == 0)
            {
                return;
            }

            var selectedIndex = listBox.SelectedIndex + direction;

            if (selectedIndex < 0 || selectedIndex >= listBox.Items.Count)
            {
                return;
            }

            var selectedItem = listBox.SelectedItem;
            var checkState = SaveCheckedState(listBox);

            listBox.Items.Remove(selectedItem);
            listBox.Items.Insert(selectedIndex, selectedItem);

            listBox.SetSelected(selectedIndex, true);

            RestoreCheckedState(listBox, checkState, selectedIndex);
        }

        private static CheckState SaveCheckedState(ListBox listBox)
        {
            if (listBox is CheckedListBox checkedListBox)
            {
                return checkedListBox.GetItemCheckState(checkedListBox.SelectedIndex);
            }

            return CheckState.Unchecked;
        }

        private static void RestoreCheckedState(ListBox listBox, CheckState checkState, int newIndex)
        {
            if (listBox is CheckedListBox checkedListBox)
            {
                checkedListBox.SetItemCheckState(newIndex, checkState);
            }
        }
    }
}