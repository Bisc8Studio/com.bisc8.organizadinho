using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Organizadinho.Editor.Storage
{
    [Serializable]
    public class FolderDesignEntry
    {
        public string guid = "";
        public bool hasColor;
        public Color color = new Color(0.23f, 0.43f, 0.65f, 1f);
        public bool propagateChildren;
        public string iconGuid = "";
    }

    [FilePath("ProjectSettings/Organizadinho/FolderDesignStorage.asset", FilePathAttribute.Location.ProjectFolder)]
    public class FolderDesignStorage : ScriptableSingleton<FolderDesignStorage>
    {
        [SerializeField] public List<FolderDesignEntry> entries = new List<FolderDesignEntry>();

        public static event Action Changed;

        public static FolderDesignStorage GetOrCreate()
        {
            if (instance.entries == null)
                instance.entries = new List<FolderDesignEntry>();

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

            entry = new FolderDesignEntry { guid = folderGuid };
            entries.Add(entry);
            return entry;
        }

        public void PruneEntry(string folderGuid)
        {
            var entry = GetEntry(folderGuid);
            if (entry != null && !entry.hasColor && string.IsNullOrEmpty(entry.iconGuid))
                entries.Remove(entry);
        }

        public void NotifyChanged(bool saveAssets = false)
        {
            EditorUtility.SetDirty(this);
            if (saveAssets)
                Save(true);

            Changed?.Invoke();
            EditorApplication.RepaintProjectWindow();
        }
    }
}
