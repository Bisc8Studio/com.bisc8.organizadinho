using UnityEditor;
using UnityEngine;

namespace Organizadinho.Editor.ProjectShortcuts
{

internal static class ProjectWindowNavigator
{
    internal static void OpenFolder(string assetPath)
    {
        if (!AssetDatabase.IsValidFolder(assetPath))
        {
            return;
        }

        var folder = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (folder == null)
        {
            return;
        }

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = folder;
        EditorGUIUtility.PingObject(folder);
        ProjectWindowIntegration.RevealFolder(folder);
    }
}
}
