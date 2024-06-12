using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class Style
    {
        private const SkinElement DefaultSkinElement = SkinElement.SkinTrackAndArtistPanel;
        private const ElementState DefaultElementState = ElementState.ElementStateDefault;
        private readonly MusicBeeApiInterface _mbApiInterface;
        private Dictionary<int, Color> _colorCache;
        private Font _defaultFont;

        public Style(MusicBeeApiInterface mbApiInterface)
        {
            _mbApiInterface = mbApiInterface;
            _colorCache = new Dictionary<int, Color>();
        }

        // Replace Tuple with custom struct to improve performance and readability
        private struct KeyStruct
        {
            public SkinElement SkinElement { get; set; }
            public ElementComponent Component { get; set; }

            public KeyStruct(SkinElement skinElement, ElementComponent component)
            {
                SkinElement = skinElement;
                Component = component;
            }
        }

        private int GetKeyFromArgs(SkinElement skinElement, ElementState elementState, ElementComponent elementComponent)
        {
            return (int)(((int)skinElement & 0x7F) << 24) | (((int)elementState & 0xFF) << 16) | ((int)elementComponent & 0xFFFF);
        }

        public Color GetElementColor(SkinElement skinElement, ElementState elementState, ElementComponent elementComponent)
        {
            int key = GetKeyFromArgs(skinElement, elementState, elementComponent);

            if (!_colorCache.TryGetValue(key, out var color))
            {
                int colorValue = _mbApiInterface.Setting_GetSkinElementColour(skinElement, elementState, elementComponent);
                color = Color.FromArgb(colorValue);
                _colorCache[key] = color;
            }

            return color;
        }


        public void StyleControl(Control formControl)
        {
            if (_defaultFont == null)
                _defaultFont = _mbApiInterface.Setting_GetDefaultFont();

            formControl.Font = _defaultFont;
            formControl.BackColor = GetElementColor(DefaultSkinElement, DefaultElementState, ElementComponent.ComponentBackground);
            formControl.ForeColor = GetElementColor(DefaultSkinElement, DefaultElementState, ElementComponent.ComponentForeground);
        }
    }
}