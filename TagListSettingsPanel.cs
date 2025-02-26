using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ToolTip = System.Windows.Forms.ToolTip;

namespace MusicBeePlugin
{
    /// <summary>
    /// Represents the settings panel for managing tag lists.
    /// </summary>
    public partial class TagListSettingsPanel : UserControl, IDisposable
    {
        private const int EM_SETCUEBANNER = 0x1501;

        private const int TOOLTIP_AUTO_POPUP_DELAY = 5000;
        private const int TOOLTIP_INITIAL_DELAY = 1000;
        private const int TOOLTIP_RESHOW_DELAY = 500;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        private readonly SettingsManager _settingsManager;
        private readonly TagsStorage _tagsStorage;
        private readonly TagsCsvHelper _tagsCsvHelper;

        // Moved ToolTip to a private field so it can be used later
        private ToolTip _toolTip;

        private int _dragIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagListSettingsPanel"/> class.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <param name="settingsManager">The settings manager.</param>
        public TagListSettingsPanel(string tagName, SettingsManager settingsManager)
        {
            InitializeComponent();
            InitializeToolTip();

            if (string.IsNullOrEmpty(tagName))
                throw new ArgumentNullException(nameof(tagName), "Tag name cannot be null or empty");

            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _tagsStorage = _settingsManager.RetrieveTagsStorageByTagName(tagName) ??
                throw new InvalidOperationException($"Could not retrieve tags storage for tag name '{tagName}'");

            _tagsCsvHelper = new TagsCsvHelper(ShowMessageBox);

            SendMessage(TxtNewTagInput.Handle, EM_SETCUEBANNER, 0, Messages.EnterTagMessagePlaceholder);
            SendMessage(TxtSearchBox.Handle, EM_SETCUEBANNER, 0, Messages.SearchTagMessagePlaceholder);
            UpdateSortOption();
            UpdateTags();
            TxtNewTagInput.Focus();
            InitializeDragDrop();

            TxtSearchBox.TextChanged += TxtSearchBox_TextChanged;
        }

        private void InitializeToolTip()
        {
            _toolTip = new ToolTip
            {
                AutoPopDelay = TOOLTIP_AUTO_POPUP_DELAY,
                InitialDelay = TOOLTIP_INITIAL_DELAY,
                ReshowDelay = TOOLTIP_RESHOW_DELAY,
                ShowAlways = true
            };
            _toolTip.SetToolTip(CbEnableAlphabeticalTagListSorting, Messages.TagSortTooltip);
            _toolTip.SetToolTip(BtnAddTagToList, "Add Tag (Enter)");
            _toolTip.SetToolTip(BtnRemoveTagFromList, "Remove Selected Tags (Delete)");
            _toolTip.SetToolTip(BtnMoveTagUp, "Move Up (Ctrl+Up)");
            _toolTip.SetToolTip(BtnMoveTagDown, "Move Down (Ctrl+Down)");
            _toolTip.SetToolTip(TxtSearchBox, "Focus Search Box (Ctrl+F)");
        }

        private void FilterTagsList(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                UpdateTags();
                return;
            }

            var tags = _tagsStorage.GetTags().Keys
                .Where(tag => tag.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();

            LstTags.BeginUpdate();
            LstTags.Items.Clear();
            LstTags.Items.AddRange(tags);
            LstTags.EndUpdate();
        }

        private void SetUpDownButtonsState(bool enabled)
        {
            BtnMoveTagUp.Enabled = enabled;
            BtnMoveTagDown.Enabled = enabled;
        }

        private void ShowMessageBox(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            MessageBox.Show(message, title, buttons, icon);
        }

