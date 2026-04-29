using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[AddComponentMenu("UI/Inverse Mask Image")]
public class InverseMaskImage : Image
{
    private Material _inverseMaskMaterial;

    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
        var stencilDepth = MaskUtilities.GetStencilDepth(transform, rootCanvas);

        if (stencilDepth <= 0)
            return baseMaterial; // not inside any Mask, draw normally

        var stencilId = (1 << stencilDepth) - 1;

        var newMat = StencilMaterial.Add(
            baseMaterial,
            stencilId,
            StencilOp.Keep,
            CompareFunction.NotEqual, // <-- the whole point
            ColorWriteMask.All,
            stencilId,
            0);

        StencilMaterial.Remove(_inverseMaskMaterial);
        _inverseMaskMaterial = newMat;
        return _inverseMaskMaterial;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StencilMaterial.Remove(_inverseMaskMaterial);
        _inverseMaskMaterial = null;
    }
}