using UnityEngine;
using Utils;
using static PhysicsDebugExtensions;
using static DebugDraw;

public class CheckGroundBox : CheckGround
{
    [Range(0.01f, 0.5f)]
    public float checkOffset = 0.1f;
    [Range(0.01f, 0.49f)]
    public float checkIndent = 0.4f;

    protected BoxCollider box;

    protected RaycastHit hit;
    protected Vector3 raycastLocalOrigin;

    protected override void Awake()
    {
        base.Awake();
        box = coll as BoxCollider;
    }
    public override bool Check (Vector3 velocity = default)
    {
        bool grounded = false;

        float velocityExtension = Mathf.Max(0f, -velocity.y) * Time.fixedDeltaTime;

        //Start Check ->
#if UNITY_EDITOR
        if (debug)
            DebugDraw.Box(box.bounds.center - Vector3.up * (checkOffset + velocityExtension),
            box.bounds.extents, Quaternion.identity, Color.beige);
#endif
        if (Physics.CheckBox(
            box.bounds.center - Vector3.up * (checkOffset + velocityExtension), 
            box.bounds.extents, 
            Quaternion.identity, groundMask))
        { //<---------------------------- CHECK BOX TO FIND POTENTIAL GROUND

            Vector3 boxCastScale = Vector3.Scale(box.size, transform.lossyScale);
            float boxCastDistance = box.bounds.size.y * (0.5f - checkIndent) + (checkOffset + velocityExtension);

            if (this.grounded)
            { //<-------------------------------- CHEAP RAYCAST TO CHECK IF GROUND STILL THERE
#if UNITY_EDITOR
                if (debug)
                    Debug.DrawLine(transform.TransformPoint(box.center) + raycastLocalOrigin - Vector3.up * checkOffset,
                        transform.TransformPoint(box.center) + raycastLocalOrigin - Vector3.up * checkOffset + Vector3.down * box.bounds.extents.y);
#endif

                if (Physics.Raycast(
                    transform.TransformPoint(box.center) + raycastLocalOrigin - Vector3.up * checkOffset,
                    Vector3.down,
                    out hit,
                    box.bounds.extents.y,
                    groundMask
                    ))
                {

                    grounded = Vector3.Angle(hit.normal, Vector3.up) <= maxGroundAngle;
                }
            }
            if (!this.grounded || !grounded)
            { //<----------------------- PRECISE BOX CAST TO FIND GROUND
#if UNITY_EDITOR
                if (debug)
                    DrawBoxCastBox(
                        transform.TransformPoint(box.center),
                        boxCastScale * checkIndent,
                        transform.rotation,
                        Vector3.down,
                        boxCastDistance,
                        Color.rebeccaPurple);
#endif

                if (Physics.BoxCast( 
                    transform.TransformPoint(box.center), 
                    boxCastScale * checkIndent,
                    Vector3.down,
                    out hit,
                    transform.rotation,
                    boxCastDistance,
                    groundMask))
                { 

                    grounded = Vector3.Angle(hit.normal, Vector3.up) <= maxGroundAngle;

                    raycastLocalOrigin = hit.point - transform.TransformPoint(box.center);
                    raycastLocalOrigin.y = 0f;
                }
            }
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
