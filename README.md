# Organizadinho

Organizadinho is a Unity editor asset focused on improving Project and Hierarchy navigation, folder styling, and visual organization.

## Installation

1. Open `Window > Package Manager`.
2. Click `+`.
3. Choose `Add package from disk...`.
4. Select `Packages/com.bisc8.organizadinho/package.json`.

## Main Features

- Hierarchy organizer rows with custom colors, fonts, icons, and child propagation.
- Project window folder styling with color inheritance and badge icons.
- Project shortcuts toolbar with drag-and-drop folder shortcuts and integrated search.

## Version Control Behavior

- Hierarchy styling is shared through version control because it is stored on the `HierarchyDesign` component in scenes and prefabs.
- Folder styling is shared through version control because it is stored per project in `ProjectSettings/Organizadinho/FolderDesignStorage.asset`.
- Folder shortcuts are not shared through version control because they are stored locally in `EditorPrefs` for each user.

## Team Usage

- Commit scene and prefab changes when you edit hierarchy styling.
- Commit `ProjectSettings/Organizadinho/FolderDesignStorage.asset` when you edit folder colors or folder badge icons.
- Do not expect folder shortcuts to appear for other users automatically; each user configures their own shortcuts locally.
