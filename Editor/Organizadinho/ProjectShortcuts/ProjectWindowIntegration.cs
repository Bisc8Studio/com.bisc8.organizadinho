using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Organizadinho.Editor.ProjectShortcuts
{

[InitializeOnLoad]
internal static class ProjectWindowIntegration
{
    private const string ToolbarHostName = "ProjectShortcutToolbarHost";

    private static readonly Type ProjectBrowserType;
    private static readonly Type SearchableEditorWindowType;
    private static readonly Type SearchModeType;
    private static readonly MethodInfo ShowFolderContentsMethod;
    private static readonly MethodInfo SetFolderSelectionMethod;
    private static readonly MethodInfo SetTwoColumnsMethod;
    private static readonly MethodInfo SetSearchMethod;
    private static readonly MethodInfo SetSearchFilterMethod;
    private static readonly object SearchModeAll;

    private static double _nextScanTime;
    private static EditorWindow _pendingSearchBrowser;
    private static string _pendingSearchText;
    private static bool _searchApplyQueued;

    static ProjectWindowIntegration()
    {
        var editorAssembly = typeof(EditorWindow).Assembly;

        ProjectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");
        SearchableEditorWindowType = editorAssembly.GetType("UnityEditor.SearchableEditorWindow");
        SearchModeType = editorAssembly.GetType("UnityEditor.SearchableEditorWindow+SearchMode");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        ShowFolderContentsMethod = ProjectBrowserType?.GetMethods(flags)
            .FirstOrDefault(method =>
            {
                if (method.Name != "ShowFolderContents")
                {
                    return false;
                }

                var parameters = method.GetParameters();
                return parameters.Length == 2 && parameters[1].ParameterType == typeof(bool);
            });

        SetFolderSelectionMethod = ProjectBrowserType?.GetMethods(flags)
            .FirstOrDefault(method =>
            {
                if (method.Name != "SetFolderSelection")
                {
                    return false;
                }

                var parameters = method.GetParameters();
                return parameters.Length == 2 &&
                       parameters[0].ParameterType.IsArray &&
                       parameters[1].ParameterType == typeof(bool);
            });

        SetTwoColumnsMethod = ProjectBrowserType?.GetMethod(
            "SetTwoColumns",
            flags,
            null,
            Type.EmptyTypes,
            null);

        SetSearchMethod = ProjectBrowserType?.GetMethod(
            "SetSearch",
            flags,
            null,
            new[] { typeof(string) },
            null) ?? SearchableEditorWindowType?.GetMethod(
            "SetSearch",
            flags,
            null,
            new[] { typeof(string) },
            null);

        if (SearchModeType != null)
        {
            SetSearchFilterMethod = SearchableEditorWindowType?.GetMethod(
                "SetSearchFilter",
                flags,
                null,
                new[] { typeof(string), SearchModeType, typeof(bool) },
                null);

            var allName = Array.Find(Enum.GetNames(SearchModeType), name => string.Equals(name, "All", StringComparison.Ordinal));
            SearchModeAll = allName != null
                ? Enum.Parse(SearchModeType, allName)
                : Enum.GetValues(SearchModeType).GetValue(0);
        }

        EditorApplication.update += OnEditorUpdate;
    }

    internal static void OpenFolder(EditorWindow projectBrowser, UnityEngine.Object folder)
    {
        if (folder == null)
        {
            return;
        }

        if (projectBrowser != null)
        {
            OpenFolderInBrowser(projectBrowser, folder);
            return;
        }

        var browsers = GetProjectBrowsers();
        for (var index = 0; index < browsers.Length; index++)
        {
            OpenFolderInBrowser(browsers[index], folder);
        }
    }

    private static void OpenFolderInBrowser(EditorWindow browser, UnityEngine.Object folder)
    {
        if (browser == null)
        {
            return;
        }

        try
        {
            if (TrySetFolderSelection(browser, folder))
            {
                browser.Repaint();
                return;
            }

            EnsureTwoColumnMode(browser);
            if (ShowFolderContentsMethod != null)
            {
                var folderIdArgument = ConvertFolderIdentifier(
                    ShowFolderContentsMethod.GetParameters()[0].ParameterType,
                    folder.GetInstanceID());
                ShowFolderContentsMethod.Invoke(browser, new[] { folderIdArgument, (object)true });
                browser.Repaint();
            }
        }
        catch
        {
        }
    }

    private static bool TrySetFolderSelection(EditorWindow browser, UnityEngine.Object folder)
    {
        if (SetFolderSelectionMethod == null)
        {
            return false;
        }

        var parameters = SetFolderSelectionMethod.GetParameters();
        var folderArrayArgument = Array.CreateInstance(
            parameters[0].ParameterType.GetElementType(),
            1);
        folderArrayArgument.SetValue(
            ConvertFolderIdentifier(parameters[0].ParameterType.GetElementType(), folder.GetInstanceID()),
            0);

        SetFolderSelectionMethod.Invoke(browser, new object[] { folderArrayArgument, true });
        return true;
    }

    private static void EnsureTwoColumnMode(EditorWindow browser)
    {
        if (browser == null || IsTwoColumnMode(browser))
        {
            return;
        }

        if (SetTwoColumnsMethod != null)
        {
            SetTwoColumnsMethod.Invoke(browser, null);
            return;
        }

        var serializedObject = new SerializedObject(browser);
        var viewModeProperty = serializedObject.FindProperty("m_ViewMode");
        if (viewModeProperty == null || viewModeProperty.intValue == 1)
        {
            return;
        }

        viewModeProperty.intValue = 1;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static bool IsTwoColumnMode(EditorWindow browser)
    {
        if (browser == null)
        {
            return false;
        }

        var serializedObject = new SerializedObject(browser);
        var viewModeProperty = serializedObject.FindProperty("m_ViewMode");
        return viewModeProperty != null && viewModeProperty.intValue == 1;
    }

    private static object ConvertFolderIdentifier(Type targetType, int instanceId)
    {
        if (targetType == null)
        {
            return instanceId;
        }

        if (targetType == typeof(int))
        {
            return instanceId;
        }

        var implicitOperator = targetType.GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(int) },
            null);
        if (implicitOperator != null)
        {
            return implicitOperator.Invoke(null, new object[] { instanceId });
        }

        return Convert.ChangeType(instanceId, targetType);
    }

    internal static void ApplySearch(EditorWindow projectBrowser, string searchText)
    {
        if (SetSearchMethod == null && SetSearchFilterMethod == null)
        {
            return;
        }

        if (projectBrowser != null)
        {
            ApplySearchToBrowser(projectBrowser, searchText);
            return;
        }

        var browsers = GetProjectBrowsers();
        for (var index = 0; index < browsers.Length; index++)
        {
            ApplySearchToBrowser(browsers[index], searchText);
        }
    }

    internal static void RequestSearchApply(EditorWindow projectBrowser, string searchText)
    {
        if (SetSearchMethod == null && SetSearchFilterMethod == null)
        {
            return;
        }

        _pendingSearchBrowser = projectBrowser;
        _pendingSearchText = searchText ?? string.Empty;

        if (_searchApplyQueued)
        {
            return;
        }

        _searchApplyQueued = true;
        EditorApplication.delayCall += FlushPendingSearchApply;
    }

    private static void OnEditorUpdate()
    {
        if (ProjectBrowserType == null)
        {
            return;
        }

        if (EditorApplication.timeSinceStartup < _nextScanTime)
        {
            return;
        }

        _nextScanTime = EditorApplication.timeSinceStartup + 0.75d;

        var browsers = GetProjectBrowsers();
        for (var index = 0; index < browsers.Length; index++)
        {
            try
            {
                TryAttachToolbar(browsers[index]);
            }
            catch
            {
            }
        }
    }

    private static void TryAttachToolbar(EditorWindow browser)
    {
        if (browser == null)
        {
            return;
        }

        if (!IsActiveProjectBrowser(browser))
        {
            return;
        }

        var root = browser.rootVisualElement;
        if (root == null || root.panel == null)
        {
            return;
        }

        var existingHost = root.Q<VisualElement>(ToolbarHostName);
        if (existingHost != null)
        {
            existingHost.style.backgroundColor = new Color(0.17f, 0.17f, 0.17f, 1f);
            var existingToolbar = existingHost.Q<ProjectShortcutToolbar>();
            if (existingToolbar != null)
            {
                existingToolbar.SetProjectBrowser(browser);
            }

            return;
        }

        var host = new VisualElement
        {
            name = ToolbarHostName
        };
        host.style.height = 24f;
        host.style.minHeight = 24f;
        host.style.maxHeight = 24f;
        host.style.flexGrow = 0f;
        host.style.flexShrink = 0f;
        host.style.overflow = Overflow.Hidden;
        host.style.backgroundColor = new Color(0.17f, 0.17f, 0.17f, 1f);

        var toolbar = new ProjectShortcutToolbar();
        toolbar.SetProjectBrowser(browser);
        host.Add(toolbar);

        if (TryGetNativeHeader(root, out var nativeHeader) && nativeHeader.parent != null)
        {
            var parent = nativeHeader.parent;
            var insertIndex = parent.IndexOf(nativeHeader);
            parent.Insert(Mathf.Max(0, insertIndex), host);
            return;
        }

        root.Insert(0, host);
    }

    private static bool IsActiveProjectBrowser(EditorWindow browser)
    {
        return browser != null &&
               (ReferenceEquals(browser, EditorWindow.focusedWindow) ||
                ReferenceEquals(browser, EditorWindow.mouseOverWindow));
    }

    private static void FlushPendingSearchApply()
    {
        _searchApplyQueued = false;

        var browser = _pendingSearchBrowser;
        var searchText = _pendingSearchText;

        _pendingSearchBrowser = null;
        _pendingSearchText = string.Empty;

        if (browser == null)
        {
            ApplySearch(null, searchText);
            return;
        }

        ApplySearch(browser, searchText);
    }

    private static void ApplySearchToBrowser(EditorWindow browser, string searchText)
    {
        if (browser == null)
        {
            return;
        }

        try
        {
            if (SetSearchMethod != null)
            {
                SetSearchMethod.Invoke(browser, new object[] { searchText ?? string.Empty });
                browser.Repaint();
                return;
            }

            if (SetSearchFilterMethod != null && SearchModeAll != null)
            {
                SetSearchFilterMethod.Invoke(browser, new[] { searchText ?? string.Empty, SearchModeAll, (object)false });
                browser.Repaint();
            }
        }
        catch
        {
        }
    }

    private static EditorWindow[] GetProjectBrowsers()
    {
        if (ProjectBrowserType == null)
        {
            return Array.Empty<EditorWindow>();
        }

        var objects = Resources.FindObjectsOfTypeAll(ProjectBrowserType);
        var browsers = new EditorWindow[objects.Length];

        for (var index = 0; index < objects.Length; index++)
        {
            browsers[index] = objects[index] as EditorWindow;
        }

        return browsers;
    }

    private static bool TryGetNativeHeader(VisualElement root, out VisualElement header)
    {
        var searchElement = FindFirst(root, element => !IsManagedElement(element) && IsSearchLikeElement(element));
        if (searchElement == null)
        {
            header = null;
            return false;
        }

        VisualElement bestMatch = null;
        var current = searchElement.parent;

        while (current != null && current != root)
        {
            if (!IsManagedElement(current) &&
                CountElements(current, IsSearchLikeElement) > 0 &&
                CountElements(current, IsButtonLikeElement) > 0)
            {
                bestMatch = current;
            }

            current = current.parent;
        }

        header = bestMatch ?? searchElement.parent;
        return header != null;
    }

    private static VisualElement FindFirst(VisualElement root, Func<VisualElement, bool> predicate)
    {
        if (root == null)
        {
            return null;
        }

        if (predicate(root))
        {
            return root;
        }

        for (var index = 0; index < root.childCount; index++)
        {
            var match = FindFirst(root[index], predicate);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static int CountElements(VisualElement root, Func<VisualElement, bool> predicate)
    {
        if (root == null || IsManagedElement(root))
        {
            return 0;
        }

        var count = predicate(root) ? 1 : 0;

        for (var index = 0; index < root.childCount; index++)
        {
            count += CountElements(root[index], predicate);
        }

        return count;
    }

    private static bool IsManagedElement(VisualElement element)
    {
        return element != null &&
               (!string.IsNullOrEmpty(element.name) &&
                (string.Equals(element.name, ToolbarHostName, StringComparison.Ordinal) ||
                 string.Equals(element.name, "ProjectShortcutToolbar", StringComparison.Ordinal)));
    }

    private static bool IsSearchLikeElement(VisualElement element)
    {
        return element != null &&
               (element.GetType().Name.IndexOf("SearchField", StringComparison.OrdinalIgnoreCase) >= 0 ||
                HasToken(element, "search"));
    }

    private static bool IsButtonLikeElement(VisualElement element)
    {
        return element != null &&
               (element is Button ||
                element.GetType().Name.IndexOf("Button", StringComparison.OrdinalIgnoreCase) >= 0 ||
                HasToken(element, "button"));
    }

    private static bool HasToken(VisualElement element, string token)
    {
        if (element == null || string.IsNullOrEmpty(token))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(element.name) &&
            element.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (!string.IsNullOrEmpty(element.viewDataKey) &&
            element.viewDataKey.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        foreach (var className in element.GetClasses())
        {
            if (className.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
}
