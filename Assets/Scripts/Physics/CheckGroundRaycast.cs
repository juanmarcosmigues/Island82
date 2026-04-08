using UnityEngine;

public class CheckGroundRaycast : CheckGround
{
    [Range(0.01f, 0.5f)]
    public float checkOffset = 0.1f;

    protected RaycastHit hit;

    protected override void Awake()
    {
        base.Awake();
    }
    public override bool Check (Vector3 velocity = default)
    {
        bool grounded = false;

        float velocityExtension = Mathf.Max(0f, -velocity.y) * Time.fixedDeltaTime;

        //Start Check ->
#if UNITY_EDITOR
        if (debug)
            Debug.DrawLine(coll.bounds.center,
            coll.bounds.center + Vector3.down * (coll.bounds.extents.y + checkOffset));
#endif
        if (Physics.Raycast(
        coll.bounds.center,
                    Vector3.down,
                    out hit,
                    coll.bounds.extents.y + checkOffset,
                    groundMask
                    ))
        {

            grounded = Vector3.Angle(hit.normal, Vector3.up) <= maxGroundAngle;
        }
        //<- End Check

        if (grounded)
        {
#if UNITY_EDITOR
            if (debug)
            {
                DebugDraw.Box(hit.point, Vector3.one * 0.1f, Quaternion.identity, Color.darkRed, 1f);
            }
#endif
            groundData.point = hit.point;
            groundData.normal = hit.normal;
            groundData.coll = hit.collider;
        }

        if (this.grounded != grounded)
        {
            if (grounded) OnGroundedStart?.Invoke(groundData);
            else OnAirborneStart?.Invoke(groundData);
        }

        this.grounded = grounded;
        return this.grounded;
    }

}
