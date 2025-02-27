using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    /// <summary>
    /// Represents a metadata item for display in a dropdown list.
    /// </summary>
    public class MetaDataTypeItem
    {
        /// <summary>
        /// Gets or sets the metadata type.
        /// </summary>
        public MetaDataType MetaDataType { get; set; }

        /// <summary>
        /// Gets or sets the display value shown in the UI.
        /// </summary>
        public string DisplayValue { get; set; }
    }

    /// <summary>
    /// Form that allows users to select a metadata tag type from a dropdown list.
    /// </summary>
    public partial class TagListSelectorForm : Form
    {
        private const int MinFormWidth = 350;
        private const int MinFormHeight = 150;

        private readonly List<MetaDataTypeItem> _metaDataTypeItems;
        private readonly UIManager _uiManager;

        /// <summary>
        /// Blacklisted metadata types that shouldn't be selectable.
        /// </summary>
        private static readonly HashSet<MetaDataType> _blacklistedMetaDataTypes = new HashSet<MetaDataType>
                {
                    MetaDataType.Artwork,
                    MetaDataType.DiscNo,
                    MetaDataType.DiscCount,
                    MetaDataType.Encoder,
                    MetaDataType.GenreCategory,
                    MetaDataType.HasLyrics,
                    MetaDataType.Lyrics,
                    MetaDataType.TrackCount,
                    MetaDataType.Rating,
                    MetaDataType.RatingAlbum,
                    MetaDataType.RatingLove
                };

        /// <summary>
        /// Initializes a new instance of the <see cref="TagListSelectorForm"/> class.
        /// </summary>
        /// <param name="usedTags">Collection of tags that are already in use and should not be available for selection.</param>
        /// <param name="mbApiInterface">Reference to MusicBee API interface for styling.</param>
        /// <exception cref="ArgumentNullException">Thrown if usedTags is null.</exception>
        public TagListSelectorForm(IEnumerable<string> usedTags, MusicBeeApiInterface? mbApiInterface = default)
        {
            if (usedTags == null)
                throw new ArgumentNullException(nameof(usedTags));

            InitializeComponent();
            KeyPreview = true; // Enable form to receive key events first

            // Set minimum size constraints
            MinimumSize = new Size(MinFormWidth, MinFormHeight);

            BtnComboBoxAddMetaDataType.DialogResult = DialogResult.OK;
            BtnComboBoxMetaDataTypCancel.DialogResult = DialogResult.Cancel;

            // Configure UI manager if API interface is provided
            if (mbApiInterface.HasValue)
            {
                _uiManager = new UIManager(mbApiInterface.Value, null, null, null);
                ApplySkinStyles();
            }

            // Initialize and populate the ComboBox
            _metaDataTypeItems = GetAvailableMetaDataTypes(usedTags);
            InitializeComboBox();

            // Set up event handlers
            ComboBoxTagSelect.Validating += ComboBoxTagSelect_Validating;
            ComboBoxTagSelect.SelectedIndexChanged += ComboBoxTagSelect_SelectedIndexChanged;
            KeyDown += TagListSelectorForm_KeyDown;
            ComboBoxTagSelect.KeyDown += ComboBoxTagSelect_KeyDown;
            BtnComboBoxAddMetaDataType.Enabled = false;

            // Set focus to the ComboBox when the form loads
            this.Load += (s, e) => ComboBoxTagSelect.Focus();
        }

        /// <summary>
        /// Applies skin styles to form controls using the UI manager.
        /// </summary>
        private void ApplySkinStyles()
        {
            if (_uiManager == null)
                return;

            _uiManager.ApplySkinStyleToControl(this);
            _uiManager.ApplySkinStyleToControl(ComboBoxTagSelect);
            _uiManager.ApplySkinStyleToControl(BtnComboBoxAddMetaDataType);
            _uiManager.ApplySkinStyleToControl(BtnComboBoxMetaDataTypCancel);
        }

        /// <summary>
        /// Initializes the ComboBox with settings and data binding.
        /// </summary>
        private void InitializeComboBox()
        {
            ComboBoxTagSelect.BeginUpdate();
            try
            {
                // Configure ComboBox properties for better UX
                ComboBoxTagSelect.DropDownStyle = ComboBoxStyle.DropDown;
                ComboBoxTagSelect.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                ComboBoxTagSelect.AutoCompleteSource = AutoCompleteSource.ListItems;
                ComboBoxTagSelect.FormattingEnabled = true;

                // Set up data binding
                ComboBoxTagSelect.DataSource = _metaDataTypeItems;
                ComboBoxTagSelect.DisplayMember = "DisplayValue";
                ComboBoxTagSelect.ValueMember = "MetaDataType";
            }
            finally
            {
                ComboBoxTagSelect.EndUpdate();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event to enable/disable the Add button based on selection.
        /// </summary>
        private void ComboBoxTagSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            BtnComboBoxAddMetaDataType.Enabled = ComboBoxTagSelect.SelectedItem != null;
        }

        /// <summary>
        /// Handles the KeyDown event for the ComboBox to prevent submitting the form on Enter.
        /// </summary>
        private void ComboBoxTagSelect_KeyDown(object sender, KeyEventArgs e)
        {
            // Prevent Enter key from closing the dropdown and submitting the form
            if (e.KeyCode == Keys.Enter && ComboBoxTagSelect.DroppedDown)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Validates the ComboBox selection.
        /// </summary>
        private void ComboBoxTagSelect_Validating(object sender, CancelEventArgs e)
        {
            if (ComboBoxTagSelect.SelectedItem == null)
            {
                MessageBox.Show(
                    GetValidationMessage(),
                    GetValidationTitle(),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                e.Cancel = true; // Prevents the focus from leaving the ComboBox if the validation fails
            }
        }

        /// <summary>
        /// Gets the validation message for the ComboBox.
        /// </summary>
        private string GetValidationMessage()
        {
            // Default message if Messages class is not available
            return "Please select a valid tag from the dropdown list.";
        }

        /// <summary>
        /// Gets the validation title for the ComboBox.
        /// </summary>
        private string GetValidationTitle()
        {
            // Default title if Messages class is not available
            return "Invalid Selection";
        }

        /// <summary>
        /// Gets available metadata types excluding those that are blacklisted or already in use.
        /// </summary>
        /// <param name="usedTags">Collection of tags that are already in use.</param>
        /// <returns>List of available metadata type items.</returns>
        private static List<MetaDataTypeItem> GetAvailableMetaDataTypes(IEnumerable<string> usedTags)
        {
            var allMetaDataTypes = Enum.GetValues(typeof(MetaDataType))
                .Cast<MetaDataType>()
                .Where(type => !_blacklistedMetaDataTypes.Contains(type));

            var usedMetaDataTypeEnums = usedTags
                .Select(tag => Enum.TryParse(tag, true, out MetaDataType type) ? (MetaDataType?)type : null)
                .Where(t => t.HasValue)
                .Select(t => t.Value);

            return allMetaDataTypes
                .Except(usedMetaDataTypeEnums)
                .Select(t => new MetaDataTypeItem
                {
                    MetaDataType = t,
                    DisplayValue = FormatDisplayValue(t)
                })
                .OrderBy(item => item.DisplayValue) // Sort alphabetically for better UX
                .ToList();
        }

        /// <summary>
        /// Formats the display value for a metadata type.
        /// </summary>
        /// <param name="type">The metadata type to format.</param>
        /// <returns>A formatted display string.</returns>
        private static string FormatDisplayValue(MetaDataType type)
        {
            // Convert enum to string and add spaces before capital letters for readability
            string name = type.ToString();
            return System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
        }

        /// <summary>
        /// Gets the selected metadata type as a string.
        /// </summary>
        /// <returns>The selected metadata type string or null if nothing is selected.</returns>
        public string GetSelectedMetaDataType()
        {
            if (ComboBoxTagSelect.SelectedValue is MetaDataType selectedMetaDataType)
            {
                return selectedMetaDataType.ToString();
            }
            return null;
        }

        /// <summary>
        /// Handles key down events for the form.
        /// </summary>
        private void TagListSelectorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !ComboBoxTagSelect.DroppedDown)
            {
                if (BtnComboBoxAddMetaDataType.Enabled)
                {
                    BtnComboBoxAddMetaDataType.PerformClick();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                BtnComboBoxMetaDataTypCancel.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        // The Dispose method is removed since it's already defined by a base class or partial class
    }
}