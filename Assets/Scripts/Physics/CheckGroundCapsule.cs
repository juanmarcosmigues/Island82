using UnityEngine;
using Utils;
using static PhysicsDebugExtensions;
using static DebugDraw;

public class CheckGroundCapsuleCollider : CheckGround
{
    [Range(0.01f, 0.5f)]
    public float checkOffset = 0.1f;
    [Range(0.5f, 1f)]
    public float checkIndent = 0.9f;

    protected CapsuleCollider capsule;

    protected RaycastHit hit;
    protected Vector3 raycastLocalOrigin;

    /// <summary>
    /// Returns the world-space center of the capsule's bottom sphere.
    /// </summary>
    protected Vector3 BottomSphereCenter
    {
        get
        {
            Vector3 worldCenter = transform.TransformPoint(capsule.center);
            float scaledRadius = capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
            float scaledHalfHeight = (capsule.height * 0.5f) * transform.lossyScale.y;
            // The sphere center is offset from the capsule center by half-height minus the radius
            float offset = Mathf.Max(0f, scaledHalfHeight - scaledRadius);
            return worldCenter - Vector3.up * offset;
        }
    }

    /// <summary>
    /// Returns the world-scaled radius of the capsule (which is the sphere cast radius).
    /// </summary>
    protected float ScaledRadius
    {
        get
        {
            return capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        capsule = coll as CapsuleCollider;
    }

    public override bool Check(Vector3 velocity = default)
    {
        bool grounded = false;

        float velocityExtension = Mathf.Max(0f, -velocity.y) * Time.fixedDeltaTime;

        Vector3 bottomCenter = BottomSphereCenter;
        float radius = ScaledRadius;

        // Use the capsule bounds for the initial cheap CheckBox
        //Start Check ->
#if UNITY_EDITOR
        if (debug)
            DebugDraw.Box(capsule.bounds.center - Vector3.up * (checkOffset + velocityExtension),
            capsule.bounds.extents, Quaternion.identity, Color.beige);
#endif
        if (Physics.CheckBox(
            capsule.bounds.center - Vector3.up * (checkOffset + velocityExtension),
            capsule.bounds.extents,
            Quaternion.identity, groundMask))
        { //<---------------------------- CHECK BOX TO FIND POTENTIAL GROUND

            float sphereCastRadius = radius * checkIndent;
            float sphereCastDistance = radius * (1f - checkIndent) + (checkOffset + velocityExtension);

            if (debug)
                DrawSphereCast(
                    bottomCenter,
                    sphereCastRadius,
                    Vector3.down,
                    sphereCastDistance,
                    Color.rebeccaPurple);

            if (this.grounded)
            { //<-------------------------------- CHEAP RAYCAST TO CHECK IF GROUND STILL THERE
#if UNITY_EDITOR
                if (debug)
                    Debug.DrawLine(bottomCenter + raycastLocalOrigin - Vector3.up * checkOffset,
                        bottomCenter + raycastLocalOrigin - Vector3.up * checkOffset + Vector3.down * radius);
#endif

                if (Physics.Raycast(
                    bottomCenter + raycastLocalOrigin - Vector3.up * checkOffset,
                    Vector3.down,
                    out hit,
                    radius + velocityExtension,
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
                        bottomCenter,
                        sphereCastRadius,
                        Vector3.down,
                        sphereCastDistance,
                        Color.rebeccaPurple);
#endif

                if (Physics.SphereCast(
                    bottomCenter,
                    sphereCastRadius,
                    Vector3.down,
                    out hit,
                    sphereCastDistance,
                    groundMask))
                {
                    //refine the normal with a raycast because the sphere cast normals are weird data.
                    if (Physics.Raycast(hit.point + Vector3.up * 0.1f, Vector3.down,
                                         out RaycastHit refinedHit, 0.3f, groundMask))
                    {
                        hit.normal = refinedHit.normal;
                    }

                    grounded = Vector3.Angle(hit.normal, Vector3.up) <= maxGroundAngle;

                    raycastLocalOrigin = hit.point - bottomCenter;
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