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
        public OrganizadinhoColorMode colorMode = OrganizadinhoColorMode.Pastel;
        public float hue = ColorPaletteUtility.DefaultHue;
        public Color customColor = ColorPaletteUtility.GetBaseColor(OrganizadinhoColorMode.Pastel, ColorPaletteUtility.DefaultHue);
        public string iconGuid = "";
    }

    public class FolderDesignStorage : ScriptableObject
    {
        private const string ProjectSettingsAssetPath = "ProjectSettings/Organizadinho/FolderDesignStorage.asset";
        private const int CurrentVersion = 4;

        private static FolderDesignStorage _instance;

        [SerializeField] public List<FolderDesignEntry> entries = new List<FolderDesignEntry>();
        [SerializeField] private int _storageVersion;

        public static event Action Changed;

        public static FolderDesignStorage GetOrCreate()
        {
            var storage = _instance ?? LoadOrCreate();
            if (storage.entries == null)
                storage.entries = new List<FolderDesignEntry>();

            storage.EnsureEntryVersion();
            return storage;
        }

        private static FolderDesignStorage LoadOrCreate()
        {
            var loadedObjects = InternalEditorUtility.LoadSerializedFileAndForget(ProjectSettingsAssetPath);
            for (var index = 0; index < loadedObjects.Length; index++)
            {
                var loadedStorage = loadedObjects[index] as FolderDesignStorage;
                if (loadedStorage == null)
                    continue;

                _instance = loadedStorage;
                return _instance;
            }

            _instance = CreateInstance<FolderDesignStorage>();
            _instance.hideFlags = HideFlags.HideAndDontSave;
            return _instance;
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
                colorMode = OrganizadinhoColorMode.Pastel,
                hue = ColorPaletteUtility.DefaultHue,
                customColor = ColorPaletteUtility.GetBaseColor(OrganizadinhoColorMode.Pastel, ColorPaletteUtility.DefaultHue)
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
            InternalEditorUtility.SaveToSerializedFileAndForget(
                new UnityEngine.Object[] { this },
                ProjectSettingsAssetPath,
                true);
        }

        private static void EnsureProjectSettingsDirectoryExists()
        {
            var directoryPath = Path.GetDirectoryName(ProjectSettingsAssetPath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
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
                    entry.colorMode = OrganizadinhoColorMode.Pastel;

                entry.hue = ColorPaletteUtility.NormalizeHue(entry.hue);
                if (entry.customColor.a <= 0f)
                    entry.customColor = ColorPaletteUtility.GetBaseColor(OrganizadinhoColorMode.Pastel, ColorPaletteUtility.DefaultHue);

                entry.customColor.a = 1f;
            }
        }

    }
}
