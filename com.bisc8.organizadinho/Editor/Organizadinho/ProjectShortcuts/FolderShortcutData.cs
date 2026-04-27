using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Organizadinho.Editor.ProjectShortcuts
{

[Serializable]
internal sealed class FolderShortcutData
{
    [SerializeField] private string _guid;
    [SerializeField] private string _assetPath;
    [SerializeField] private string _displayName;

    internal string Guid => _guid;
    internal string AssetPath => _assetPath;
    internal string DisplayName => _displayName;

    private FolderShortcutData()
    {
    }

    internal FolderShortcutData(string assetPath)
    {
        _assetPath = NormalizePath(assetPath);
        _guid = AssetDatabase.AssetPathToGUID(_assetPath);
        _displayName = GetDisplayName(_assetPath);
    }

    internal bool IsValid()
    {
        return !string.IsNullOrEmpty(_assetPath) && AssetDatabase.IsValidFolder(_assetPath);
    }

    internal bool MatchesPath(string assetPath)
    {
        return string.Equals(_assetPath, NormalizePath(assetPath), StringComparison.OrdinalIgnoreCase);
    }

    internal void Refresh()
    {
        if (!string.IsNullOrEmpty(_guid))
        {
            var guidPath = NormalizePath(AssetDatabase.GUIDToAssetPath(_guid));
            if (AssetDatabase.IsValidFolder(guidPath))
            {
                _assetPath = guidPath;
            }
        }

        if (string.IsNullOrEmpty(_guid) && !string.IsNullOrEmpty(_assetPath))
        {
            _guid = AssetDatabase.AssetPathToGUID(_assetPath);
        }

        _displayName = GetDisplayName(_assetPath);
    }

    private static string NormalizePath(string assetPath)
    {
        return string.IsNullOrWhiteSpace(assetPath)
            ? string.Empty
            : assetPath.Replace('\\', '/').Trim();
    }

    private static string GetDisplayName(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return string.Empty;
        }

        return Path.GetFileName(assetPath.TrimEnd('/'));
    }
}
}
