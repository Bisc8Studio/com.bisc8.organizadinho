using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityOrganizer.Editor.ProjectShortcuts
{

internal static class FolderShortcutStorage
{
    private const string StorageKey = "ProjectShortcutTools.FolderShortcuts";

    private static readonly List<FolderShortcutData> Shortcuts = new List<FolderShortcutData>();

    private static bool _loaded;

    internal static event Action Changed;

    internal static IReadOnlyList<FolderShortcutData> GetShortcuts()
    {
        EnsureLoaded();
        return Shortcuts;
    }

    internal static bool AddFolder(string assetPath)
    {
        EnsureLoaded();

        if (!AssetDatabase.IsValidFolder(assetPath))
        {
            return false;
        }

        for (var index = 0; index < Shortcuts.Count; index++)
        {
            if (Shortcuts[index].MatchesPath(assetPath))
            {
                return false;
            }
        }

        Shortcuts.Add(new FolderShortcutData(assetPath));
        Save();
        Changed?.Invoke();
        return true;
    }

    internal static bool RemoveShortcut(string guid)
    {
        EnsureLoaded();

        if (string.IsNullOrEmpty(guid))
        {
            return false;
        }

        for (var index = Shortcuts.Count - 1; index >= 0; index--)
        {
            if (!string.Equals(Shortcuts[index].Guid, guid, StringComparison.Ordinal))
            {
                continue;
            }

            Shortcuts.RemoveAt(index);
            Save();
            Changed?.Invoke();
            return true;
        }

        return false;
    }

    internal static bool MoveShortcut(string guid, int targetIndex)
    {
        EnsureLoaded();

        if (string.IsNullOrEmpty(guid))
        {
            return false;
        }

        var sourceIndex = -1;
        for (var index = 0; index < Shortcuts.Count; index++)
        {
            if (!string.Equals(Shortcuts[index].Guid, guid, StringComparison.Ordinal))
            {
                continue;
            }

            sourceIndex = index;
            break;
        }

        if (sourceIndex < 0)
        {
            return false;
        }

        if (sourceIndex == targetIndex)
        {
            return false;
        }

        var shortcut = Shortcuts[sourceIndex];
        Shortcuts.RemoveAt(sourceIndex);
        targetIndex = Mathf.Clamp(targetIndex, 0, Shortcuts.Count);
        Shortcuts.Insert(targetIndex, shortcut);
        Save();
        Changed?.Invoke();
        return true;
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        Shortcuts.Clear();

        var json = EditorPrefs.GetString(StorageKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            var container = JsonUtility.FromJson<ShortcutCollection>(json);
            if (container != null && container.Items != null)
            {
                Shortcuts.AddRange(container.Items);
            }
        }

        PruneAndRefresh();
    }

    private static void Save()
    {
        var container = new ShortcutCollection(Shortcuts);
        EditorPrefs.SetString(StorageKey, JsonUtility.ToJson(container));
    }

    private static void PruneAndRefresh()
    {
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = Shortcuts.Count - 1; index >= 0; index--)
        {
            var shortcut = Shortcuts[index];
            shortcut.Refresh();

            if (!shortcut.IsValid() || !seenPaths.Add(shortcut.AssetPath))
            {
                Shortcuts.RemoveAt(index);
            }
        }

        Save();
    }

    [Serializable]
    private sealed class ShortcutCollection
    {
        [SerializeField] private List<FolderShortcutData> _items = new List<FolderShortcutData>();

        internal List<FolderShortcutData> Items => _items;

        private ShortcutCollection()
        {
        }

        internal ShortcutCollection(List<FolderShortcutData> items)
        {
            _items = new List<FolderShortcutData>(items);
        }
    }
}
}
