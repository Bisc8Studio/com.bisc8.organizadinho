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
    private const string ShortcutButtonClassName = "organizadinho-project-shortcut-button";
    private const string ShortcutPlaceholderClassName = "organizadinho-project-shortcut-placeholder";
    private const float ShortcutDragThreshold = 4f;

    private readonly Button _compactSearchButton;
    private readonly ToolbarSearchField _searchField;
    private readonly ScrollView _shortcutScroll;

    private EditorWindow _projectBrowser;
    private string _searchText = string.Empty;
    private bool _searchExpanded;
    private string _pressedShortcutGuid;
    private FolderShortcutData _pressedShortcut;
    private Vector2 _pressedShortcutPosition;
    private Button _pressedShortcutButton;
    private Button _draggedShortcutButton;
    private VisualElement _shortcutPlaceholder;
    private VisualElement _shortcutDragEventRoot;
    private float _shortcutDragGrabOffsetX;
    private float _shortcutDragTop;
    private bool _suppressShortcutClick;

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
        ApplyChrome();

        _compactSearchButton = new Button(ExpandSearch)
        {
            text = "Search"
        };
        _compactSearchButton.style.width = 86f;
        _compactSearchButton.style.minWidth = 86f;
        _compactSearchButton.style.maxWidth = 86f;
        _compactSearchButton.style.height = 18f;
        _compactSearchButton.style.marginRight = 6f;
        _compactSearchButton.style.overflow = Overflow.Hidden;

        _searchField = new ToolbarSearchField();
        _searchField.style.display = DisplayStyle.None;
        _searchField.style.width = 180f;
        _searchField.style.minWidth = 180f;
        _searchField.style.height = 18f;
        _searchField.style.marginRight = 6f;
        _searchField.RegisterValueChangedCallback(OnSearchChanged);
        _searchField.RegisterCallback<KeyDownEvent>(OnSearchKeyDown, TrickleDown.TrickleDown);
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
        ResetShortcutPressState();
    }

    private void OnSearchChanged(ChangeEvent<string> evt)
    {
        _searchText = evt.newValue ?? string.Empty;
        UpdateCompactSearchLabel();
        ProjectWindowIntegration.RequestSearchApply(_projectBrowser, _searchText);
    }

    private void OnSearchKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
        {
            return;
        }

        _searchText = _searchField.value ?? string.Empty;
        UpdateCompactSearchLabel();
        ProjectWindowIntegration.RequestSearchApply(_projectBrowser, _searchText);
        evt.StopPropagation();
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
            _compactSearchButton.tooltip = string.Empty;
            return;
        }

        const int maxLength = 12;
        const string suffix = "...";
        var value = _searchText.Length > maxLength
            ? _searchText.Substring(0, maxLength - suffix.Length) + suffix
            : _searchText;
        _compactSearchButton.text = value;
        _compactSearchButton.tooltip = _searchText;
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
        var button = new Button
        {
            tooltip = shortcut.AssetPath
        };
        button.clicked += () =>
        {
            if (_suppressShortcutClick)
            {
                _suppressShortcutClick = false;
                return;
            }

            ProjectWindowNavigator.OpenFolder(_projectBrowser, shortcut.AssetPath);
        };

        button.AddToClassList(ShortcutButtonClassName);
        button.userData = shortcut.Guid;

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
        button.RegisterCallback<MouseDownEvent>(evt => OnShortcutMouseDown(evt, button, shortcut), TrickleDown.TrickleDown);
        button.RegisterCallback<MouseMoveEvent>(evt => OnShortcutMouseMove(evt, button, shortcut), TrickleDown.TrickleDown);
        button.RegisterCallback<MouseUpEvent>(evt => OnShortcutMouseUp(evt, button), TrickleDown.TrickleDown);

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

    private void ApplyChrome()
    {
        style.backgroundColor = new Color(0.17f, 0.17f, 0.17f, 1f);
        style.borderBottomColor = new Color(0f, 0f, 0f, 0.35f);
        style.borderBottomWidth = 1f;
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

    private void OnShortcutMouseDown(MouseDownEvent evt, VisualElement button, FolderShortcutData shortcut)
    {
        if (evt.button != 0)
        {
            return;
        }

        if (_shortcutDragEventRoot != null)
        {
            UnregisterShortcutDragEvents();
        }

        _pressedShortcutGuid = shortcut.Guid;
        _pressedShortcut = shortcut;
        _pressedShortcutPosition = evt.mousePosition;
        _pressedShortcutButton = (Button)button;
        RegisterShortcutDragEvents(button);
    }

    private void OnShortcutMouseMove(MouseMoveEvent evt, VisualElement button, FolderShortcutData shortcut)
    {
        if (shortcut == null ||
            string.IsNullOrEmpty(_pressedShortcutGuid) ||
            !string.Equals(_pressedShortcutGuid, shortcut.Guid, System.StringComparison.Ordinal))
        {
            return;
        }

        if (_draggedShortcutButton != null)
        {
            UpdateShortcutDrag(evt.mousePosition);
            evt.StopPropagation();
            return;
        }

        if ((evt.mousePosition - _pressedShortcutPosition).sqrMagnitude < ShortcutDragThreshold * ShortcutDragThreshold)
        {
            return;
        }

        BeginShortcutDrag((Button)button, shortcut, evt.mousePosition);
        UpdateShortcutDrag(evt.mousePosition);
        _suppressShortcutClick = true;
        evt.StopPropagation();
    }

    private void OnShortcutMouseUp(MouseUpEvent evt, VisualElement button)
    {
        if (_pressedShortcutButton != null && button != _pressedShortcutButton)
        {
            return;
        }

        if (_draggedShortcutButton != null)
        {
            EndShortcutDrag();
            evt.StopPropagation();
        }

        ResetShortcutPressState();
    }

    private void OnShortcutGlobalMouseMove(MouseMoveEvent evt)
    {
        if (_pressedShortcutButton == null || string.IsNullOrEmpty(_pressedShortcutGuid))
        {
            return;
        }

        OnShortcutMouseMove(evt, _pressedShortcutButton, _pressedShortcut);
    }

    private void OnShortcutGlobalMouseUp(MouseUpEvent evt)
    {
        if (_pressedShortcutButton == null)
        {
            return;
        }

        OnShortcutMouseUp(evt, _pressedShortcutButton);
    }

    private void RegisterShortcutDragEvents(VisualElement button)
    {
        _shortcutDragEventRoot = button.panel?.visualTree;
        if (_shortcutDragEventRoot == null)
        {
            button.CaptureMouse();
            return;
        }

        _shortcutDragEventRoot.RegisterCallback<MouseMoveEvent>(OnShortcutGlobalMouseMove, TrickleDown.TrickleDown);
        _shortcutDragEventRoot.RegisterCallback<MouseUpEvent>(OnShortcutGlobalMouseUp, TrickleDown.TrickleDown);
        button.CaptureMouse();
    }

    private void UnregisterShortcutDragEvents()
    {
        if (_shortcutDragEventRoot == null)
        {
            return;
        }

        _shortcutDragEventRoot.UnregisterCallback<MouseMoveEvent>(OnShortcutGlobalMouseMove, TrickleDown.TrickleDown);
        _shortcutDragEventRoot.UnregisterCallback<MouseUpEvent>(OnShortcutGlobalMouseUp, TrickleDown.TrickleDown);
        _shortcutDragEventRoot = null;
    }

    private void ResetShortcutPressState()
    {
        if (_pressedShortcutButton != null && _pressedShortcutButton.HasMouseCapture())
        {
            _pressedShortcutButton.ReleaseMouse();
        }

        UnregisterShortcutDragEvents();
        _pressedShortcutGuid = string.Empty;
        _pressedShortcut = null;
        _pressedShortcutButton = null;
    }

    private void BeginShortcutDrag(Button button, FolderShortcutData shortcut, Vector2 panelPosition)
    {
        var content = _shortcutScroll.contentContainer;
        var sourceIndex = content.IndexOf(button);
        if (sourceIndex < 0)
        {
            return;
        }

        var buttonWorldBound = button.worldBound;
        var contentPosition = content.WorldToLocal(buttonWorldBound.position);

        _draggedShortcutButton = button;
        _shortcutDragGrabOffsetX = panelPosition.x - buttonWorldBound.x;
        _shortcutDragTop = contentPosition.y;

        _shortcutPlaceholder = new VisualElement();
        _shortcutPlaceholder.AddToClassList(ShortcutPlaceholderClassName);
        _shortcutPlaceholder.style.width = buttonWorldBound.width;
        _shortcutPlaceholder.style.height = buttonWorldBound.height;
        _shortcutPlaceholder.style.minWidth = buttonWorldBound.width;
        _shortcutPlaceholder.style.marginRight = 4f;
        _shortcutPlaceholder.style.borderBottomWidth = 1f;
        _shortcutPlaceholder.style.borderTopWidth = 1f;
        _shortcutPlaceholder.style.borderLeftWidth = 1f;
        _shortcutPlaceholder.style.borderRightWidth = 1f;
        _shortcutPlaceholder.style.borderBottomColor = new Color(1f, 1f, 1f, 0.28f);
        _shortcutPlaceholder.style.borderTopColor = new Color(1f, 1f, 1f, 0.28f);
        _shortcutPlaceholder.style.borderLeftColor = new Color(1f, 1f, 1f, 0.28f);
        _shortcutPlaceholder.style.borderRightColor = new Color(1f, 1f, 1f, 0.28f);
        _shortcutPlaceholder.style.borderTopLeftRadius = 5f;
        _shortcutPlaceholder.style.borderTopRightRadius = 5f;
        _shortcutPlaceholder.style.borderBottomLeftRadius = 5f;
        _shortcutPlaceholder.style.borderBottomRightRadius = 5f;
        _shortcutPlaceholder.style.backgroundColor = new Color(1f, 1f, 1f, 0.08f);

        content.Remove(button);
        content.Insert(sourceIndex, _shortcutPlaceholder);
        content.Add(button);

        button.style.position = Position.Absolute;
        button.style.width = buttonWorldBound.width;
        button.style.height = buttonWorldBound.height;
        button.style.minWidth = buttonWorldBound.width;
        button.style.left = contentPosition.x;
        button.style.top = _shortcutDragTop;
        button.style.marginRight = 0f;
        button.style.opacity = 0.92f;
        button.BringToFront();
    }

    private void UpdateShortcutDrag(Vector2 panelPosition)
    {
        var content = _shortcutScroll.contentContainer;
        if (_draggedShortcutButton == null || _shortcutPlaceholder == null)
        {
            return;
        }

        var localPosition = content.WorldToLocal(panelPosition);
        var width = _draggedShortcutButton.worldBound.width;
        var viewportMaxX = _shortcutScroll.worldBound.xMax - content.worldBound.x;
        var maxLeft = Mathf.Max(0f, viewportMaxX - width);
        var left = Mathf.Clamp(localPosition.x - _shortcutDragGrabOffsetX, 0f, maxLeft);
        _draggedShortcutButton.style.left = left;
        _draggedShortcutButton.style.top = _shortcutDragTop;

        var mouseX = localPosition.x;
        var placeholderIndex = GetPlaceholderTargetIndex(mouseX);
        if (placeholderIndex < 0)
        {
            return;
        }

        var currentIndex = content.IndexOf(_shortcutPlaceholder);
        if (currentIndex == placeholderIndex)
        {
            return;
        }

        content.Remove(_shortcutPlaceholder);
        if (placeholderIndex > currentIndex)
        {
            placeholderIndex--;
        }

        content.Insert(Mathf.Clamp(placeholderIndex, 0, content.childCount), _shortcutPlaceholder);
    }

    private int GetPlaceholderTargetIndex(float contentLocalMouseX)
    {
        var content = _shortcutScroll.contentContainer;

        for (var index = 0; index < content.childCount; index++)
        {
            var child = content[index];
            if (child == _draggedShortcutButton || child == _shortcutPlaceholder)
            {
                continue;
            }

            if (contentLocalMouseX < child.layout.center.x)
            {
                return index;
            }
        }

        var draggedIndex = _draggedShortcutButton == null
            ? content.childCount
            : content.IndexOf(_draggedShortcutButton);
        return draggedIndex < 0 ? content.childCount : draggedIndex;
    }

    private void EndShortcutDrag()
    {
        var button = _draggedShortcutButton;
        var placeholder = _shortcutPlaceholder;
        var guid = _pressedShortcutGuid;
        var content = _shortcutScroll.contentContainer;

        _draggedShortcutButton = null;
        _shortcutPlaceholder = null;

        if (button == null || placeholder == null)
        {
            _suppressShortcutClick = false;
            return;
        }

        var targetIndex = content.IndexOf(placeholder);

        content.Remove(button);
        content.Remove(placeholder);

        button.style.position = Position.Relative;
        button.style.left = StyleKeyword.Auto;
        button.style.top = StyleKeyword.Auto;
        button.style.width = StyleKeyword.Auto;
        button.style.minWidth = StyleKeyword.Auto;
        button.style.marginRight = 4f;
        button.style.opacity = 1f;

        if (!FolderShortcutStorage.MoveShortcut(guid, targetIndex))
        {
            RebuildShortcutButtons();
        }
    }
}
}
