# Organizadinho

Organizadinho is a Unity editor asset focused on improving Project and Hierarchy navigation, folder styling, and visual organization.

## Version

Current package version: `1.2.0`

## Installation

1. Open `Window > Package Manager`.
2. Click `+`.
3. Choose `Add package from disk...`.
4. Select `Packages/com.bisc8.organizadinho/package.json`.

## Main Features

- Hierarchy organizer rows with custom colors, fonts, icons, and child propagation.
- Project window folder styling with color inheritance, badge icons, and empty-folder-aware rendering.
- Project shortcuts toolbar with drag-and-drop folder shortcuts and integrated search.
- Shared color controls for Hierarchy and FolderDesign, including copy/paste between systems.
- Base, white, and black color modes for folder and hierarchy styling.
- Improved color slider drag behavior that keeps updating while the mouse is held down, even outside the slider bounds.

## Folder Styling

- Use `Alt + Click` on a folder in the Project window to open the FolderDesign popup.
- Apply a base color, white, or black style.
- Enable propagation to apply the folder color to child folders.
- Empty folders preserve an empty-folder shape while still using the selected color.
- Badge icons can be selected from `Packages/com.bisc8.organizadinho/Editor Default Resources/Organizadinho/Icons`.

## Hierarchy Styling

- Use `Alt + Click` on a GameObject in the Hierarchy, or use the row dot, to open the Hierarchy organizer popup.
- Mark a GameObject as an organizer to draw a custom row in the Hierarchy.
- Customize color, font, font size, and icon.
- Enable child color propagation to visually group descendants.

## Color Copy/Paste

- Use `Copy Color` in either the Hierarchy or FolderDesign color UI.
- Use `Paste Color` in the other system to apply the same color mode and base slider value.
- Copy/paste works for base colors, white, and black.

## Version Control Behavior

- Hierarchy styling is shared through version control because it is stored on the `HierarchyDesign` component in scenes and prefabs.
- Folder styling is shared through version control because it is stored per project in `ProjectSettings/Organizadinho/FolderDesignStorage.asset`.
- Folder shortcuts are not shared through version control because they are stored locally in `EditorPrefs` for each user.

## Team Usage

- Commit scene and prefab changes when you edit hierarchy styling.
- Commit `ProjectSettings/Organizadinho/FolderDesignStorage.asset` when you edit folder colors or folder badge icons.
- Do not expect folder shortcuts to appear for other users automatically; each user configures their own shortcuts locally.
