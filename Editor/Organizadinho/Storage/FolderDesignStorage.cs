using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Organizadinho.Editor.Utilities;
using Organizadinho.Runtime;

namespace Organizadinho.Editor.Storage
{
    [Serializable]
    public class FolderDesignEntry
    {
        public string guid = "";
        public bool hasColor;
        public bool propagateChildren;
        public OrganizadinhoColorMode colorMode = OrganizadinhoColorMode.Base;
        public float hue = ColorPaletteUtility.DefaultHue;
        public string iconGuid = "";
    }

    [FilePath(ProjectSettingsAssetPath, FilePathAttribute.Location.ProjectFolder)]
    public class FolderDesignStorage : ScriptableSingleton<FolderDesignStorage>
    {
        private const string ProjectSettingsAssetPath = "ProjectSettings/Organizadinho/FolderDesignStorage.asset";
        private const int CurrentVersion = 3;

        [SerializeField] public List<FolderDesignEntry> entries = new List<FolderDesignEntry>();
        [SerializeField] private int _storageVersion;

        public static event Action Changed;

        private static bool _migrationChecked;

        public static FolderDesignStorage GetOrCreate()
        {
            if (instance.entries == null)
                instance.entries = new List<FolderDesignEntry>();

            instance.RestoreProjectSettingsEntriesIfNeeded();
            instance.MigrateLegacyEntriesIfNeeded();
            instance.EnsureEntryVersion();
            return instance;
        }

        public FolderDesignEntry GetEntry(string folderGuid)
        {
            return entries.Find(entry => entry.guid == folderGuid);
        }

        public FolderDesignEntry GetOrCreateEntry(string folderGuid)
        {
            var entry = GetEntry(folderGuid);
            if (entry != null)
                return entry;

            entry = new FolderDesignEntry
            {
                guid = folderGuid,
                colorMode = OrganizadinhoColorMode.Base,
                hue = ColorPaletteUtility.DefaultHue
            };
            entries.Add(entry);
            return entry;
        }

        public void PruneEntry(string folderGuid)
        {
            var entry = GetEntry(folderGuid);
            if (entry != null && !entry.hasColor && string.IsNullOrEmpty(entry.iconGuid))
                entries.Remove(entry);
        }

        public void NotifyChanged(bool saveAssets = true)
        {
            EditorUtility.SetDirty(this);
            if (saveAssets)
                SaveToProjectSettings();

            Changed?.Invoke();
            EditorApplication.RepaintProjectWindow();
        }

        private void SaveToProjectSettings()
        {
            EnsureProjectSettingsDirectoryExists();
            Save(true);
        }

        private static void EnsureProjectSettingsDirectoryExists()
        {
            var directoryPath = Path.GetDirectoryName(ProjectSettingsAssetPath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void RestoreProjectSettingsEntriesIfNeeded()
        {
            if (entries.Count > 0 || !File.Exists(ProjectSettingsAssetPath))
            {
                return;
            }

            var loadedObjects = InternalEditorUtility.LoadSerializedFileAndForget(ProjectSettingsAssetPath);
            for (var index = 0; index < loadedObjects.Length; index++)
            {
                var loadedStorage = loadedObjects[index] as FolderDesignStorage;
                if (loadedStorage == null || loadedStorage == this ||
                    loadedStorage.entries == null || loadedStorage.entries.Count == 0)
                {
                    continue;
                }

                entries = CloneEntries(loadedStorage.entries);
                _storageVersion = loadedStorage._storageVersion;
                break;
            }
        }

        private void MigrateLegacyEntriesIfNeeded()
        {
            if (_migrationChecked)
            {
                return;
            }

            _migrationChecked = true;

            if (entries.Count > 0 && File.Exists(ProjectSettingsAssetPath))
            {
                return;
            }

            var guids = AssetDatabase.FindAssets("t:FolderDesignStorage");
            for (var index = 0; index < guids.Length; index++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                if (string.IsNullOrEmpty(path) ||
                    path.StartsWith("ProjectSettings/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var legacyStorage = AssetDatabase.LoadAssetAtPath<FolderDesignStorage>(path);
                if (legacyStorage == null || legacyStorage == this || legacyStorage.entries == null || legacyStorage.entries.Count == 0)
                {
                    continue;
                }

                entries = CloneEntries(legacyStorage.entries);

                SaveToProjectSettings();
                Changed?.Invoke();
                EditorApplication.RepaintProjectWindow();
                break;
            }
        }

        private void EnsureEntryVersion()
        {
            if (_storageVersion >= CurrentVersion)
            {
                NormalizeEntryHues();
                return;
            }

            if (entries == null)
            {
                entries = new List<FolderDesignEntry>();
            }

            NormalizeEntryColors();

            _storageVersion = CurrentVersion;
            SaveToProjectSettings();
        }

        private void NormalizeEntryHues()
        {
            NormalizeEntryColors();
        }

        private void NormalizeEntryColors()
        {
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null)
                {
                    continue;
                }

                if (!Enum.IsDefined(typeof(OrganizadinhoColorMode), entry.colorMode))
                    entry.colorMode = OrganizadinhoColorMode.Base;

                entry.hue = ColorPaletteUtility.NormalizeHue(entry.hue);
            }
        }

        private static List<FolderDesignEntry> CloneEntries(List<FolderDesignEntry> source)
        {
            var clonedEntries = new List<FolderDesignEntry>(source.Count);
            for (var index = 0; index < source.Count; index++)
            {
                var sourceEntry = source[index];
                if (sourceEntry == null)
                {
                    continue;
                }

                clonedEntries.Add(new FolderDesignEntry
                {
                    guid = sourceEntry.guid,
                    hasColor = sourceEntry.hasColor,
                    propagateChildren = sourceEntry.propagateChildren,
                    colorMode = sourceEntry.colorMode,
                    hue = sourceEntry.hue,
                    iconGuid = sourceEntry.iconGuid
                });
            }

            return clonedEntries;
        }
    }
}
