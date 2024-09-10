// This is an open source non-commercial project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class TagListSettingsPanel : UserControl
    {
        // private TagsStorage _tagsStorage;

        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        private SettingsManager _settingsManager;
        private TagsStorage tagsStorage;

        public TagListSettingsPanel(string tagName, SettingsManager settingsManager)
        {
            InitializeComponent();
            InitializeToolTip();
            SendMessage(TxtNewTagInput.Handle, EM_SETCUEBANNER, 0, Messages.EnterTagMessagePlaceholder);

            // Use the provided SettingsManager instance
            _settingsManager = settingsManager;
            tagsStorage = _settingsManager.RetrieveTagsStorageByTagName(tagName);

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
            toolTip.SetToolTip(CbEnableAlphabeticalTagListSorting, Messages.TagSortTooltip);
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

        public void SetUpPanelForFirstUse()
        {
            LstTags.SelectedIndex = LstTags.Items.Count > 0 ? 0 : -1;
            SetUpDownButtonsState(!tagsStorage.Sorted);
        }

        private void UpdateSortOption()
        {
            bool isSorted = tagsStorage.Sorted;
            CbEnableAlphabeticalTagListSorting.Checked = isSorted;
            // Stellen Sie sicher, dass lstTags.Sorted basierend auf dem Zustand der Checkbox gesetzt wird
            LstTags.Sorted = CbEnableAlphabeticalTagListSorting.Checked;
            SetUpDownButtonsState(!isSorted);
        }

        private void AttachEventHandlers()
        {
            LstTags.KeyDown += KeyEventHandler;
            TxtNewTagInput.KeyDown += KeyEventHandler;

            CbEnableAlphabeticalTagListSorting.CheckedChanged += CbEnableTagSort_CheckedChanged;
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
                else if (sender == LstTags)
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
                LstTags.Sorted = false;
                UpdateTags();
            }
        }

        public bool IsSortEnabled() => CbEnableAlphabeticalTagListSorting.Checked;

        public void UpdateTags()
        {
            var tags = tagsStorage.GetTags().Keys;

            // Konvertieren beider Seiten des bedingten Ausdrucks in List<string>
            var sortedOrUnsortedTags = IsSortEnabled() ? tags.OrderBy(tag => tag).ToList() : tags.ToList();

            LstTags.BeginUpdate(); // Suspend drawing of the ListBox
            LstTags.Items.Clear(); // Clear the existing items

            foreach (var tag in sortedOrUnsortedTags)
            {
                LstTags.Items.Add(tag);
            }

            LstTags.EndUpdate(); // Resume drawing of the ListBox
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
            LstTags.Items.Add(newTag);
            TxtNewTagInput.Text = string.Empty;
        }

        public void RemoveSelectedTagFromList()
        {
            if (LstTags.SelectedIndex == -1 || LstTags.Items.Count == 0)
            {
                return;
            }

            var selectedItems = LstTags.SelectedItems.Cast<string>().ToList();

            int newIndex = LstTags.SelectedIndex - selectedItems.Count;

            foreach (var selectedItem in selectedItems)
            {
                int itemIndex = LstTags.Items.IndexOf(selectedItem);
                LstTags.Items.Remove(selectedItem);
                tagsStorage.TagList.Remove(selectedItem);

                if (itemIndex < newIndex)
                {
                    newIndex--;
                }
            }

            if (LstTags.Items.Count > 0)
            {
                newIndex = Math.Max(0, newIndex);
                LstTags.SelectedIndex = newIndex;
            }
        }

        public void ClearTagsListInSettings()
        {
            LstTags.Items.Clear();
            tagsStorage.Clear();
        }

        public void ImportTagsFromCsv()
        {
            using (var openFileDialog1 = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Title = Messages.CsvDialogTitle,
                Filter = Messages.CsvFileFilter,
                DefaultExt = Messages.CsvDefaultExt,
                Multiselect = false,
                RestoreDirectory = true
            })
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var importCsvFilename = openFileDialog1.FileName;
                    if (string.IsNullOrEmpty(importCsvFilename))
                    {
                        return;
                    }

                    if (MessageBox.Show(Messages.CsvImportWarningReplaceMessage, Messages.WarningTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
                                    LstTags.Items.Add(tag);
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
                        foreach (var tag in LstTags.Items.Cast<string>())
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
            if (LstTags.SelectedItem == null || LstTags.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = LstTags.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= LstTags.Items.Count)
                return; // Index out of range - nothing to do

            // Removing removable element
            object selected = LstTags.SelectedItem;
            LstTags.Items.RemoveAt(LstTags.SelectedIndex);

            // Insert it in new position
            LstTags.Items.Insert(newIndex, selected);

            // Restore selection
            LstTags.SetSelected(newIndex, true);

            // Put the selected item to a new position
            tagsStorage.SwapElement(selected.ToString(), newIndex);
        }

        public void SortAlphabetically()
        {
            SetUpDownButtonsState(false);
            tagsStorage.Sort();
            var tags = tagsStorage.GetTags().Keys.ToList();
            tags.Sort();
            LstTags.BeginUpdate(); // Suspend drawing of the ListBox
            LstTags.Items.Clear();
            LstTags.Items.AddRange(tags.ToArray());
            LstTags.EndUpdate(); // Resume drawing of the ListBox
        }

        private void ShowConfirmationDialogToSort()
        {
            if (MessageBox.Show(Messages.TagListSortConfirmationMessage, Messages.WarningTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes && IsSortEnabled())
            {
                SortAlphabetically(); // Sortiert die Tags alphabetisch
                tagsStorage.Sorted = true; // Aktualisiert den Sortierungszustand im _tagsStorage
                LstTags.Sorted = true; // Stellt sicher, dass die ListBox weiß, dass sie sortiert ist
                CbEnableAlphabeticalTagListSorting.Checked = true; // Stellt sicher, dass die Checkbox markiert bleibt
            }
        }

        private void PromptClearTagsConfirmation()
        {
            if (MessageBox.Show(Messages.ClearAllCurrentTagsInLIstMessage, Messages.WarningTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ClearTagsListInSettings();
            }
        }
    }
}