using UnityEngine;

public class Patrol
{
    public LocomotionPath path;
    public bool followPathDirection = true;
    public float threshold = 0.2f;

    private int currentIndex;
    private int direction = 1;

    public Patrol (LocomotionPath path, float threshold)
    {
        this.path = path;
        this.threshold = threshold;
        currentIndex = this.path.GetStartIndex();
        direction = followPathDirection ? 1 : -1;
    }

    public Vector3 GetTargetDirection (Vector3 position)
    {
        if (path == null || path.Count == 0) return Vector3.zero;

        Vector3 target = path.GetPoint(currentIndex);
        Vector3 toTarget = target - position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= threshold)
        {
            currentIndex = path.GetNextIndex(currentIndex, ref direction);
            target = path.GetPoint(currentIndex);
            toTarget = target - position;
        }

        Vector3 moveDir = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector3.zero;
        return moveDir;
    }
}