using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.Utilities;

namespace Organizadinho.Editor.Storage
{
    [Serializable]
    public class FolderDesignEntry
    {
        public string guid = "";
        public bool hasColor;
        public bool propagateChildren;
        public float hue = PastelColorUtility.DefaultHue;
        public string iconGuid = "";
    }

    [FilePath(ProjectSettingsAssetPath, FilePathAttribute.Location.ProjectFolder)]
    public class FolderDesignStorage : ScriptableSingleton<FolderDesignStorage>
    {
        private const string ProjectSettingsAssetPath = "ProjectSettings/Organizadinho/FolderDesignStorage.asset";
        private const int CurrentVersion = 2;

        [SerializeField] public List<FolderDesignEntry> entries = new List<FolderDesignEntry>();
        [SerializeField] private int _storageVersion;

        public static event Action Changed;

        private static bool _migrationChecked;

        public static FolderDesignStorage GetOrCreate()
        {
            if (instance.entries == null)
                instance.entries = new List<FolderDesignEntry>();

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
                hue = PastelColorUtility.DefaultHue
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

                entries = new List<FolderDesignEntry>(legacyStorage.entries.Count);
                for (var entryIndex = 0; entryIndex < legacyStorage.entries.Count; entryIndex++)
                {
                    var legacyEntry = legacyStorage.entries[entryIndex];
                    if (legacyEntry == null)
                    {
                        continue;
                    }

                    entries.Add(new FolderDesignEntry
                    {
                        guid = legacyEntry.guid,
                        hasColor = legacyEntry.hasColor,
                        propagateChildren = legacyEntry.propagateChildren,
                        hue = legacyEntry.hue,
                        iconGuid = legacyEntry.iconGuid
                    });
                }

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

            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null)
                {
                    continue;
                }

                entry.hue = PastelColorUtility.DefaultHue;
            }

            _storageVersion = CurrentVersion;
            SaveToProjectSettings();
        }

        private void NormalizeEntryHues()
        {
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null)
                {
                    continue;
                }

                entry.hue = PastelColorUtility.NormalizeHue(entry.hue);
            }
        }
    }
}
