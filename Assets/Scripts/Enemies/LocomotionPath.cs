// Path.cs
using UnityEngine;

public class LocomotionPath : MonoBehaviour
{
    public enum PathMode { Loop, PingPong }

    public PathMode mode = PathMode.Loop;
    public bool flip = false;

    public int Count => transform.childCount;

    public Vector3 GetPoint(int index)
    {
        return transform.GetChild(Mathf.Clamp(index, 0, Count - 1)).position;
    }

    /// <summary>
    /// Returns the next index given the current index and a direction (+1/-1).
    /// Updates direction by reference for ping-pong mode.
    /// </summary>
    public int GetNextIndex(int current, ref int direction)
    {
        if (Count == 0) return 0;

        int effectiveDir = flip ? -direction : direction;
        int next = current + effectiveDir;

        if (mode == PathMode.Loop)
        {
            if (next >= Count) next = 0;
            else if (next < 0) next = Count - 1;
        }
        else // PingPong
        {
            if (next >= Count || next < 0)
            {
                direction = -direction;
                next = current + (flip ? -direction : direction);
                next = Mathf.Clamp(next, 0, Count - 1);
            }
        }

        return next;
    }

    public int GetStartIndex()
    {
        return flip ? Count - 1 : 0;
    }

    private void OnDrawGizmos()
    {
        int count = Count;
        if (count < 2)
        {
            if (count == 1)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.GetChild(0).position, 0.15f);
            }
            return;
        }

        Gizmos.color = Color.yellow;

        int segments = (mode == PathMode.Loop) ? count : count - 1;

        for (int i = 0; i < segments; i++)
        {
            int aIdx = i;
            int bIdx = (i + 1) % count;

            Vector3 a = transform.GetChild(aIdx).position;
            Vector3 b = transform.GetChild(bIdx).position;

            if (flip) { var tmp = a; a = b; b = tmp; }

            Gizmos.DrawWireSphere(transform.GetChild(i).position, 0.15f);
            Gizmos.DrawLine(a, b);
            DrawArrow(a, b);
        }

        if (mode != PathMode.Loop)
        {
            Gizmos.DrawWireSphere(transform.GetChild(count - 1).position, 0.15f);
        }
    }

    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Vector3 dir = (to - from);
        float len = dir.magnitude;
        if (len < 0.0001f) return;
        dir /= len;

        Vector3 mid = (from + to) * 0.5f;
        float headSize = Mathf.Min(0.3f, len * 0.3f);

        Vector3 up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.99f ? Vector3.right : Vector3.up;
        Vector3 right = Vector3.Cross(dir, up).normalized;

        Vector3 left = mid - dir * headSize + right * headSize * 0.5f;
        Vector3 rgt = mid - dir * headSize - right * headSize * 0.5f;

        Gizmos.DrawLine(mid, left);
        Gizmos.DrawLine(mid, rgt);
    }
}