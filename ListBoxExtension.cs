using System.Windows.Forms;

namespace MusicBeePlugin
{
    public static class ListBoxExtension
    {
        public static void MoveSelectedItemUp(this ListBox listBox)
        {
            MoveSelectedItem(listBox, -1);
        }

        public static void MoveSelectedItemDown(this ListBox listBox)
        {
            MoveSelectedItem(listBox, 1);
        }

        private static void MoveSelectedItem(ListBox listBox, int direction)
        {
            if (!HasSelectedItem(listBox))
                return;

            int newIndex = listBox.SelectedIndex + direction;

            if (!IsIndexWithinBounds(newIndex, listBox.Items.Count))
                return;

            object selected = listBox.SelectedItem;
            CheckState checkState = SaveCheckedState(listBox);

            listBox.Items.Remove(selected);
            listBox.Items.Insert(newIndex, selected);
            listBox.SetSelected(newIndex, true);

            RestoreCheckedState(listBox, checkState, newIndex);
        }

        private static bool HasSelectedItem(ListBox listBox)
        {
            return listBox.SelectedItem != null;
        }

        private static bool IsIndexWithinBounds(int index, int itemCount)
        {
            return index >= 0 && index < itemCount;
        }

        private static CheckState SaveCheckedState(ListBox listBox)
        {
            if (listBox is CheckedListBox checkedListBox)
                return checkedListBox.GetItemCheckState(checkedListBox.SelectedIndex);

            return CheckState.Unchecked;
        }

        private static void RestoreCheckedState(ListBox listBox, CheckState checkState, int newIndex)
        {
            if (listBox is CheckedListBox checkedListBox)
                checkedListBox.SetItemCheckState(newIndex, checkState);
        }
    }
}
