using System;
using System.Collections.Generic;
using System.IO;
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

    [CreateAssetMenu(fileName = "FolderDesignStorage", menuName = "Hierarchy Design/Folder Design Storage")]
    public class FolderDesignStorage : ScriptableObject
    {
        private const string StoragePath = "Packages/com.bisc8.organizadinho/Editor/Organizadinho/Storage/FolderDesignStorage.asset";

        [SerializeField] public List<FolderDesignEntry> entries = new List<FolderDesignEntry>();

        private static FolderDesignStorage _instance;

        public static event Action Changed;

        public static FolderDesignStorage GetOrCreate()
        {
            if (_instance != null)
                return _instance;

            _instance = AssetDatabase.LoadAssetAtPath<FolderDesignStorage>(StoragePath);
            if (_instance != null)
                return _instance;

            _instance = CreateInstance<FolderDesignStorage>();
            Directory.CreateDirectory(Path.GetDirectoryName(StoragePath) ?? "Packages/com.bisc8.organizadinho/Editor/Organizadinho/Storage");
            AssetDatabase.CreateAsset(_instance, StoragePath);
            AssetDatabase.SaveAssets();
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
                AssetDatabase.SaveAssets();

            Changed?.Invoke();
            EditorApplication.RepaintProjectWindow();
        }
    }
}
