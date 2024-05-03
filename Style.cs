using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class Style
    {
        private readonly MusicBeeApiInterface mbApiInterface;
        private readonly Dictionary<(SkinElement, ElementComponent), Color> colorCache;

        public Style(MusicBeeApiInterface mbApiInterface)
        {
            this.mbApiInterface = mbApiInterface;
            this.colorCache = new Dictionary<(SkinElement, ElementComponent), Color>();
        }

        public Color GetElementColor(SkinElement skinElement, ElementState elementState, ElementComponent elementComponent)
        {
            if (!colorCache.TryGetValue((skinElement, elementComponent), out var color))
            {
                int colorValue = this.mbApiInterface.Setting_GetSkinElementColour(skinElement, elementState, elementComponent);
                color = Color.FromArgb(colorValue);
                colorCache[(skinElement, elementComponent)] = color;
            }

            return color;
        }

        public void StyleControl(Control formControl)
        {
            // apply current skin colors to tag panel
            var defaultFont = this.mbApiInterface.Setting_GetDefaultFont();
            var trackAndArtistPanelColor = this.GetElementColor(SkinElement.SkinTrackAndArtistPanel, ElementState.ElementStateDefault, ElementComponent.ComponentBackground);
            var inputControlColor = this.GetElementColor(SkinElement.SkinInputControl, ElementState.ElementStateDefault, ElementComponent.ComponentForeground);

            formControl.Font = defaultFont;
            formControl.BackColor = trackAndArtistPanelColor;
            formControl.ForeColor = inputControlColor;
        }

    }
}
