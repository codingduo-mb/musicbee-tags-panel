using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    /// <summary>
    /// Helper class for importing and exporting tags from CSV files.
    /// </summary>
    public class TagsCsvHelper
    {
        // Class-level properties for CSV configuration
        public char Delimiter { get; set; } = ';';
        public System.Text.Encoding FileEncoding { get; set; } = System.Text.Encoding.UTF8;
        public bool ExportWithHeader { get; set; } = false;
        public bool ImportSkipFirstRow { get; set; } = false;
        public string HeaderText { get; set; } = "Tag";

        private readonly Action<string, string, MessageBoxButtons, MessageBoxIcon> _showMessageBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsCsvHelper"/> class.
        /// </summary>
        /// <param name="showMessageBoxAction">Action to show message boxes.</param>
        public TagsCsvHelper(Action<string, string, MessageBoxButtons, MessageBoxIcon> showMessageBoxAction)
        {
            _showMessageBox = showMessageBoxAction ?? throw new ArgumentNullException(nameof(showMessageBoxAction));
        }

        /// <summary>
        /// Imports tags from a CSV file and adds them to the provided TagsStorage.
        /// </summary>
        /// <param name="tagsStorage">The tags storage to add imported tags to.</param>
        /// <param name="addItemToListAction">Optional action to update UI control after tag import.</param>
        /// <param name="importFilePath">Optional file path. If not provided, will show a dialog.</param>
        public void ImportTagsFromCsv(TagsStorage tagsStorage, Action<string> addItemToListAction = null, string importFilePath = null)
        {
            if (tagsStorage == null)
                throw new ArgumentNullException(nameof(tagsStorage));

            string importCsvFilename = importFilePath;

            // Show dialog only if no file path was provided
            if (string.IsNullOrEmpty(importCsvFilename))
            {
                importCsvFilename = ShowImportFileDialog();
                if (string.IsNullOrEmpty(importCsvFilename))
                {
                    return;
                }

                // Ask for confirmation only when selecting file via dialog
                if (MessageBox.Show(Messages.CsvImportWarningReplaceMessage, Messages.WarningTitle,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    _showMessageBox(Messages.CsvImportCancelMessage, Messages.CsvDialogTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            try
            {
                int importedCount = ImportTagsFromFile(importCsvFilename, tagsStorage, addItemToListAction);

                if (importedCount > 0)
                {
                    _showMessageBox($"{importedCount} {Messages.CsvImportTagImportSuccessfulMessage}",
                        Messages.CsvDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _showMessageBox(Messages.CsvImportNoTagsFoundMessage, Messages.CsvDialogTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (FileNotFoundException ex)
            {
                _showMessageBox($"File not found: {ex.Message}", Messages.WarningTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                _showMessageBox($"Error reading file: {ex.Message}", Messages.WarningTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                _showMessageBox($"Error importing tags: {ex.Message}", Messages.WarningTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ShowImportFileDialog()
        {
            using (var openFileDialog = new OpenFileDialog
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
                return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : null;
            }
        }

        private int ImportTagsFromFile(string filePath, TagsStorage tagsStorage, Action<string> addItemToListAction)
        {
            // Read all lines with appropriate encoding
            var lines = File.ReadAllLines(filePath, FileEncoding);
            if (lines.Length == 0)
                return 0;

            var importedTags = new HashSet<string>();
            int startIndex = ImportSkipFirstRow && lines.Length > 0 ? 1 : 0;

            // First pass - collect unique tags
            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = line.Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    var importTag = value.Trim();
                    if (!string.IsNullOrEmpty(importTag))
                    {
                        importedTags.Add(importTag);
                    }
                }
            }

            // Start adding at the current max index + 1
            int nextIndex = tagsStorage.TagList.Count > 0 ? tagsStorage.TagList.Values.Max() + 1 : 0;

            // Second pass - add tags to storage
            foreach (var tag in importedTags)
            {
                if (!tagsStorage.TagList.ContainsKey(tag))
                {
                    tagsStorage.TagList.Add(tag, nextIndex++);
                    addItemToListAction?.Invoke(tag);
                }
            }

            return importedTags.Count;
        }

        /// <summary>
        /// Exports tags to a CSV file.
        /// </summary>
        /// <param name="tags">The tags to export.</param>
        /// <param name="exportFilePath">Optional file path. If not provided, will show a dialog.</param>
        public void ExportTagsToCsv(IEnumerable<string> tags, string exportFilePath = null)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));

            // Filter out empty tags
            var tagsToExport = tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            if (tagsToExport.Count == 0)
            {
                _showMessageBox("No tags to export.", Messages.CsvDialogTitle, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string exportCSVFilename = exportFilePath;

            // Show dialog only if no file path was provided
            if (string.IsNullOrEmpty(exportCSVFilename))
            {
                exportCSVFilename = ShowExportFileDialog();
                if (string.IsNullOrEmpty(exportCSVFilename))
                {
                    return;
                }
            }

            try
            {
                // Use more efficient file writing with encoding options
                using (var csvWriter = new StreamWriter(exportCSVFilename, false, FileEncoding))
                {
                    // Add header if configured
                    if (ExportWithHeader)
                    {
                        csvWriter.WriteLine(HeaderText);
                    }

                    // Write tags line by line
                    foreach (var tag in tagsToExport)
                    {
                        csvWriter.WriteLine(tag);
                    }
                }

                _showMessageBox($"{Messages.CsvExportSuccessMessage} {exportCSVFilename}",
                    Messages.CsvDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException ex)
            {
                _showMessageBox($"Access denied: {ex.Message}", Messages.WarningTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (DirectoryNotFoundException ex)
            {
                _showMessageBox($"Directory not found: {ex.Message}", Messages.WarningTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                _showMessageBox($"Error writing file: {ex.Message}", Messages.WarningTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                _showMessageBox($"Error exporting tags: {ex.Message}", Messages.WarningTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Shows a file dialog for the user to select a CSV export location.
        /// </summary>
        /// <param name="initialDirectory">Optional initial directory to show in the dialog.</param>
        /// <returns>The selected file path, or null if the user cancelled the operation.</returns>
        private string ShowExportFileDialog(string initialDirectory = null)
        {
            using (var saveFileDialog = new SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                OverwritePrompt = true,
                Title = Messages.CsvDialogTitle,
                Filter = Messages.CsvFileFilter,
                DefaultExt = Messages.CsvDefaultExt,
                RestoreDirectory = true,
                InitialDirectory = initialDirectory
            })
            {
                try
                {
                    return saveFileDialog.ShowDialog() == DialogResult.OK ? saveFileDialog.FileName : null;
                }
                catch (Exception ex)
                {
                    _showMessageBox($"Error displaying file dialog: {ex.Message}", Messages.WarningTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }
    }
}
