using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.Utilities;

namespace Organizadinho.Editor.UI
{
    internal static class PastelColorSlider
    {
        private static Texture2D _gradientTexture;

        internal static float DrawHueSlider(string label, float currentHue, string previewLabel)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            var hue = PastelColorUtility.NormalizeHue(currentHue);
            var sliderRect = GUILayoutUtility.GetRect(1f, 18f, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(sliderRect, GetGradientTexture(), ScaleMode.StretchToFill);
                EditorGUI.DrawRect(new Rect(sliderRect.x, sliderRect.y, sliderRect.width, 1f), new Color(0f, 0f, 0f, 0.3f));
                EditorGUI.DrawRect(new Rect(sliderRect.x, sliderRect.yMax - 1f, sliderRect.width, 1f), new Color(0f, 0f, 0f, 0.4f));
            }

            hue = HandleSliderInput(sliderRect, hue);
            DrawHandle(sliderRect, hue);

            var previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(18f));
            DrawPreview(previewRect, hue, previewLabel);

            return hue;
        }

        private static float HandleSliderInput(Rect sliderRect, float currentHue)
        {
            var currentEvent = Event.current;
            if (currentEvent == null || currentEvent.button != 0)
            {
                return currentHue;
            }

            if ((currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) &&
                sliderRect.Contains(currentEvent.mousePosition))
            {
                currentEvent.Use();
                return Mathf.Clamp01((currentEvent.mousePosition.x - sliderRect.x) / sliderRect.width);
            }

            return currentHue;
        }

        private static void DrawHandle(Rect sliderRect, float hue)
        {
            var x = Mathf.Lerp(sliderRect.x, sliderRect.xMax, hue);
            var handleRect = new Rect(x - 4f, sliderRect.y - 2f, 8f, sliderRect.height + 4f);
            EditorGUI.DrawRect(handleRect, new Color(0f, 0f, 0f, 0.55f));
            EditorGUI.DrawRect(
                new Rect(handleRect.x + 1f, handleRect.y + 1f, handleRect.width - 2f, handleRect.height - 2f),
                PastelColorUtility.GetReadableTextColor(PastelColorUtility.FromHue(hue)));
        }

        private static void DrawPreview(Rect previewRect, float hue, string previewLabel)
        {
            var palette = PastelColorUtility.BuildPalette(hue);
            var swatchRect = new Rect(previewRect.x, previewRect.y + 2f, 26f, previewRect.height - 4f);
            EditorGUI.DrawRect(swatchRect, palette.BaseColor);
            EditorGUI.DrawRect(new Rect(swatchRect.x, swatchRect.yMax - 1f, swatchRect.width, 1f), palette.BorderColor);

            var labelRect = new Rect(swatchRect.xMax + 6f, previewRect.y, previewRect.width - swatchRect.width - 6f, previewRect.height);
            EditorGUI.LabelField(labelRect, string.IsNullOrEmpty(previewLabel) ? "Pastel preview" : previewLabel, EditorStyles.miniLabel);
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
                _gradientTexture.SetPixel(index, 0, PastelColorUtility.FromHue(hue));
            }

            _gradientTexture.Apply();
            return _gradientTexture;
        }
    }
}
