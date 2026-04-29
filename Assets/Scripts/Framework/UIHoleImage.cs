using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteAlways]
public class UIHoleImage : MonoBehaviour
{
    [Range(0f, 1.5f)] public float radius = 0f;
    [Range(0f, 0.5f)] public float softness = 0.005f;
    public Vector2 center = new Vector2(0.5f, 0.5f);

    private Image _image;
    private Material _instance;

    private static readonly int RadiusID = Shader.PropertyToID("_HoleRadius");
    private static readonly int SoftnessID = Shader.PropertyToID("_HoleSoftness");
    private static readonly int CenterID = Shader.PropertyToID("_HoleCenter");
    private static readonly int AspectID = Shader.PropertyToID("_Aspect");

    private void OnEnable()
    {
        _image = GetComponent<Image>();
        EnsureInstance();
        Apply();
    }

    private void OnDisable()
    {
        if (_instance != null)
        {
            if (Application.isPlaying) Destroy(_instance);
            else DestroyImmediate(_instance);
            _instance = null;
        }
    }

    private void EnsureInstance()
    {
        if (_image == null || _image.material == null) return;
        if (_instance == null || _image.material != _instance)
        {
            _instance = new Material(_image.material);
            _image.material = _instance;
        }
    }

    private void LateUpdate() => Apply();
    private void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        EnsureInstance();
        Apply();
    }

    private void Apply()
    {
        if (_instance == null) return;
        var rect = ((RectTransform)transform).rect;
        float aspect = rect.height > 0f ? rect.width / rect.height : 1f;

        _instance.SetFloat(RadiusID, radius);
        _instance.SetFloat(SoftnessID, softness);
        _instance.SetVector(CenterID, new Vector4(center.x, center.y, 0f, 0f));
        _instance.SetFloat(AspectID, aspect);
    }
}