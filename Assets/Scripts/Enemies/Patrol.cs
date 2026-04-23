using UnityEngine;

[System.Serializable]
public class Patrol
{
    public LocomotionPath path;
    public bool flat = true;
    public bool followPathDirection = true;
    public float threshold = 0.2f;

    private bool initialized;
    private int currentIndex;
    private int direction = 1;

    public Patrol (LocomotionPath path, float threshold)
    {
        this.path = path;
        this.threshold = threshold;   
    }

    public void SetNextPointByPosition (Vector3 position)
    {
        currentIndex = path.GetClosestForwardIndex(position, direction);
    }
    public (Vector3 point, Vector3 direction) GetTarget (Vector3 position)
    {
        if (!initialized)
        {      
            direction = followPathDirection ? 1 : -1;
            currentIndex = path.GetClosestForwardIndex(position, direction);
            initialized = true;
        }

        if (path == null || path.Count == 0) return (Vector3.zero, Vector3.zero);

        Vector3 target = path.GetPoint(currentIndex);
        Vector3 toTarget = target - position;
        if (flat) toTarget.y = 0f;

        if (toTarget.magnitude <= threshold)
        {
            currentIndex = path.GetNextIndex(currentIndex, ref direction);
            target = path.GetPoint(currentIndex);
            toTarget = target - position;
            if (flat) toTarget.y = 0f;
        }

        Vector3 moveDir = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector3.zero;
        return (target, moveDir);
    }
}