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
    private const int CurrentVersion = 1;
    private const float DefaultHue = 0.58f;

    public enum FadeMode { LeftToRight, RightToLeft, CenterOut, CenterIn }

    [Header("Organizer")]
    public bool isOrganizer = false;
    public bool propagateToChildren = false;
    [HideInInspector] public float colorHue = DefaultHue;
    public Font customFont = null;
    [Range(8, 20)]
    public int fontSize = 12;
    [HideInInspector] public FontStyle fontStyle = FontStyle.Normal;
    public Texture2D customIcon = null;
    [SerializeField, HideInInspector] private int _dataVersion;

    public void EnsurePastelData()
    {
        if (_dataVersion < CurrentVersion)
        {
            colorHue = DefaultHue;
            _dataVersion = CurrentVersion;
            return;
        }

        colorHue = Mathf.Repeat(colorHue, 1f);
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
        EnsurePastelData();
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
