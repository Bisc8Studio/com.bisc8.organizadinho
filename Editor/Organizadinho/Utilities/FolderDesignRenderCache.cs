using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.Storage;

namespace Organizadinho.Editor.Utilities
{
    internal readonly struct FolderDesignRenderData
    {
        internal FolderDesignRenderData(FolderDesignResolvedStyle style, Texture folderTexture, bool isEmptyFolder)
        {
            Style = style;
            FolderTexture = folderTexture;
            IsEmptyFolder = isEmptyFolder;
        }

        internal FolderDesignResolvedStyle Style { get; }
        internal Texture FolderTexture { get; }
        internal bool IsEmptyFolder { get; }
    }

    internal static class FolderDesignRenderCache
    {
        private static readonly Dictionary<string, FolderDesignRenderData> RenderDataCache =
            new Dictionary<string, FolderDesignRenderData>(StringComparer.Ordinal);
        private static bool _lastProSkin = EditorGUIUtility.isProSkin;

        static FolderDesignRenderCache()
        {
            FolderDesignStorage.Changed -= InvalidateStyles;
            FolderDesignStorage.Changed += InvalidateStyles;
            EditorApplication.projectChanged -= InvalidateProject;
            EditorApplication.projectChanged += InvalidateProject;
        }

        internal static FolderDesignRenderData Get(string guid, string path)
        {
            InvalidateIfSkinChanged();

            path = NormalizeAssetPath(path);
            var key = string.IsNullOrEmpty(guid) ? path : guid;

            if (RenderDataCache.TryGetValue(key, out var renderData))
                return renderData;

            var style = FolderDesignStyleResolver.Resolve(guid, path);
            var folderTexture = AssetDatabase.GetCachedIcon(path) ?? EditorGUIUtility.FindTexture("Folder Icon");
            var isEmptyFolder = style.HasResolvedColor && IsEmptyFolder(path);

            renderData = new FolderDesignRenderData(style, folderTexture, isEmptyFolder);
            RenderDataCache[key] = renderData;
            return renderData;
        }

        internal static void InvalidateProject()
        {
            _lastProSkin = EditorGUIUtility.isProSkin;
            RenderDataCache.Clear();
            FolderDesignStyleResolver.Invalidate();
        }

        internal static void InvalidateStyles()
        {
            _lastProSkin = EditorGUIUtility.isProSkin;
            RenderDataCache.Clear();
            FolderDesignStyleResolver.Invalidate();
        }

        private static void InvalidateIfSkinChanged()
        {
            if (_lastProSkin == EditorGUIUtility.isProSkin)
                return;

            InvalidateStyles();
        }

        private static bool IsEmptyFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return !FolderHasDirectContent(path);
        }

        private static bool FolderHasDirectContent(string path)
        {
            if (AssetDatabase.GetSubFolders(path).Length > 0)
                return true;

            var guids = AssetDatabase.FindAssets(string.Empty, new[] { path });
            for (var index = 0; index < guids.Length; index++)
            {
                var assetPath = NormalizeAssetPath(AssetDatabase.GUIDToAssetPath(guids[index]));
                if (string.IsNullOrEmpty(assetPath) || assetPath == path)
                    continue;

                if (NormalizeAssetPath(Path.GetDirectoryName(assetPath)) == path)
                    return true;
            }

            return false;
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? string.Empty
                : path.Replace('\\', '/').TrimEnd('/');
        }
    }
}
