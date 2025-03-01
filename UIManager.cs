using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    /// <summary>
    /// Manages UI-related operations for MusicBee plugin including theming, color management, and panel interactions.
    /// </summary>
    public class UIManager : IDisposable
    {
        private const SkinElement DefaultSkinElement = SkinElement.SkinTrackAndArtistPanel;
        private const ElementState DefaultElementState = ElementState.ElementStateDefault;

        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly ConcurrentDictionary<int, Color> _colorCache = new ConcurrentDictionary<int, Color>();
        private readonly object _colorCacheLock = new object();
        private readonly Dictionary<string, TagListPanel> _checklistBoxList;
        private readonly string[] _selectedFileUrls;
        private readonly Action<string[]> _refreshPanelTagsFromFiles;

        private Font _defaultFont;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIManager"/> class.
        /// </summary>
        /// <param name="mbApiInterface">The MusicBee API interface.</param>
        /// <param name="checklistBoxList">Dictionary of tag panels.</param>
        /// <param name="selectedFileUrls">Currently selected file URLs.</param>
        /// <param name="refreshPanelTagsFromFiles">Action to refresh panel tags from files.</param>
        /// <exception cref="ArgumentNullException">Thrown when essential parameters are null.</exception>
        public UIManager(
    MusicBeeApiInterface mbApiInterface,
    Dictionary<string, TagListPanel> checklistBoxList,
    string[] selectedFileUrls,
    Action<string[]> refreshPanelTagsFromFiles)
        {
            if (mbApiInterface.Equals(default(MusicBeeApiInterface))) throw new ArgumentNullException(nameof(mbApiInterface));
            _mbApiInterface = mbApiInterface;
            _checklistBoxList = checklistBoxList ?? throw new ArgumentNullException(nameof(checklistBoxList));
            _selectedFileUrls = selectedFileUrls;
            _refreshPanelTagsFromFiles = refreshPanelTagsFromFiles;
        }

        /// <summary>
        /// Displays a settings prompt label in the specified panel.
        /// </summary>
        /// <param name="panel">The panel to display the label on.</param>
        /// <param name="tabControl">The tab control to manage visibility.</param>
        /// <param name="message">The message text to display.</param>
        /// <exception cref="ArgumentNullException">Thrown when panel or tabControl is null.</exception>
        public void DisplaySettingsPromptLabel(Control panel, TabControl tabControl, string message)
        {
            if (panel == null) throw new ArgumentNullException(nameof(panel));
            if (tabControl == null) throw new ArgumentNullException(nameof(tabControl));
            if (string.IsNullOrEmpty(message)) return;

            if (panel.InvokeRequired)
            {
                panel.Invoke(new Action(() => DisplaySettingsPromptLabelCore(panel, tabControl, message)));
                return;
            }

            DisplaySettingsPromptLabelCore(panel, tabControl, message);
        }

        /// <summary>
        /// Core implementation for displaying a settings prompt label.
        /// </summary>
        private void DisplaySettingsPromptLabelCore(Control panel, TabControl tabControl, string message)
        {
            // Remove any existing prompt labels
            foreach (var existingLabel in panel.Controls.OfType<Label>().Where(l => l.Text == message).ToList())
            {
                panel.Controls.Remove(existingLabel);
                existingLabel.Dispose();
            }

            var emptyPanelText = new Label
            {
                AutoSize = true,
                Location = new Point(14, 30),
                Text = message,
                Font = _defaultFont ?? (_defaultFont = _mbApiInterface.Setting_GetDefaultFont())
            };

            panel.SuspendLayout();
            panel.Controls.Add(emptyPanelText);

            // Ensure correct z-order
            panel.Controls.SetChildIndex(emptyPanelText, 1);
            panel.Controls.SetChildIndex(tabControl, 0);

            tabControl.Visible = tabControl.TabPages.Count > 0;
            panel.ResumeLayout(true);
        }

        /// <summary>
        /// Adds tags to a checklist box panel.
        /// </summary>
        /// <param name="tagName">The name of the tag panel.</param>
        /// <param name="tags">Dictionary of tag names and their check states.</param>
        public void AddTagsToChecklistBoxPanel(string tagName, Dictionary<string, CheckState> tags)
        {
            if (string.IsNullOrEmpty(tagName) || tags == null)
                return;

            if (_checklistBoxList.TryGetValue(tagName, out var checklistBoxPanel) &&
                checklistBoxPanel?.IsDisposed == false)
            {
                if (checklistBoxPanel.InvokeRequired)
                {
                    checklistBoxPanel.Invoke(new Action(() => checklistBoxPanel.PopulateChecklistBoxesFromData(tags)));
                }
                else
                {
                    checklistBoxPanel.PopulateChecklistBoxesFromData(tags);
                }
            }
        }

        /// <summary>
        /// Switches the visible tag panel.
        /// </summary>
        /// <param name="visibleTag">The tag to make visible.</param>
        public void SwitchVisibleTagPanel(string visibleTag)
        {
            if (_checklistBoxList == null || string.IsNullOrEmpty(visibleTag))
            {
                return;
            }

            foreach (var pair in _checklistBoxList)
            {
                var checklistBoxPanel = pair.Value;
                if (checklistBoxPanel?.Tag != null)
                {
                    checklistBoxPanel.Visible = checklistBoxPanel.Tag.ToString() == visibleTag;
                }
            }

            _refreshPanelTagsFromFiles?.Invoke(_selectedFileUrls ?? Array.Empty<string>());
        }

        /// <summary>
        /// Creates a unique key from skin element parameters.
        /// </summary>
        private int GetKeyFromArgs(SkinElement skinElement, ElementState elementState, ElementComponent elementComponent)
        {
            return ((int)skinElement & 0x7F) << 24 | ((int)elementState & 0xFF) << 16 | (int)elementComponent & 0xFFFF;
        }

        /// <summary>
        /// Gets the color for a skin element with caching.
        /// </summary>
        /// <param name="skinElement">The skin element.</param>
        /// <param name="elementState">The element state.</param>
        /// <param name="elementComponent">The element component.</param>
        /// <returns>The color associated with the specified skin element.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the manager has been disposed.</exception>
        public Color GetElementColor(SkinElement skinElement, ElementState elementState, ElementComponent elementComponent)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(UIManager));

            var key = GetKeyFromArgs(skinElement, elementState, elementComponent);

            return _colorCache.GetOrAdd(key, k =>
            {
                lock (_colorCacheLock)
                {
                    int colorValue = _mbApiInterface.Setting_GetSkinElementColour(
                        skinElement, elementState, elementComponent);
                    return Color.FromArgb(colorValue);
                }
            });
        }

        /// <summary>
        /// Applies the MusicBee skin style to a control.
        /// </summary>
        /// <param name="formControl">The control to style.</param>
        /// <exception cref="ArgumentNullException">Thrown when formControl is null.</exception>
        public void ApplySkinStyleToControl(Control formControl)
        {
            if (formControl == null) throw new ArgumentNullException(nameof(formControl));
            if (_disposed) throw new ObjectDisposedException(nameof(UIManager));

            if (_defaultFont == null)
            {
                _defaultFont = _mbApiInterface.Setting_GetDefaultFont();
            }

            formControl.Font = _defaultFont;
            formControl.BackColor = GetElementColor(DefaultSkinElement, DefaultElementState, ElementComponent.ComponentBackground);
            formControl.ForeColor = GetElementColor(DefaultSkinElement, DefaultElementState, ElementComponent.ComponentForeground);
        }

        /// <summary>
        /// Disposes of the UIManager and its resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _defaultFont?.Dispose();
                _colorCache.Clear();
            }

            _disposed = true;
        }
    }
}