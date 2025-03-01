# MusicBee Tags Panel Plugin

The MusicBee Tags Panel Plugin enhances the MusicBee music player by providing advanced tag management capabilities directly within the MusicBee interface. This plugin introduces a custom panel that offers a comprehensive overview of your music tags and makes tag editing seamless and efficient.

![Version](https://img.shields.io/badge/version-1.0.509-blue.svg)
![Status](https://img.shields.io/badge/status-alpha-orange.svg)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-green.svg)

> **Note:** This plugin is currently in alpha stage - use at your own risk.

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Technical Details](#technical-details)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Tag Overview**: View all tags of your music collection in a dedicated panel
- **Tag Editing**: Add, remove, and edit tags with simple checkbox interactions
- **Multi-File Tag Management**: Apply tag changes to multiple selected files simultaneously
- **Customizable Tag Lists**: Define and organize tags for specific metadata fields (Genre, Mood, Keywords, etc.)
- **Multiple Tag Panels**: Support for multiple checkbox lists for different tag categories
- **Visual Integration**: Seamless integration with MusicBee's UI, including skin support
- **Optimized Performance**: Built with performance in mind, including double-buffering for smooth UI rendering
- **Cross-Panel Communication**: Tag changes in one panel can be reflected across the application
- **Sorted Tags**: Option to automatically sort tags alphabetically
- **Import/Export**: Easily manage your tags by importing and exporting them in CSV format

## Requirements

- MusicBee 3.6 or later - MusicBee 3.6 or later. (Refer to the [MusicBee Forum](https://getmusicbee.com/forum/index.php?topic=41468.0) for beta releases if needed.)

- .NET Framework 4.8 (included in Windows 10 and 11 updates)

## Installation

### From GitHub Releases

1. **Download the Plugin:**
   - Navigate to the [GitHub Releases page](https://github.com/kn9f/MusicBeeTagsPanelPlugin/releases) and download the latest release package.
   
2. **Extract Files:**
   - Unzip the downloaded file.
   - Copy the extracted plugin DLL file(s) into the MusicBee Plugins directory:
     - `C:\Program Files (x86)\MusicBee\Plugins` or
     - `C:\Program Files\MusicBee\Plugins`
     
3. **Restart MusicBee:**
   - Close and relaunch MusicBee.
   - If the plugin doesn't appear automatically, open MusicBee's "Preferences," navigate to the "Plugins" tab, and enable the Tags Panel Plugin.

### From Source

1. **Clone the Repository:**
   - Run: `git clone https://github.com/kn9f/MusicBeeTagsPanelPlugin.git`
   
2. **Build the Plugin:**
   - Open the solution in Visual Studio 2019 or later.
   - Ensure the project targets .NET Framework 4.8.
   - Build the solution in Release mode.
   
3. **Deploy the Plugin:**
   - Copy the built DLL from the output directory to the MusicBee Plugins directory.
   - Restart MusicBee and enable the plugin in the "Preferences" if necessary.

## Usage

1. **Adding the Tags Panel:**
   - Open MusicBee
   - Right-click on any panel header in MusicBee and select "Arrange Panels"
   - Drag the "Tags-Panel" to your desired location within the MusicBee interface

2. **Managing Tags:**
   - Select one or more tracks in your library
   - The Tags Panel will display all tags from the selected tracks
   - Check or uncheck tags to apply or remove them from all selected tracks
   - Tags that appear in some but not all selected tracks will show with an indeterminate state (grayed checkbox)

3. **Tag Operations:**
   - **Add Tag**: Check a tag to add it to all selected files
   - **Remove Tag**: Uncheck a tag to remove it from all selected files
   - **View Tag Distribution**: Tags present in all files appear checked, tags in some files appear indeterminate

4. **Using Multiple Tag Panels:**
   - Configure different panels for different metadata fields (Genre, Mood, etc.)
   - Switch between panels using the tab interface

## Configuration

- **Access Settings:**
  - Navigate to **Tools → Tags-Panel Settings** in MusicBee
  - Or click on the panel header and select "Settings"

- **Available Settings:**
  - **Tag Lists**: Manage which tags appear in each list
  - **Sorting**: Enable/disable automatic alphabetical sorting of tags
  - **Panel Configuration**: Add/remove panel tab pages for different metadata fields
  - **Visual Settings**: Configure display options for each tag panel

## Technical Details

The plugin is built using C# 7.3 and .NET Framework 4.8. Key components include:

- **TagManager**: Core class for reading and writing tag data to files
- **TagListPanel**: UI control that displays tag checkboxes with optimized rendering
- **SettingsManager**: Handles persistence of user preferences
- **UIManager**: Ensures consistent visual appearance with MusicBee's skin

The plugin uses the MusicBee API interface to interact with the player and implements double buffering for smooth UI performance.

## Contributing

Contributions are welcome! Please consider:

1. **Reporting Issues**: Report bugs or suggest features through GitHub issues
2. **Pull Requests**: Submit improvements or bug fixes via pull requests
3. **Documentation**: Help improve documentation or add examples

Before submitting code, please ensure it follows the existing code style.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

Copyright © 2020-2024 kn9ff
