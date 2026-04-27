using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Organizadinho.Editor.Storage;
using Organizadinho.Editor.Utilities;

namespace Organizadinho.Editor.ProjectShortcuts
{

internal sealed class ProjectShortcutToolbar : VisualElement
{
    private readonly Button _compactSearchButton;
    private readonly ToolbarSearchField _searchField;
    private readonly ScrollView _shortcutScroll;

    private EditorWindow _projectBrowser;
    private string _searchText = string.Empty;
    private bool _searchExpanded;

    internal ProjectShortcutToolbar()
    {
        name = "ProjectShortcutToolbar";
        style.flexDirection = FlexDirection.Row;
        style.alignItems = Align.Center;
        style.flexGrow = 0f;
        style.height = 24f;
        style.minHeight = 24f;
        style.maxHeight = 24f;
        style.paddingLeft = 6f;
        style.paddingRight = 6f;
        style.paddingTop = 2f;
        style.paddingBottom = 2f;
        style.backgroundColor = new Color(0.17f, 0.17f, 0.17f, 1f);
        style.borderBottomColor = new Color(0f, 0f, 0f, 0.35f);
        style.borderBottomWidth = 1f;

        _compactSearchButton = new Button(ExpandSearch)
        {
            text = "Search"
        };
        _compactSearchButton.style.width = 86f;
        _compactSearchButton.style.minWidth = 86f;
        _compactSearchButton.style.height = 18f;
        _compactSearchButton.style.marginRight = 6f;

        _searchField = new ToolbarSearchField();
        _searchField.style.display = DisplayStyle.None;
        _searchField.style.width = 180f;
        _searchField.style.minWidth = 180f;
        _searchField.style.height = 18f;
        _searchField.style.marginRight = 6f;
        _searchField.RegisterValueChangedCallback(OnSearchChanged);
        _searchField.RegisterCallback<FocusOutEvent>(OnSearchFocusOut);

        _shortcutScroll = new ScrollView(ScrollViewMode.Horizontal);
        _shortcutScroll.style.flexGrow = 1f;
        _shortcutScroll.style.height = 18f;
        _shortcutScroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        _shortcutScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        _shortcutScroll.contentContainer.style.flexDirection = FlexDirection.Row;
        _shortcutScroll.contentContainer.style.alignItems = Align.Center;

        Add(_compactSearchButton);
        Add(_searchField);
        Add(_shortcutScroll);

        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
        RegisterCallback<DragPerformEvent>(OnDragPerform);
        RegisterCallback<DragLeaveEvent>(OnDragLeave);

        RebuildShortcutButtons();
    }

    internal void SetProjectBrowser(EditorWindow projectBrowser)
    {
        _projectBrowser = projectBrowser;
    }

    private void OnAttachToPanel(AttachToPanelEvent evt)
    {
        FolderShortcutStorage.Changed += RebuildShortcutButtons;
        FolderDesignStorage.Changed += RebuildShortcutButtons;
        EditorApplication.projectChanged += RebuildShortcutButtons;
    }

    private void OnDetachFromPanel(DetachFromPanelEvent evt)
    {
        FolderShortcutStorage.Changed -= RebuildShortcutButtons;
        FolderDesignStorage.Changed -= RebuildShortcutButtons;
        EditorApplication.projectChanged -= RebuildShortcutButtons;
    }

    private void OnSearchChanged(ChangeEvent<string> evt)
    {
        _searchText = evt.newValue ?? string.Empty;
        UpdateCompactSearchLabel();
        ProjectWindowIntegration.RequestSearchApply(_projectBrowser, _searchText);
    }

    private void OnSearchFocusOut(FocusOutEvent evt)
    {
        CollapseSearch();
    }

    private void ExpandSearch()
    {
        _searchExpanded = true;
        _compactSearchButton.style.display = DisplayStyle.None;
        _searchField.style.display = DisplayStyle.Flex;
        _searchField.SetValueWithoutNotify(_searchText);
        _searchField.Focus();
    }

    private void CollapseSearch()
    {
        if (!_searchExpanded)
        {
            return;
        }

        _searchExpanded = false;
        _searchField.style.display = DisplayStyle.None;
        _compactSearchButton.style.display = DisplayStyle.Flex;
        UpdateCompactSearchLabel();
    }

    private void UpdateCompactSearchLabel()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _compactSearchButton.text = "Search";
            return;
        }

