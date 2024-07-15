using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class TagListSelectorForm : Form
    {
        // Blacklisted metadata types that are not allowed to be selected.
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

        public TagListSelectorForm(IEnumerable<string> usedTags)
        {
            InitializeComponent();
            BtnComboBoxAddMetaDataType.DialogResult = DialogResult.OK;
            BtnComboBoxMetaDataTypCancel.DialogResult = DialogResult.Cancel;
            ComboBoxTagSelect.DataSource = GetAvailableMetaDataTypes(usedTags);
        }

        private static IEnumerable<string> GetAvailableMetaDataTypes(IEnumerable<string> usedTags)
        {
            return Enum.GetValues(typeof(MetaDataType))
                .Cast<MetaDataType>()
                .Except(_blacklistedMetaDataTypes)
                .Select(dataType => dataType.ToString("g"))
                .Except(usedTags)
                .OrderBy(dataType => dataType)
                .ToList();
        }

        public string GetSelectedMetaDataType()
        {
            return ComboBoxTagSelect.SelectedItem?.ToString();
        }
    }
}
