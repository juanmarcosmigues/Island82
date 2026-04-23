using UnityEngine;

/// <summary>
/// Makes this GameObject follow a target Transform as if parented to it,
/// without any hierarchy dependency. Supports position, rotation, and scale
/// with independent axis toggles and an optional offset.
/// </summary>
public class VirtualParent : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The transform to treat as the virtual parent.")]
    public Transform target;

    [Header("Follow Toggles")]
    public bool followPosition = true;
    public bool followRotation = true;
    public bool followScale = false;

    [Header("Axis Mask (Position)")]
    public bool x = true;
    public bool y = true;
    public bool z = true;

    [Header("Offset (Local to Target)")]
    [Tooltip("Offset in the target's local space, like a child's localPosition.")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    [Header("Smoothing")]
    [Tooltip("0 = instant snap. Higher = softer follow.")]
    [Range(0f, 50f)]
    public float positionSmoothing = 0f;
    [Range(0f, 50f)]
    public float rotationSmoothing = 0f;

    [Header("Timing")]
    public UpdateMode updateMode = UpdateMode.LateUpdate;

    public enum UpdateMode
    {
        Update,
        LateUpdate,
        FixedUpdate
    }

    // Cached on Start so the offset can be auto-calculated from initial placement.
    private bool _initialized;

    private void Start()
    {
        CacheOffset();
    }

    /// <summary>
    /// Calculates the offset from the current world position/rotation
    /// relative to the target, so you can just place the object in the
    /// scene and hit play.
    /// </summary>
    public void CacheOffset()
    {
        if (target == null) return;

        // Convert current world position into target-local offset.
        positionOffset = target.InverseTransformPoint(transform.position);
        rotationOffset = (Quaternion.Inverse(target.rotation) * transform.rotation).eulerAngles;
        _initialized = true;
    }

    private void Update()
    {
        if (updateMode == UpdateMode.Update) Apply();
    }

    private void LateUpdate()
    {
        if (updateMode == UpdateMode.LateUpdate) Apply();
    }

    private void FixedUpdate()
    {
        if (updateMode == UpdateMode.FixedUpdate) Apply();
    }

    private void Apply()
    {
        if (target == null) return;
        if (!_initialized) CacheOffset();

        float dt = (updateMode == UpdateMode.FixedUpdate) ? Time.fixedDeltaTime : Time.deltaTime;

        // ---- Position ----
        if (followPosition)
        {
            Vector3 goalPos = target.TransformPoint(positionOffset);

            // Axis masking: keep current value on disabled axes.
            if (!x) goalPos.x = transform.position.x;
            if (!y) goalPos.y = transform.position.y;
            if (!z) goalPos.z = transform.position.z;

            transform.position = positionSmoothing > 0f
                ? Vector3.Lerp(transform.position, goalPos, 1f - Mathf.Exp(-positionSmoothing * dt))
                : goalPos;
        }

        // ---- Rotation ----
        if (followRotation)
        {
            Quaternion goalRot = target.rotation * Quaternion.Euler(rotationOffset);

            transform.rotation = rotationSmoothing > 0f
                ? Quaternion.Slerp(transform.rotation, goalRot, 1f - Mathf.Exp(-rotationSmoothing * dt))
                : goalRot;
        }

        // ---- Scale ----
        if (followScale)
        {
            transform.localScale = target.lossyScale;
        }
    }

    /// <summary>
    /// Snap immediately to the target (ignoring smoothing) then re-cache the offset.
    /// Useful after teleporting the target.
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        float tmpPos = positionSmoothing;
        float tmpRot = rotationSmoothing;
        positionSmoothing = 0f;
        rotationSmoothing = 0f;
        Apply();
        positionSmoothing = tmpPos;
        rotationSmoothing = tmpRot;
    }

    /// <summary>
    /// Assign a new target at runtime and re-cache the offset from
    /// the current world position.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        CacheOffset();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireSphere(target.TransformPoint(positionOffset), 0.05f);
    }
#endif
}