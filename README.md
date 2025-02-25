# MusicBee Tags Panel Plugin (Alpha-Stage, You have been warned!)

The MusicBee Tags Panel Plugin enhances the MusicBee music player by allowing users to manage their music collection's tags directly within the MusicBee UI. This plugin introduces a custom panel for a comprehensive overview and easy editing of tags.

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Tag Overview**: View all tags of your music collection in a dedicated panel.
- **Tag Editing**: Add, remove, and edit tags seamlessly.
- **Support for Various Tag Types**: Handles different metadata types, including custom tags.
- **Seamless Integration**: Designed to fit naturally into the MusicBee interface.
- **Customizable Tag Lists**: Add self-defined tags to specific metadata fields (e.g., Genre, Mood, Keywords) with support for multiple checkbox lists.
- **Import/Export Tags**: Easily manage your tags by importing and exporting them in CSV format.

## Requirements

- MusicBee 3.6 or later. (Refer to the [MusicBee Forum](https://getmusicbee.com/forum/index.php?topic=41468.0) for beta releases if needed.)
- .NET Framework 4.8 (typically included in Windows 10 or 11 updates)

## Installation

### From GitHub Releases

1. **Download the Plugin:**
   - Navigate to the [GitHub Releases page](#) and download the latest release package.
   
2. **Extract Files:**
   - Unzip the downloaded file.
   - Copy the extracted plugin folder or DLL file(s) into the MusicBee Plugins directory, which is typically found at:
     - `C:\Program Files (x86)\MusicBee\Plugins`
     - or `C:\Program Files\MusicBee\Plugins`
     
3. **Restart MusicBee:**
   - Close MusicBee and launch it again.
   - If the plugin does not appear automatically, open MusicBee's "Preferences," then go to the "Plugins" tab and manually enable the Tags Panel Plugin.

### From Source

1. **Clone the Repository:**
   - Run: `git clone https://github.com/yourusername/MusicBeeTagsPanelPlugin.git`
   
2. **Build the Plugin:**
   - Open the solution in Visual Studio.
   - Ensure the project targets .NET Framework 4.8.
   - Build the solution in Release mode.
   
3. **Deploy the Plugin:**
   - Copy the built DLL from the output directory to the MusicBee Plugins directory listed above.
   - Restart MusicBee and enable the plugin in the "Preferences" if it doesn’t automatically load.

## Usage

1. **Accessing the Tags Panel:**
   - Open MusicBee.
   - Drag the "Tags-Panel" from the "Arrange Panels" option (located in the top right corner) to your desired location within the MusicBee interface.

2. **Managing Tags:**
   - View the complete list of tags from your music collection within the panel.
   - **Editing Tags:**
     - Click on a tag to edit its content.
     - Use the provided options to add, remove, or modify tags.
  
3. **Import/Export Functionality:**
   - Use the CSV Import/Export feature to back up or update tags in bulk.
   - Follow on-screen instructions for selecting the CSV file and mapping tag fields accordingly.

4. **Customization:**
   - Customize the tag lists as per your needs by adding self-defined tags to fields like Genre, Mood, or Keywords.
   - Adjust checkbox lists and configuration directly in the settings panel.

## Configuration

- **Access Settings:**
  - Open the configuration by navigating to **Tools -> Tags-Panel Settings** in MusicBee or by clicking on the panel header and selecting "Settings."
- **Options Include:**
  - Managing tag lists.
  - Adding or removing plugin tab pages.
  - Setting up CSV import/export mappings.

## Contributing

Contributions are welcome! For bugs or feature suggestions, please open an issue or pull request on [GitHub](https://github.com/yourusername/MusicBeeTagsPanelPlugin).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
