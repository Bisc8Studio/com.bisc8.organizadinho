# Unity Organizer

**Unity Organizer** (by Erased) is a Unity Editor extension focused on improving Project window and
Hierarchy navigation, folder styling, and overall visual organization of your projects.

## Requirements

- Unity **6000.3** or newer (Editor only feature set; no runtime dependency required in builds).
- Tested on Windows and macOS Editor.

> If you need support for older LTS versions (2021.3 / 2022.3), please contact us — the codebase does not use any
> Unity 6-exclusive API, so backporting the minimum supported version is possible after verification.

## Installation

1. Open `Window > Package Manager`.
2. Click `+`.
3. Choose `Add package from disk...` (or install via the Asset Store / a Git URL).
4. Select `Packages/com.erased.unityorganizer/package.json`.

## Main Features

- Hierarchy organizer rows with custom colors, fonts, icons, and child propagation.
- Project window folder styling with color inheritance, badge icons, and empty-folder-aware rendering.
- Project shortcuts toolbar with drag-and-drop folder shortcuts and integrated search.
- Shared color controls for Hierarchy and Folder styling, including copy/paste between systems.
- Pastel, vibrant, white, black, and custom color modes.
- Improved color slider drag behavior that keeps updating while the mouse is held down, even outside the slider bounds.
- Configurable popup shortcut (keyboard modifier + mouse button) via `Tools > Unity Organizer > Settings`.

## Folder Styling

- Use the configured keyboard and mouse shortcut, by default `Alt + Left Click`, on a folder in the Project window to open the Folder Design popup.
- Apply a pastel, vibrant, white, black, or custom color style.
- Enable propagation to apply the folder color to child folders.
- Empty folders preserve an empty-folder shape while still using the selected color.
- Badge icons can be selected from the bundled icon set shipped with the package.

## Hierarchy Styling

- Use the configured keyboard and mouse shortcut, by default `Alt + Left Click`, on a GameObject in the Hierarchy, or use the row dot, to open the Hierarchy organizer popup.
- Mark a GameObject as an organizer to draw a custom row in the Hierarchy.
- Customize color, font, font size, and icon.
- Enable child color propagation to visually group descendants.

## Color Copy/Paste

- Use `Copy Color` in either the Hierarchy or Folder Design color UI.
- Use `Paste Color` in the other system to apply the same color mode and base slider value.
- Copy/paste works for pastel, vibrant, white, black, and custom colors.

## Settings

Open `Tools > Unity Organizer > Settings` to:

- Toggle which color palettes are available in the color pickers.
- Choose the keyboard modifier and mouse button used to open the customization popups.
- Enable/disable folder colors, hierarchy colors, and the project shortcuts toolbar per editor user.
- Reset all local settings to their defaults.

## Version Control Behavior

- Hierarchy styling is shared through version control because it is stored on the `HierarchyDesign` component in scenes and prefabs.
- Folder styling is shared through version control because it is stored per project in `ProjectSettings/UnityOrganizer/FolderDesignStorage.asset`.
- Folder shortcuts are not shared through version control because they are stored locally in `EditorPrefs` for each user.

## Team Usage

- Commit scene and prefab changes when you edit hierarchy styling.
- Commit `ProjectSettings/UnityOrganizer/FolderDesignStorage.asset` when you edit folder colors or folder badge icons.
- Do not expect folder shortcuts to appear for other users automatically; each user configures their own shortcuts locally.

## Support

For bug reports, feature requests, or licensing questions, please reach out through the contact channel listed on the Asset Store product page.

## License

See [`LICENSE.md`](LICENSE.md). Distributed under the Unity Asset Store EULA.

## Changelog

See [`CHANGELOG.md`](CHANGELOG.md).


