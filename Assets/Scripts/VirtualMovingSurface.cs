using UnityEngine;

public class VirtualMovingSurface : MovingSurface
{
    public Vector3 translation;

    public override Vector3 GetFinalFrameTranslation(Vector3 worldPosition)
    {
        return transform.TransformDirection(translation) * Time.fixedDeltaTime;
    }
    protected override void FixedUpdate()
    {
        //base.FixedUpdate();
    }
}
