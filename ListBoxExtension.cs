using System;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public static class ListBoxExtension
    {
        // Use 'var' for type inference and avoid unnecessary object creations 
        private static bool IsListBoxNullOrNoSelectedItem(this ListBox listBox) => listBox == null || listBox.SelectedItems.Count == 0;

        public static void MoveUp(this ListBox listBox) => MoveSelectedItem(listBox, -1);

        public static void MoveDown(this ListBox listBox) => MoveSelectedItem(listBox, 1);

        private static void MoveSelectedItem(ListBox listBox, int direction)
        {
            if (IsListBoxNullOrNoSelectedItem(listBox)) return;

            // Check whether new index is within ListBox's bounds
            var selectedIndex = listBox.SelectedIndex + direction;

            if (!IsValidIndexForListBoxItemsCount(selectedIndex, listBox.Items.Count))
                return;

            object selectedItem = listBox.SelectedItem;
            CheckState checkState = SaveCheckedState(listBox);

            // Remove and Insert the selected item at new index location
            listBox.Items.Remove(selectedItem);
            listBox.Items.Insert(selectedIndex, selectedItem);

            listBox.SetSelected(selectedIndex, true);

            RestoreCheckedState(listBox, checkState, selectedIndex);
        }

        private static bool IsValidIndexForListBoxItemsCount(int index, int itemCount) => index >= 0 && index < itemCount;

        // Save and restore Checked state of the ListBox if it's a CheckedListBox type
        private static CheckState SaveCheckedState(ListBox listBox)
            => listBox is CheckedListBox checkedListBox ? checkedListBox.GetItemCheckState(checkedListBox.SelectedIndex) : CheckState.Unchecked;

        private static void RestoreCheckedState(ListBox listBox, CheckState checkState, int newIndex)
        {
            if (!(listBox is CheckedListBox checkedListBox)) return;

            checkedListBox.SetItemCheckState(newIndex, checkState);
        }
    }
}