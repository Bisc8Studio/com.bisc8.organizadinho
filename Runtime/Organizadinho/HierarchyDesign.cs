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
    private const int CurrentVersion = 2;
    private const float DefaultHue = 0.58f;
    private static readonly Color DefaultCustomColor = new Color(0.62f, 0.78f, 0.96f, 1f);

    public enum FadeMode { LeftToRight, RightToLeft, CenterOut, CenterIn }

    [Header("Organizer")]
    public bool isOrganizer = false;
    public bool propagateToChildren = false;
    [HideInInspector] public OrganizadinhoColorMode colorMode = OrganizadinhoColorMode.Pastel;
    [HideInInspector] public float colorHue = DefaultHue;
    [HideInInspector] public Color customColor = DefaultCustomColor;
    public Font customFont = null;
    [Range(8, 20)]
    public int fontSize = 12;
    [HideInInspector] public FontStyle fontStyle = FontStyle.Normal;
    public Texture2D customIcon = null;
    [SerializeField, HideInInspector] private int _dataVersion;

    public void EnsureColorData()
    {
        if (_dataVersion < 1)
        {
            colorMode = OrganizadinhoColorMode.Pastel;
            colorHue = DefaultHue;
            customColor = DefaultCustomColor;
            _dataVersion = 1;
        }

        if (_dataVersion < 2)
        {
            if (customColor.a <= 0f)
                customColor = DefaultCustomColor;

            _dataVersion = CurrentVersion;
        }

        if (!System.Enum.IsDefined(typeof(OrganizadinhoColorMode), colorMode))
            colorMode = OrganizadinhoColorMode.Pastel;

        colorHue = Mathf.Repeat(colorHue, 1f);
        customColor.a = 1f;
    }

#if UNITY_EDITOR
    public void SyncInspectorVisibility()
    {
        var isHidden = (hideFlags & HideFlags.HideInInspector) != 0;
        if (isHidden)
        {
            return;
        }

        hideFlags |= HideFlags.HideInInspector;
        EditorUtility.SetDirty(this);
    }

    private void OnValidate()
    {
        EnsureColorData();
        SyncInspectorVisibility();
        EditorApplication.RepaintHierarchyWindow();
    }
#endif

    private void Awake()
    {
#if !UNITY_EDITOR
        Destroy(this);
#endif
    }
}
}
