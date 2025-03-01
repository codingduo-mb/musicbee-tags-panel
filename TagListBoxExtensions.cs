using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    /// <summary>
    /// Extension methods for operating on ListBox controls.
    /// </summary>
    public static class TagListBoxExtensions
    {
        /// <summary>
        /// Moves selected items up one position in the ListBox.
        /// </summary>
        /// <param name="listBox">The ListBox to operate on.</param>
        /// <exception cref="ArgumentNullException">Thrown when listBox is null.</exception>
        public static void MoveUp(this ListBox listBox)
        {
            if (listBox == null)
                throw new ArgumentNullException(nameof(listBox));

            MoveSelectedItems(listBox, -1);
        }

        /// <summary>
        /// Moves selected items down one position in the ListBox.
        /// </summary>
        /// <param name="listBox">The ListBox to operate on.</param>
        /// <exception cref="ArgumentNullException">Thrown when listBox is null.</exception>
        public static void MoveDown(this ListBox listBox)
        {
            if (listBox == null)
                throw new ArgumentNullException(nameof(listBox));

            MoveSelectedItems(listBox, 1);
        }

        private static void MoveSelectedItems(ListBox listBox, int direction)
        {
            if (listBox == null || listBox.SelectedItems.Count == 0)
                return;

            // Validate direction
            if (direction != -1 && direction != 1)
                throw new ArgumentException("Direction must be -1 (up) or 1 (down).", nameof(direction));

            bool isCheckedListBox = listBox is CheckedListBox;
            var selectedIndices = listBox.SelectedIndices.Cast<int>().ToList();

            // Prevent movement if any selected item is at the boundary
            bool cannotMoveUp = direction < 0 && selectedIndices.Contains(0);
            bool cannotMoveDown = direction > 0 && selectedIndices.Contains(listBox.Items.Count - 1);

            if (cannotMoveUp || cannotMoveDown)
                return;

            // Sort indices based on direction to avoid index shifting issues
            selectedIndices.Sort();
            if (direction > 0)
                selectedIndices.Reverse();

            // Store items and their check states before moving
            var itemsToMove = new List<object>(selectedIndices.Count);
            var checkStates = isCheckedListBox ? new List<CheckState>(selectedIndices.Count) : null;

            foreach (int index in selectedIndices)
            {
                itemsToMove.Add(listBox.Items[index]);

                if (isCheckedListBox)
                {
                    var checkedListBox = (CheckedListBox)listBox;
                    checkStates.Add(checkedListBox.GetItemCheckState(index));
                }
            }

            // Begin moving items
            listBox.BeginUpdate();

            try
            {
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
                        var checkedListBox = (CheckedListBox)listBox;
                        checkedListBox.SetItemCheckState(newIndex, checkStates[i]);
                    }

                    listBox.SetSelected(newIndex, true);
                }
            }
            finally
            {
                listBox.EndUpdate();
            }
        }
    }
}