using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Organizadinho.Runtime
{

[AddComponentMenu("Hierarchy Design/Hierarchy Design")]
[DisallowMultipleComponent]
public class HierarchyDesign : MonoBehaviour
{
    public enum FadeMode { LeftToRight, RightToLeft, CenterOut, CenterIn }

    [Header("Organizer")]
    public bool isOrganizer = false;
    public bool propagateToChildren = false;

    [Header("Background")]
    public Color backgroundColor = new Color(0.23f, 0.43f, 0.65f, 1f);

    [Header("Text")]
    public Color textColor = Color.white;
    public Font customFont = null;
    [Range(8, 20)]
    public int fontSize = 12;
    [HideInInspector] public FontStyle fontStyle = FontStyle.Normal;

    [Header("Arrow & Icon")]
    public Color arrowTint = Color.white;
    public Color iconTint = Color.white;
    public Texture2D customIcon = null;

#if UNITY_EDITOR
    private void OnValidate() => EditorApplication.RepaintHierarchyWindow();
#endif

    private void Awake()
    {
#if !UNITY_EDITOR
        Destroy(this);
#endif
    }
}
}
