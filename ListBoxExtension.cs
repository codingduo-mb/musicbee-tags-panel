using System;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public static class ListBoxExtension
    {
        private static bool IsListBoxNullOrNoSelectedItem(this ListBox listBox) => listBox?.SelectedItems.Count == 0;

        public static void MoveUp(this ListBox listBox) => MoveSelectedItem(listBox, -1);

        public static void MoveDown(this ListBox listBox) => MoveSelectedItem(listBox, 1);

        private static void MoveSelectedItem(ListBox listBox, int direction)
        {
            if (IsListBoxNullOrNoSelectedItem(listBox)) return;

            var selectedIndex = listBox.SelectedIndex + direction;

            if (!IsValidIndexForListBoxItemsCount(selectedIndex, listBox.Items.Count))
                return;

            var selectedItem = listBox.SelectedItem;
            var checkState = SaveCheckedState(listBox);

            listBox.Items.Remove(selectedItem);
            listBox.Items.Insert(selectedIndex, selectedItem);

            listBox.SetSelected(selectedIndex, true);

            RestoreCheckedState(listBox, checkState, selectedIndex);
        }

        private static bool IsValidIndexForListBoxItemsCount(int index, int itemCount) => index >= 0 && index < itemCount;

        private static CheckState SaveCheckedState(ListBox listBox)
            => listBox is CheckedListBox checkedListBox ? checkedListBox.GetItemCheckState(checkedListBox.SelectedIndex) : CheckState.Unchecked;

        private static void RestoreCheckedState(ListBox listBox, CheckState checkState, int newIndex)
        {
            if (!(listBox is CheckedListBox checkedListBox)) return;

            checkedListBox.SetItemCheckState(newIndex, checkState);
        }
    }
}