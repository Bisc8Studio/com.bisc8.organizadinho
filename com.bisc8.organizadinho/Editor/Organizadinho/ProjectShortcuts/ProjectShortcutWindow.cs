using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Organizadinho.Editor.ProjectShortcuts
{

internal sealed class ProjectShortcutWindow : EditorWindow
{
    [MenuItem("Window/Project Shortcuts/Toolbar Fallback")]
    private static void Open()
    {
        var window = GetWindow<ProjectShortcutWindow>();
        window.titleContent = new GUIContent("Project Shortcuts");
        window.minSize = new Vector2(320f, 60f);
    }

    private void CreateGUI()
    {
        rootVisualElement.Clear();
        rootVisualElement.style.flexDirection = FlexDirection.Column;

        var toolbar = new ProjectShortcutToolbar();
        rootVisualElement.Add(toolbar);

        var hint = new Label("Fallback window for shortcut management and search.");
        hint.style.paddingLeft = 8f;
        hint.style.paddingTop = 6f;
        hint.style.color = new Color(0.78f, 0.78f, 0.78f, 0.9f);
        rootVisualElement.Add(hint);
    }
}
}
