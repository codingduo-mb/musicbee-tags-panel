using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class TabPageSelectorForm : Form
    {
        // Blacklist certain metadata types that are not allowed to be added as a new tab
        private static readonly HashSet<MetaDataType> blacklist = new HashSet<MetaDataType> {
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

        private List<string> metaDataTypes;

        public TabPageSelectorForm(List<string> usedTags)
        {
            InitializeComponent();
            Btn_ComboBoxAddTag.DialogResult = DialogResult.OK;
            Btn_ComboBoxCancel.DialogResult = DialogResult.Cancel;
            metaDataTypes = GetMetaDataTypesAsString(usedTags);
            SetMetaDataTypes();
        }

        private void SetMetaDataTypes()
        {
            comboBoxTagSelect.DataSource = metaDataTypes;
        }

        private List<string> GetMetaDataTypesAsString(List<string> usedTags)
        {
            List<string> dataTypesAsString = Enum.GetValues(typeof(MetaDataType))
                .Cast<MetaDataType>()
                .Where(dataType => !blacklist.Contains(dataType) && !usedTags.Contains(dataType.ToString("g")))
                .Select(dataType => dataType.ToString("g"))
                .ToList();

            dataTypesAsString.Sort();
            return dataTypesAsString;
        }

        public string GetMetaDataType()
        {
            return comboBoxTagSelect.SelectedItem as string;
        }
    }
}
