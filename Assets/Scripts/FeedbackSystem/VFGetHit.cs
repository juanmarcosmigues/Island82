using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFGetHit : VisualFeedback
{
    public Vector3 addScale;
    public Gradient color;
    public AnimationCurve scaleCurve;

    [Header("Targets")]
    public Transform scaleTarget;
    public Renderer[] mesh;
    public string matPropertyTarget;
    public SpriteRenderer spriteRenderer;

    public override void Clear()
    {
        base.Clear();
        if (scaleTarget) scaleTarget.localScale = Vector3.one;
        if (mesh.Length > 0) mesh.ForEach(m => m.material.SetColor(matPropertyTarget, color.Evaluate(1)));
        if (spriteRenderer) spriteRenderer.color = color.Evaluate(1);
    }
    public override void PlayTime(float t)
    {
        base.PlayTime(t);

        if (scaleTarget) scaleTarget.localScale = Vector3.one + addScale * scaleCurve.Evaluate(t);
        if (mesh.Length > 0) mesh.ForEach(m => m.material.SetColor(matPropertyTarget, color.Evaluate(t)));
        if (spriteRenderer) spriteRenderer.color = color.Evaluate(t);
    }
    protected override IEnumerator PlayAnimation(float speedMultiplier)
    {
        float t = 0f;
        while (t != duration)
        {
            t = Mathf.Clamp(t + Time.deltaTime * speedMultiplier, 0f, duration);

            PlayTime(t / duration);

            yield return null;
        }
    }
}
