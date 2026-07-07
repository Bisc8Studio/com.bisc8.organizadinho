using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Organizadinho.Editor.Settings
{
    [InitializeOnLoad]
    internal static class UnityEditorTint
    {
        private const string OverlayName = "OrganizadinhoUnityEditorTint";
        private static double _nextRefreshTime;

        static UnityEditorTint()
        {
            OrganizadinhoUserPreferences.Changed += ApplyToOpenWindows;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.delayCall += ApplyToOpenWindows;
        }

        private static void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup < _nextRefreshTime)
                return;

            _nextRefreshTime = EditorApplication.timeSinceStartup + 1.0d;
            ApplyToOpenWindows();
        }

        internal static void ApplyToOpenWindows()
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (var index = 0; index < windows.Length; index++)
            {
                ApplyToWindow(windows[index]);
            }
        }

        private static void ApplyToWindow(EditorWindow window)
        {
            if (window == null || window.rootVisualElement == null)
                return;

            var root = window.rootVisualElement;
            var overlay = root.Q<VisualElement>(OverlayName);

            if (!OrganizadinhoUserPreferences.EnableUnityEditorTint)
            {
                overlay?.RemoveFromHierarchy();
                window.Repaint();
                return;
            }

            if (overlay == null)
            {
                overlay = new VisualElement
                {
                    name = OverlayName,
                    pickingMode = PickingMode.Ignore
                };

                overlay.style.position = Position.Absolute;
                overlay.style.left = 0f;
                overlay.style.right = 0f;
                overlay.style.top = 0f;
                overlay.style.bottom = 0f;
                root.Add(overlay);
            }

            var color = OrganizadinhoUserPreferences.UnityEditorTintColor;
            color.a = Mathf.Clamp01(OrganizadinhoUserPreferences.UnityEditorTintStrength);
            overlay.style.backgroundColor = color;
            overlay.BringToFront();
            window.Repaint();
        }
    }
}
