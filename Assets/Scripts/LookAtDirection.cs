using UnityEngine;

public static partial class GameWorld
{
    public static Vector3 LookAtDirection (Vector3 direction)
    {
        Vector3 forward = Camera.main.RotateTowardsCamera(new Vector2(0f, 1f));
        Vector3 right = Camera.main.RotateTowardsCamera(new Vector2(1f, 0f));

        float angle = Vector3.Angle(forward, direction);
        float side = Mathf.Sign(Vector3.Dot(direction, right));
        float saturatedAngle = Mathf.Round(angle / 45f) * 45f * side;
        return Quaternion.AngleAxis(saturatedAngle, Vector3.up) * forward;
    }
}
