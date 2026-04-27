using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.UI;
using Organizadinho.Runtime;

namespace Organizadinho.Editor.Core
{

[InitializeOnLoad]
public class HierarchyDesignEditor
{
    static HierarchyDesignEditor()
    {
        EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        Event currentEvent = Event.current;
        if (currentEvent == null)
            return;

        if (currentEvent.type == EventType.MouseDown && currentEvent.alt && currentEvent.button == 0)
        {
#pragma warning disable 0618
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#pragma warning restore 0618
            if (obj == null)
                return;

            Vector2 localMouse = currentEvent.mousePosition;
            if (!selectionRect.Contains(localMouse))
                return;

            var popup = new HierarchyDesignPopup(obj);
            Vector2 size = popup.GetWindowSize();
            Rect showRect = new Rect(
                localMouse.x - size.x * 0.5f,
                localMouse.y - size.y * 0.5f,
                size.x,
                size.y);

            PopupWindow.Show(showRect, popup);
            currentEvent.Use();
        }
    }
}
}
