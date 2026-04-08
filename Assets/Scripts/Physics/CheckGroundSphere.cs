using UnityEngine;
using Utils;
using static PhysicsDebugExtensions;
using static DebugDraw;

public class CheckGroundSphere : CheckGround
{
    [Range(0.01f, 0.5f)]
    public float checkOffset = 0.1f;
    [Range(0.5f, 1f)]
    public float checkIndent = 0.9f;

    protected SphereCollider sphere;

    protected RaycastHit hit;
    protected Vector3 raycastLocalOrigin;

    protected override void Awake()
    {
        base.Awake();
        sphere = coll as SphereCollider;
    }
    public override bool Check (Vector3 velocity = default)
    {
        bool grounded = false;

        float velocityExtension = Mathf.Max(0f, -velocity.y) * Time.fixedDeltaTime;

        //Start Check ->
#if UNITY_EDITOR
        if (debug)
            DebugDraw.Box(sphere.bounds.center - Vector3.up * (checkOffset + velocityExtension),
            sphere.bounds.extents, Quaternion.identity, Color.beige);
#endif
        if (Physics.CheckBox(
            sphere.bounds.center - Vector3.up * (checkOffset + velocityExtension),
            sphere.bounds.extents, 
            Quaternion.identity, groundMask))
        { //<---------------------------- CHECK BOX TO FIND POTENTIAL GROUND

            float sphereCastRadius = sphere.radius * 
                Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z); //get max value (thats how sphere colliders scales)

            float sphereCastDistance = sphere.bounds.size.y * ((1f - checkIndent) * 0.5f) + (checkOffset + velocityExtension);

            if (debug)
                DrawSphereCast(
                    transform.TransformPoint(sphere.center),
                    sphereCastRadius * checkIndent,
                    Vector3.down,
                    sphereCastDistance,
                    Color.rebeccaPurple);

            if (this.grounded)
            { //<-------------------------------- CHEAP RAYCAST TO CHECK IF GROUND STILL THERE
#if UNITY_EDITOR
                if (debug)
                    Debug.DrawLine(transform.TransformPoint(sphere.center) + raycastLocalOrigin - Vector3.up * checkOffset,
                        transform.TransformPoint(sphere.center) + raycastLocalOrigin - Vector3.up * checkOffset + Vector3.down * sphere.bounds.extents.y);
#endif

                if (Physics.Raycast(
                    transform.TransformPoint(sphere.center) + raycastLocalOrigin - Vector3.up * checkOffset,
                    Vector3.down,
                    out hit,
                    sphere.bounds.extents.y,
                    groundMask
                    ))
                {

                    grounded = Vector3.Angle(hit.normal, Vector3.up) <= maxGroundAngle;
                }
            }
            if (!this.grounded || !grounded)
            { //<----------------------- PRECISE SPHERE CAST TO FIND GROUND
#if UNITY_EDITOR
                if (debug)
                    DrawSphereCast(
                        transform.TransformPoint(sphere.center),
                        sphereCastRadius * checkIndent,
                        Vector3.down,
                        sphereCastDistance,
                        Color.rebeccaPurple);
#endif

                if (Physics.SphereCast( 
                    transform.TransformPoint(sphere.center), 
                    sphereCastRadius * checkIndent,
                    Vector3.down,
                    out hit,
                    sphereCastDistance,
                    groundMask))
                { 

                    grounded = Vector3.Angle(hit.normal, Vector3.up) <= maxGroundAngle;

                    raycastLocalOrigin = hit.point - transform.TransformPoint(sphere.center);
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
