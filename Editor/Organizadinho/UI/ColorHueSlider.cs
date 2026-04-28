using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.Utilities;
using Organizadinho.Runtime;

namespace Organizadinho.Editor.UI
{
    internal static class ColorHueSlider
    {
        private static Texture2D _gradientTexture;
        private static int _activeSliderControl;

        internal static float DrawHueSlider(string label, float currentHue, string previewLabel)
        {
            return DrawColorSlider(label, OrganizadinhoColorMode.Base, currentHue, previewLabel, out _).Hue;
        }

        internal static float DrawHueSlider(string label, float currentHue, string previewLabel, out bool pasted)
        {
            return DrawColorSlider(label, OrganizadinhoColorMode.Base, currentHue, previewLabel, out pasted).Hue;
        }

        internal static OrganizadinhoColorSelection DrawColorSlider(
            string label,
            OrganizadinhoColorMode currentMode,
            float currentHue,
            string previewLabel)
        {
            return DrawColorSlider(label, currentMode, currentHue, previewLabel, out _);
        }

        internal static OrganizadinhoColorSelection DrawColorSlider(
            string label,
            OrganizadinhoColorMode currentMode,
            float currentHue,
            string previewLabel,
            out bool pasted)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            pasted = false;

            var selection = new OrganizadinhoColorSelection(currentMode, currentHue);
            var sliderRect = GUILayoutUtility.GetRect(1f, 18f, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(sliderRect, GetGradientTexture(), ScaleMode.StretchToFill);
                EditorGUI.DrawRect(new Rect(sliderRect.x, sliderRect.y, sliderRect.width, 1f), new Color(0f, 0f, 0f, 0.3f));
                EditorGUI.DrawRect(new Rect(sliderRect.x, sliderRect.yMax - 1f, sliderRect.width, 1f), new Color(0f, 0f, 0f, 0.4f));
            }

            selection = HandleSliderInput(sliderRect, selection);
            selection = DrawSpecialColorControls(selection);
            selection = DrawClipboardControls(selection, out pasted);
            DrawHandle(sliderRect, selection);

            var previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(18f));
            DrawPreview(previewRect, selection, previewLabel);

