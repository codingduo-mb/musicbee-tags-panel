using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public static class TagListBoxExtensions
    {
        public static void MoveUp(this ListBox listBox)
        {
            MoveSelectedItems(listBox, -1);
        }

        public static void MoveDown(this ListBox listBox)
        {
            MoveSelectedItems(listBox, 1);
        }

        private static void MoveSelectedItems(ListBox listBox, int direction)
        {
            if (listBox == null || listBox.SelectedItems.Count == 0)
                return;

            bool isCheckedListBox = listBox is CheckedListBox;
            var selectedIndices = listBox.SelectedIndices.Cast<int>().ToList();

            // Prevent movement if any selected item is at the boundary
            if ((direction < 0 && selectedIndices.Contains(0)) ||
                (direction > 0 && selectedIndices.Contains(listBox.Items.Count - 1)))
                return;

            // Sort indices based on direction
            selectedIndices.Sort();
            if (direction > 0)
                selectedIndices.Reverse();

            var itemsToMove = new List<object>();
            var checkStates = new List<CheckState>();

            foreach (int index in selectedIndices)
            {
                itemsToMove.Add(listBox.Items[index]);

                if (isCheckedListBox)
                {
                    var clb = (CheckedListBox)listBox;
                    checkStates.Add(clb.GetItemCheckState(index));
                }
            }

            // Remove items
            foreach (int index in selectedIndices)
            {
                listBox.Items.RemoveAt(index);
            }

            // Insert items at new positions
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                int newIndex = selectedIndices[i] + direction;
                listBox.Items.Insert(newIndex, itemsToMove[i]);

                if (isCheckedListBox)
                {
                    var clb = (CheckedListBox)listBox;
                    clb.SetItemCheckState(newIndex, checkStates[i]);
                }

                listBox.SetSelected(newIndex, true);
            }
        }
    }
}