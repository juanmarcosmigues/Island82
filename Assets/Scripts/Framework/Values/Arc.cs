using UnityEngine;
using static Utils.GizmosExtensions;
[System.Serializable]
public struct Arc
{
    public float angle;
    public float radius;

    /// <summary>
    /// Returns true if <paramref name="point"/> is inside the FOV cone
    /// (symmetric ±angle/2 around forward, on the XZ plane).
    /// </summary>
    public bool Contains(Vector3 position, Quaternion rotation, Vector3 point)
    {
        Vector3 local = Quaternion.Inverse(rotation) * (point - position);

        float distSq = local.x * local.x + local.z * local.z;
        if (distSq > radius * radius) return false;

        float a = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
        return Mathf.Abs(a) <= angle * 0.5f;
    }

    public void Debug(Vector3 position, Quaternion rotation, Color color = default)
    {
        Gizmos.color = color;

        // Offset rotation by -angle/2 around local Y so the arc is centered on forward
        Quaternion offset = rotation * Quaternion.Euler(0f, -angle * 0.5f, 0f);
        DrawWireArc(position, radius, angle, 20, offset);

        // Draw the two edges of the cone so the FOV slice is visually closed
        Vector3 leftDir = rotation * Quaternion.Euler(0f, -angle * 0.5f, 0f) * Vector3.forward;
        Vector3 rightDir = rotation * Quaternion.Euler(0f, angle * 0.5f, 0f) * Vector3.forward;
        Gizmos.DrawLine(position, position + leftDir * radius);
        Gizmos.DrawLine(position, position + rightDir * radius);
    }
}