            return selection;
        }

        private static OrganizadinhoColorSelection DrawSpecialColorControls(OrganizadinhoColorSelection selection)
        {
            EditorGUILayout.BeginHorizontal();
            selection = DrawColorModeButton("Base", OrganizadinhoColorMode.Base, ColorPaletteUtility.FromHue(selection.Hue), selection);
            selection = DrawColorModeButton("White", OrganizadinhoColorMode.White, ColorPaletteUtility.GetBaseColor(OrganizadinhoColorMode.White, selection.Hue), selection);
            selection = DrawColorModeButton("Black", OrganizadinhoColorMode.Black, ColorPaletteUtility.GetBaseColor(OrganizadinhoColorMode.Black, selection.Hue), selection);
            EditorGUILayout.EndHorizontal();

            return selection;
        }

        private static OrganizadinhoColorSelection DrawColorModeButton(
            string label,
            OrganizadinhoColorMode mode,
            Color swatchColor,
            OrganizadinhoColorSelection selection)
        {
            var previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = selection.Mode == mode
                ? ColorPaletteUtility.BuildPalette(mode, selection.Hue).SelectedColor
                : previousBackground;

            var content = new GUIContent("  " + label);
            var rect = GUILayoutUtility.GetRect(content, GUI.skin.button, GUILayout.Height(20f));
            if (GUI.Button(rect, content))
                selection = new OrganizadinhoColorSelection(mode, selection.Hue);

            var swatchRect = new Rect(rect.x + 6f, rect.y + 4f, 12f, rect.height - 8f);
            EditorGUI.DrawRect(swatchRect, swatchColor);
            EditorGUI.DrawRect(new Rect(swatchRect.x, swatchRect.yMax - 1f, swatchRect.width, 1f),
                ColorPaletteUtility.BuildPalette(mode, selection.Hue).BorderColor);

            GUI.backgroundColor = previousBackground;
            return selection;
        }

        private static OrganizadinhoColorSelection DrawClipboardControls(OrganizadinhoColorSelection selection, out bool pasted)
        {
            pasted = false;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Color", GUILayout.Height(20f)))
            {
                OrganizadinhoColorClipboard.CopyColor(selection);
            }

            EditorGUI.BeginDisabledGroup(!OrganizadinhoColorClipboard.HasColor);
            if (GUILayout.Button("Paste Color", GUILayout.Height(20f)) &&
                OrganizadinhoColorClipboard.TryGetColor(out var copiedColor))
            {
                selection = copiedColor;
                pasted = true;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            return selection;
        }

        private static OrganizadinhoColorSelection HandleSliderInput(Rect sliderRect, OrganizadinhoColorSelection selection)
        {
            var currentEvent = Event.current;
            if (currentEvent == null)
            {
                return selection;
            }

            int controlId = GUIUtility.GetControlID(FocusType.Passive, sliderRect);
            bool isActive = GUIUtility.hotControl == controlId && _activeSliderControl == controlId;

            switch (currentEvent.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (currentEvent.button == 0 && sliderRect.Contains(currentEvent.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        _activeSliderControl = controlId;
                        currentEvent.Use();
                        GUI.changed = true;
                        RepaintActiveWindow();
                        return SelectionFromMouse(sliderRect, currentEvent.mousePosition);
                    }
                    break;

                case EventType.MouseDrag:
                    if (currentEvent.button == 0 && isActive)
                    {
                        currentEvent.Use();
                        GUI.changed = true;
                        RepaintActiveWindow();
                        return SelectionFromMouse(sliderRect, currentEvent.mousePosition);
                    }
                    break;

                case EventType.MouseUp:
                    if (isActive)
                    {
                        ReleaseSliderControl(controlId);
                        currentEvent.Use();
                        RepaintActiveWindow();
                    }
                    break;

                case EventType.KeyDown:
                    if (isActive && currentEvent.keyCode == KeyCode.Escape)
                    {
                        ReleaseSliderControl(controlId);
                        currentEvent.Use();
                    }
                    break;

                case EventType.Ignore:
                    if (isActive)
                        ReleaseSliderControl(controlId);
                    break;
            }

            return selection;
        }

        private static OrganizadinhoColorSelection SelectionFromMouse(Rect sliderRect, Vector2 mousePosition)
        {
            return OrganizadinhoColorSelection.Base(
                Mathf.Clamp01((mousePosition.x - sliderRect.x) / sliderRect.width));
        }

        private static void ReleaseSliderControl(int controlId)
        {
            if (GUIUtility.hotControl == controlId)
                GUIUtility.hotControl = 0;

            if (_activeSliderControl == controlId)
                _activeSliderControl = 0;
        }

        private static void RepaintActiveWindow()
        {
            EditorWindow.focusedWindow?.Repaint();
            if (!ReferenceEquals(EditorWindow.mouseOverWindow, EditorWindow.focusedWindow))
                EditorWindow.mouseOverWindow?.Repaint();
        }

        private static void DrawHandle(Rect sliderRect, OrganizadinhoColorSelection selection)
        {
            var x = Mathf.Lerp(sliderRect.x, sliderRect.xMax, selection.Hue);
            var handleRect = new Rect(x - 4f, sliderRect.y - 2f, 8f, sliderRect.height + 4f);
            EditorGUI.DrawRect(handleRect, new Color(0f, 0f, 0f, 0.55f));
            EditorGUI.DrawRect(
                new Rect(handleRect.x + 1f, handleRect.y + 1f, handleRect.width - 2f, handleRect.height - 2f),
                ColorPaletteUtility.GetReadableTextColor(ColorPaletteUtility.GetBaseColor(selection.Mode, selection.Hue)));
        }

        private static void DrawPreview(Rect previewRect, OrganizadinhoColorSelection selection, string previewLabel)
        {
            var palette = ColorPaletteUtility.BuildPalette(selection.Mode, selection.Hue);
            var swatchRect = new Rect(previewRect.x, previewRect.y + 2f, 26f, previewRect.height - 4f);
            EditorGUI.DrawRect(swatchRect, palette.BaseColor);
            EditorGUI.DrawRect(new Rect(swatchRect.x, swatchRect.yMax - 1f, swatchRect.width, 1f), palette.BorderColor);

            var labelRect = new Rect(swatchRect.xMax + 6f, previewRect.y, previewRect.width - swatchRect.width - 6f, previewRect.height);
            EditorGUI.LabelField(
                labelRect,
                string.IsNullOrEmpty(previewLabel) ? "Color preview" : previewLabel,
                EditorStyles.miniLabel);
        }

        private static Texture2D GetGradientTexture()
        {
            if (_gradientTexture != null)
            {
                return _gradientTexture;
            }

            const int width = 256;
            _gradientTexture = new Texture2D(width, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            for (var index = 0; index < width; index++)
            {
                var hue = index / (float)(width - 1);
                _gradientTexture.SetPixel(index, 0, ColorPaletteUtility.FromHue(hue));
            }

            _gradientTexture.Apply();
            return _gradientTexture;
        }
    }
}
