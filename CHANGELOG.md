# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.3] - Unreleased

### Changed

- Rebranded the package from `Organizadinho` (Bisc8) to `Unity Organizer` (Erased).
  - Package id changed from `com.bisc8.organizadinho` to `com.erased.unityorganizer`.
  - Root namespace changed from `Organizadinho` to `UnityOrganizer`.
  - Settings menu moved from `Organizadinho/Settings` to `Tools/Unity Organizer/Settings`.
  - `ProjectSettings/Organizadinho/FolderDesignStorage.asset` moved to `ProjectSettings/UnityOrganizer/FolderDesignStorage.asset`.
  - `EditorPrefs` key prefix changed from `com.bisc8.organizadinho.` to `com.erased.unityorganizer.`.

> **Upgrade note:** if you were using the previous `Organizadinho` package in a project, your folder colors
> (`ProjectSettings/Organizadinho/FolderDesignStorage.asset`) and local preferences (`EditorPrefs`) will not be
> picked up automatically after upgrading to `Unity Organizer`. Hierarchy styling stored on the `HierarchyDesign`
> component in scenes/prefabs still works, since the component's serialized fields are compatible.

## [1.3.3] - Previous history (as Organizadinho)

- Hierarchy organizer rows with custom colors, fonts, icons, and child propagation.
- Project window folder styling with color inheritance, badge icons, and empty-folder-aware rendering.
- Project shortcuts toolbar with drag-and-drop folder shortcuts and integrated search.
- Shared color controls for Hierarchy and FolderDesign, including copy/paste between systems.
- Base, white, and black color modes for folder and hierarchy styling.
- Improved color slider drag behavior that keeps updating while the mouse is held down, even outside the slider bounds.

