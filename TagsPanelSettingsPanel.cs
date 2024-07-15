using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class TagsPanelSettingsPanel : UserControl
    {
        private TagsStorage tagsStorage;

        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public TagsPanelSettingsPanel(string tagName)
        {
            InitializeComponent();
            InitializeToolTip();
            SendMessage(TxtNewTagInput.Handle, EM_SETCUEBANNER, 0, Messages.EnterTagMessagePlaceholder);
            tagsStorage = SettingsManager.RetrieveTagsStorageByTagName(tagName);
            UpdateTags();
            UpdateSortOption();
            AttachEventHandlers(); // this must be at the very end to suppress the events
            TxtNewTagInput.Focus(); // Set focus to the textbox
        }

        private void InitializeToolTip()
        {
            var toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                ShowAlways = true
            };
            toolTip.SetToolTip(cbEnableAlphabeticalTagSort, Messages.TagSortTooltip);
        }

        private void SetUpDownButtonsState(bool enabled)
        {
            btnTagUp.Enabled = enabled;
            btnTagDown.Enabled = enabled;
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

        public void SetUpPanelForFirstUse()
        {
            lstTags.SelectedIndex = lstTags.Items.Count > 0 ? 0 : -1;
            SetUpDownButtonsState(!tagsStorage.Sorted);
        }

        private void UpdateSortOption()
        {
            bool isSorted = tagsStorage.Sorted;
            cbEnableAlphabeticalTagSort.Checked = isSorted;
            // Stellen Sie sicher, dass lstTags.Sorted basierend auf dem Zustand der Checkbox gesetzt wird
            lstTags.Sorted = cbEnableAlphabeticalTagSort.Checked;
            SetUpDownButtonsState(!isSorted);
        }

        private void AttachEventHandlers()
        {
            lstTags.KeyDown += KeyEventHandler;
            TxtNewTagInput.KeyDown += KeyEventHandler;

            cbEnableAlphabeticalTagSort.CheckedChanged += CbEnableTagSort_CheckedChanged;
        }

        private void KeyEventHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (sender == TxtNewTagInput)
                {
                    e.SuppressKeyPress = true;
                    AddNewTagToList();
                    e.Handled = true;
                }
                else if (sender == lstTags)
                {
                    e.SuppressKeyPress = true;
                    RemoveSelectedTagFromList();
                    e.Handled = true;
                }
            }
        }

        private void CbEnableTagSort_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Checked)
            {
                ShowConfirmationDialogToSort();

                SetUpDownButtonsState(false);
                UpdateTags();
            }
            else
            {
                SetUpDownButtonsState(true);
                tagsStorage.Sorted = false;
                lstTags.Sorted = false;
                UpdateTags();
            }
        }

        public bool IsSortEnabled() => cbEnableAlphabeticalTagSort.Checked;

        public void UpdateTags()
        {
            var tags = tagsStorage.GetTags().Keys;

            // Konvertieren beider Seiten des bedingten Ausdrucks in List<string>
            var sortedOrUnsortedTags = IsSortEnabled() ? tags.OrderBy(tag => tag).ToList() : tags.ToList();

            lstTags.BeginUpdate(); // Suspend drawing of the ListBox
            lstTags.Items.Clear(); // Clear the existing items

            foreach (var tag in sortedOrUnsortedTags)
            {
                lstTags.Items.Add(tag);
            }

            lstTags.EndUpdate(); // Resume drawing of the ListBox
        }

        public void AddNewTagToList()
        {
            var newTag = TxtNewTagInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(newTag) || tagsStorage.TagList.ContainsKey(newTag))
            {
                ShowMessageBox(string.IsNullOrWhiteSpace(newTag) ? Messages.TagInputBoxEmptyMessage : Messages.TagListAddDuplicateTagMessage, Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            tagsStorage.TagList.Add(newTag, tagsStorage.TagList.Count);
            lstTags.Items.Add(newTag);
            TxtNewTagInput.Text = string.Empty;
        }

        public void RemoveSelectedTagFromList()
        {
            if (lstTags.SelectedIndex == -1 || lstTags.Items.Count == 0)
            {
                return;
            }

            var selectedItems = lstTags.SelectedItems.Cast<string>().ToList();

            int newIndex = lstTags.SelectedIndex - selectedItems.Count;

            foreach (var selectedItem in selectedItems)
            {
                int itemIndex = lstTags.Items.IndexOf(selectedItem);
                lstTags.Items.Remove(selectedItem);
                tagsStorage.TagList.Remove(selectedItem);

                if (itemIndex < newIndex)
                {
                    newIndex--;
                }
            }

            if (lstTags.Items.Count > 0)
            {
                newIndex = Math.Max(0, newIndex);
                lstTags.SelectedIndex = newIndex;
            }
        }

        public void ClearTagsListInSettings()
        {
            lstTags.Items.Clear();
            tagsStorage.Clear();
        }

        public void ImportTagsFromCsv()
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.CheckPathExists = true;

                openFileDialog1.Title = Messages.CsvDialogTitle;
                openFileDialog1.Filter = Messages.CsvFileFilter;
                openFileDialog1.DefaultExt = Messages.CsvDefaultExt;
                openFileDialog1.Multiselect = false;

                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var importCsvFilename = openFileDialog1.FileName;
                    if (string.IsNullOrEmpty(importCsvFilename))
                    {
                        return;
                    }

                    var dialogResult = MessageBox.Show(Messages.CsvImportWarningReplaceMessage, Messages.WarningTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        var lines = File.ReadAllLines(importCsvFilename);
                        var importedTags = new HashSet<string>();

                        foreach (var line in lines)
                        {
                            var values = line.Split(';');
                            foreach (var value in values)
                            {
                                var importTag = value.Trim();
                                if (!string.IsNullOrEmpty(importTag))
                                {
                                    importedTags.Add(importTag);
                                }
                            }
                        }

                        if (importedTags.Count > 0)
                        {
                            foreach (var tag in importedTags)
                            {
                                if (!tagsStorage.TagList.ContainsKey(tag))
                                {
                                    tagsStorage.TagList.Add(tag, tagsStorage.TagList.Count);
                                    lstTags.Items.Add(tag);
                                }
                            }
                            ShowMessageBox($"{importedTags.Count} {Messages.CsvImportTagImportSuccesfullMessage}", Messages.CsvDialogTitle);
                        }
                        else
                        {
                            ShowMessageBox(Messages.CsvImportNoTagsFoundMessage, Messages.CsvDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        ShowMessageBox(Messages.CsvImportCancelMessage, Messages.CsvDialogTitle);
                    }
                }
            }
        }

        public void ExportTagsToCsv()
        {
            using (var saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.CheckFileExists = false;
                saveFileDialog1.Title = Messages.CsvDialogTitle;
                saveFileDialog1.Filter = Messages.CsvFileFilter;
                saveFileDialog1.DefaultExt = Messages.CsvDefaultExt;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var exportCSVFilename = saveFileDialog1.FileName;

                    using (var csvWriter = new StreamWriter(exportCSVFilename))
                    {
                        foreach (var tag in lstTags.Items.Cast<string>())
                        {
                            csvWriter.WriteLine(tag);
                        }
                    }

                    ShowMessageBox($"{Messages.CsvExportSuccessMessage} {exportCSVFilename}", Messages.CsvDialogTitle);
                }
            }
        }

        private void BtnAddTag_Click(object sender, EventArgs e)
        {
            AddNewTagToList();
        }

        private void BtnRemTag_Click(object sender, EventArgs e)
        {
            RemoveSelectedTagFromList();
        }

        private void BtnImportCsv_Click(object sender, EventArgs e)
        {
            ImportTagsFromCsv();
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            ExportTagsToCsv();
        }

        private void BtnClearTagSettings_Click(object sender, EventArgs e)
        {
            if (lstTags.Items.Count != 0)
            {
                ShowDialogToClearList();
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

        public void MoveUp()
        {
            MoveItem(-1);
        }

        public void MoveDown()
        {
            MoveItem(1);
        }

        public void MoveItem(int direction)
        {
            // Checking selected item
            if (lstTags.SelectedItem == null || lstTags.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = lstTags.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= lstTags.Items.Count)
                return; // Index out of range - nothing to do

            // Removing removable element
            object selected = lstTags.SelectedItem;
            lstTags.Items.RemoveAt(lstTags.SelectedIndex);

            // Insert it in new position
            lstTags.Items.Insert(newIndex, selected);

            // Restore selection
            lstTags.SetSelected(newIndex, true);

            // Put the selected item to a new position
            tagsStorage.SwapElement(selected.ToString(), newIndex);
        }

        public void SortAlphabetically()
        {
            SetUpDownButtonsState(false);
            tagsStorage.Sort();
            var tags = tagsStorage.GetTags().Keys.ToList();
            tags.Sort();
            lstTags.BeginUpdate(); // Suspend drawing of the ListBox
            lstTags.Items.Clear();
            lstTags.Items.AddRange(tags.ToArray());
            lstTags.EndUpdate(); // Resume drawing of the ListBox
        }

        private void ShowConfirmationDialogToSort()
        {
            DialogResult dialogResult = MessageBox.Show(Messages.TagListSortConfirmationMessage, Messages.WarningTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes && IsSortEnabled())
            {
                SortAlphabetically(); // Sortiert die Tags alphabetisch
                tagsStorage.Sorted = true; // Aktualisiert den Sortierungszustand im tagsStorage
                lstTags.Sorted = true; // Stellt sicher, dass die ListBox weiß, dass sie sortiert ist
                cbEnableAlphabeticalTagSort.Checked = true; // Stellt sicher, dass die Checkbox markiert bleibt
            }
        }

        private void ShowDialogToClearList()
        {
            DialogResult dialogResult = MessageBox.Show(Messages.ClearAllCurrentTagsInLIstMessage, Messages.WarningTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                ClearTagsListInSettings();
            }
        }
    }
}