        private void TxtNewTagInput_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtNewTagInput.Text))
            {
                TxtNewTagInput.Text = Messages.EnterTagMessagePlaceholder;
                TxtNewTagInput.ForeColor = SystemColors.GrayText;
            }
        }

        private void TxtNewTagInput_Enter(object sender, EventArgs e)
        {
            if (TxtNewTagInput.Text == Messages.EnterTagMessagePlaceholder)
            {
                TxtNewTagInput.Text = string.Empty;
                TxtNewTagInput.ForeColor = SystemColors.WindowText;
            }
        }

        /// <summary>
        /// Sets up the panel for first use.
        /// </summary>
        public void SetUpPanelForFirstUse()
        {
            LstTags.SelectedIndex = LstTags.Items.Count > 0 ? 0 : -1;
            SetUpDownButtonsState(!_tagsStorage.Sorted);
            UpdateTags();
        }

        private void UpdateSortOption()
        {
            CbEnableAlphabeticalTagListSorting.CheckedChanged -= CbEnableTagSort_CheckedChanged;
            CbEnableAlphabeticalTagListSorting.Checked = _tagsStorage.Sorted;
            SetUpDownButtonsState(!_tagsStorage.Sorted);
            CbEnableAlphabeticalTagListSorting.CheckedChanged += CbEnableTagSort_CheckedChanged;
        }

        private void AttachEventHandlers()
        {
            LstTags.KeyDown += KeyEventHandler;
            TxtNewTagInput.KeyDown += KeyEventHandler;
            CbEnableAlphabeticalTagListSorting.CheckedChanged += CbEnableTagSort_CheckedChanged;
            this.KeyDown += KeyEventHandler;
            TxtSearchBox.TextChanged += TxtSearchBox_TextChanged;
        }

        private void KeyEventHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (sender == TxtNewTagInput)
                {
                    AddNewTagToList();
                }
                else if (sender == LstTags)
                {
                    RemoveSelectedTagFromList();
                }
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Up)
            {
                MoveUp();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Down)
            {
                MoveDown();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedTagFromList();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                TxtSearchBox.Focus();
                e.Handled = true;
            }
        }

        private void CbEnableTagSort_CheckedChanged(object sender, EventArgs e)
        {
            _tagsStorage.Sorted = CbEnableAlphabeticalTagListSorting.Checked;
            SetUpDownButtonsState(!_tagsStorage.Sorted);
            UpdateTags();
        }

        /// <summary>
        /// Updates the tags displayed in the list.
        /// </summary>
        public void UpdateTags()
        {
            var tags = _tagsStorage.GetTags().Keys.ToList();

            LstTags.BeginUpdate();
            LstTags.Items.Clear();
            LstTags.Items.AddRange(tags.ToArray());
            LstTags.EndUpdate();
        }

        /// <summary>
        /// Adds a new tag to the list.
        /// </summary>
        public void AddNewTagToList()
        {
            var newTag = TxtNewTagInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(newTag) || _tagsStorage.TagList.ContainsKey(newTag))
            {
                ShowMessageBox(string.IsNullOrWhiteSpace(newTag) ? Messages.TagInputBoxEmptyMessage : Messages.TagListAddDuplicateTagMessage, Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int newIndex = _tagsStorage.TagList.Count;
            _tagsStorage.TagList.Add(newTag, newIndex);
            UpdateTags();
            SaveChanges();
            TxtNewTagInput.Text = string.Empty;
        }

        /// <summary>
        /// Reindexes all tags in the tag list.
        /// </summary>
        private void ReindexTags()
        {
            int index = 0;
            foreach (var key in _tagsStorage.TagList.Keys.ToList())
            {
                _tagsStorage.TagList[key] = index++;
            }
        }

        /// <summary>
        /// Removes the selected tag from the list.
        /// </summary>
        public void RemoveSelectedTagFromList()
        {
            if (LstTags.SelectedIndex == -1 || LstTags.Items.Count == 0)
            {
                return;
            }

            var selectedItems = LstTags.SelectedItems.Cast<string>().ToList();

            foreach (var selectedItem in selectedItems)
            {
                _tagsStorage.TagList.Remove(selectedItem);
            }

            // Reindex remaining tags
            ReindexTags();

            UpdateTags();

            // Update selection
            if (LstTags.Items.Count > 0)
            {
                LstTags.SelectedIndex = Math.Min(LstTags.SelectedIndex, LstTags.Items.Count - 1);
            }
        }

        /// <summary>
        /// Clears the tags list in the settings and updates the UI.
        /// </summary>
        public void ClearTagsListInSettings()
        {
            _tagsStorage.Clear();
            UpdateTags();
            // Reset selection state
            SetUpDownButtonsState(!_tagsStorage.Sorted);
        }

        /// <summary>
        /// Imports tags from a CSV file.
        /// </summary>
        public void ImportTagsFromCsv()
        {
            _tagsCsvHelper.ImportTagsFromCsv(_tagsStorage, tag => LstTags.Items.Add(tag));
            UpdateTags(); // Ensure the list is fully updated
        }

        /// <summary>
        /// Exports the tags to a CSV file.
        /// </summary>
        public void ExportTagsToCsv()
        {
            _tagsCsvHelper.ExportTagsToCsv(LstTags.Items.Cast<string>());
        }

        private void BtnAddTag_Click(object sender, EventArgs e)
        {
            AddNewTagToList();
        }

        private void BtnRemTag_Click(object sender, EventArgs e)
        {
            RemoveSelectedTagFromList();
        }

        private void BtnImportCSVToList_Click(object sender, EventArgs e)
        {
            ImportTagsFromCsv();
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            ExportTagsToCsv();
        }

        private void BtnClearTagSettings_Click(object sender, EventArgs e)
        {
            if (LstTags.Items.Count != 0)
            {
                PromptClearTagsConfirmation();
            }
        }

        private void BtnMoveTagUpSettings_Click(object sender, EventArgs e)
        {
            MoveItem(-1);
        }

        private void BtnMoveTagDownSettings_Click(object sender, EventArgs e)
        {
            MoveItem(1);
        }

        /// <summary>
        /// Moves the selected tag up in the list.
        /// </summary>
        public void MoveUp()
        {
            MoveItem(-1);
        }

        /// <summary>
        /// Moves the selected tag down in the list.
        /// </summary>
        public void MoveDown()
        {
            MoveItem(1);
        }

        /// <summary>
        /// Moves the selected item in the specified direction.
        /// </summary>
        /// <param name="direction">The direction to move the item.</param>
        public void MoveItem(int direction)
        {
            // Add early return for sorting check
            if (_tagsStorage.Sorted)
            {
                return; // No moving allowed when sorted
            }

            if (LstTags.SelectedItem == null || LstTags.SelectedIndex < 0)
                return;

            int oldIndex = LstTags.SelectedIndex;
            int newIndex = oldIndex + direction;

            if (newIndex < 0 || newIndex >= LstTags.Items.Count)
                return;

            var tags = _tagsStorage.GetTags().ToList();
            var selectedTag = tags[oldIndex];
            var otherTag = tags[newIndex];

            // Swap indices in TagList
            int tempIndex = _tagsStorage.TagList[selectedTag.Key];
            _tagsStorage.TagList[selectedTag.Key] = _tagsStorage.TagList[otherTag.Key];
            _tagsStorage.TagList[otherTag.Key] = tempIndex;

            UpdateTags();
            LstTags.SetSelected(newIndex, true);
        }

        /// <summary>
        /// Saves all tag settings.
        /// </summary>
        private void SaveChanges()
        {
            _settingsManager.SaveAllSettings();
        }

        private void PromptClearTagsConfirmation()
        {
            if (MessageBox.Show(Messages.ClearAllCurrentTagsInLIstMessage, Messages.WarningTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ClearTagsListInSettings();
            }
        }

        /// <summary>
        /// Overrides OnMouseMove to show tooltip for disabled reordering buttons.
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Only show the tooltip when reordering is disabled (sorting is active)
            if (_tagsStorage.Sorted)
            {
                Point pt = this.PointToClient(Cursor.Position);
                // Check if mouse is over BtnMoveTagUp or BtnMoveTagDown bounds
                if (BtnMoveTagUp.Bounds.Contains(pt))
                {
                    _toolTip.Show("Reordering disabled when alphabetical sorting is active", this, BtnMoveTagUp.Left + BtnMoveTagUp.Width / 2, BtnMoveTagUp.Top + BtnMoveTagUp.Height / 2, 2000);
                }
                else if (BtnMoveTagDown.Bounds.Contains(pt))
                {
                    _toolTip.Show("Reordering disabled when alphabetical sorting is active", this, BtnMoveTagDown.Left + BtnMoveTagDown.Width / 2, BtnMoveTagDown.Top + BtnMoveTagDown.Height / 2, 2000);
                }
                else
                {
                    _toolTip.Hide(this);
                }
            }
            else
            {
                _toolTip.Hide(this);
            }
        }

        private void InitializeDragDrop()
        {
            LstTags.AllowDrop = true;
            LstTags.DragEnter += LstTags_DragEnter;
            LstTags.DragDrop += LstTags_DragDrop;
            LstTags.MouseDown += LstTags_MouseDown;
        }

        private void LstTags_MouseDown(object sender, MouseEventArgs e)
        {
            if (_tagsStorage.Sorted)
                return; // No drag when sorted

            _dragIndex = LstTags.IndexFromPoint(e.X, e.Y);
            if (_dragIndex != -1)
            {
                LstTags.DoDragDrop(LstTags.Items[_dragIndex].ToString(), DragDropEffects.Move);
            }
        }

        private void LstTags_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
                e.Effect = DragDropEffects.Move;
        }

        private void LstTags_DragDrop(object sender, DragEventArgs e)
        {
            if (_tagsStorage.Sorted)
                return;

            Point point = LstTags.PointToClient(new Point(e.X, e.Y));
            int dropIndex = LstTags.IndexFromPoint(point);

            if (dropIndex == -1) dropIndex = LstTags.Items.Count - 1;
            if (_dragIndex == dropIndex) return;

            // Calculate the equivalent "move" as in MoveItem method
            int direction = dropIndex > _dragIndex ? dropIndex - _dragIndex : -(_dragIndex - dropIndex);

            // Call existing MoveItem multiple times to reach the target position
            int steps = Math.Abs(direction);
            int singleDirection = direction > 0 ? 1 : -1;

            for (int i = 0; i < steps; i++)
            {
                MoveItem(singleDirection);
            }
        }

        private void TxtSearchBox_TextChanged(object sender, EventArgs e)
        {
            FilterTagsList(TxtSearchBox.Text);
        }
    }
}