        const int maxLength = 16;
        var value = _searchText.Length > maxLength
            ? _searchText.Substring(0, maxLength - 1) + "..."
            : _searchText;
        _compactSearchButton.text = value;
    }

    private void RebuildShortcutButtons()
    {
        _shortcutScroll.contentContainer.Clear();

        var shortcuts = FolderShortcutStorage.GetShortcuts();
        if (shortcuts.Count == 0)
        {
            var emptyLabel = new Label("Drag folders here");
            emptyLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            emptyLabel.style.color = new Color(0.78f, 0.78f, 0.78f, 0.8f);
            _shortcutScroll.contentContainer.Add(emptyLabel);
            return;
        }

        for (var index = 0; index < shortcuts.Count; index++)
        {
            var shortcut = shortcuts[index];
            var button = CreateShortcutButton(shortcut);
            _shortcutScroll.contentContainer.Add(button);
        }
    }

    private Button CreateShortcutButton(FolderShortcutData shortcut)
    {
        var style = FolderDesignStyleResolver.Resolve(shortcut.Guid, shortcut.AssetPath);
        var button = new Button(() => ProjectWindowNavigator.OpenFolder(shortcut.AssetPath))
        {
            tooltip = shortcut.AssetPath
        };

        button.style.height = 18f;
        button.style.minHeight = 18f;
        button.style.marginRight = 4f;
        button.style.paddingLeft = 8f;
        button.style.paddingRight = 8f;
        button.style.paddingTop = 0f;
        button.style.paddingBottom = 0f;
        button.style.borderTopLeftRadius = 5f;
        button.style.borderTopRightRadius = 5f;
        button.style.borderBottomLeftRadius = 5f;
        button.style.borderBottomRightRadius = 5f;
        button.style.flexDirection = FlexDirection.Row;
        button.style.alignItems = Align.Center;
        button.style.unityTextAlign = TextAnchor.MiddleCenter;
        ApplyShortcutStyle(button, shortcut, style);
        button.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Remove Shortcut", _ => FolderShortcutStorage.RemoveShortcut(shortcut.Guid));
        }));

        return button;
    }

    private static void ApplyShortcutStyle(Button button, FolderShortcutData shortcut, FolderDesignResolvedStyle style)
    {
        var backgroundColor = FolderDesignStyleResolver.GetShortcutBackgroundColor(style);
        var textColor = FolderDesignStyleResolver.GetReadableTextColor(backgroundColor);

        button.style.backgroundColor = backgroundColor;
        button.style.borderBottomColor = FolderDesignStyleResolver.GetShortcutBorderColor(style);
        button.style.borderTopColor = FolderDesignStyleResolver.GetShortcutBorderColor(style);
        button.style.borderLeftColor = FolderDesignStyleResolver.GetShortcutBorderColor(style);
        button.style.borderRightColor = FolderDesignStyleResolver.GetShortcutBorderColor(style);
        button.style.borderBottomWidth = 1f;
        button.style.borderTopWidth = 1f;
        button.style.borderLeftWidth = 1f;
        button.style.borderRightWidth = 1f;

        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.flexGrow = 1f;
        row.pickingMode = PickingMode.Ignore;

        var badgeIcon = FolderDesignStyleResolver.LoadBadgeIcon(style.IconGuid);
        if (badgeIcon != null)
        {
            var icon = new Image
            {
                image = badgeIcon,
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore
            };
            icon.style.width = 12f;
            icon.style.height = 12f;
            icon.style.minWidth = 12f;
            icon.style.marginRight = 4f;
            row.Add(icon);
        }

        var label = new Label(shortcut.DisplayName);
        label.style.color = textColor;
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.unityFontStyleAndWeight = style.IsColorInherited ? FontStyle.Normal : FontStyle.Bold;
        label.style.fontSize = 11f;
        label.style.flexGrow = 0f;
        label.pickingMode = PickingMode.Ignore;
        row.Add(label);

        button.Add(row);
    }

    private void OnDragUpdated(DragUpdatedEvent evt)
    {
        DragAndDrop.visualMode = HasValidDraggedFolder() ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
    }

    private void OnDragPerform(DragPerformEvent evt)
    {
        if (!HasValidDraggedFolder())
        {
            return;
        }

        DragAndDrop.AcceptDrag();

        var changed = false;
        var paths = DragAndDrop.paths;
        if (paths == null)
        {
            return;
        }

        for (var index = 0; index < paths.Length; index++)
        {
            var path = paths[index];
            if (!AssetDatabase.IsValidFolder(path))
            {
                continue;
            }

            changed |= FolderShortcutStorage.AddFolder(path);
        }

        if (changed)
        {
            RebuildShortcutButtons();
        }
    }

    private void OnDragLeave(DragLeaveEvent evt)
    {
        DragAndDrop.visualMode = DragAndDropVisualMode.None;
    }

    private static bool HasValidDraggedFolder()
    {
        var paths = DragAndDrop.paths;
        if (paths == null)
        {
            return false;
        }

        for (var index = 0; index < paths.Length; index++)
        {
            if (AssetDatabase.IsValidFolder(paths[index]))
            {
                return true;
            }
        }

        return false;
    }
}
}
