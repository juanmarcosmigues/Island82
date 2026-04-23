using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using Utils;

public class Screw : MonoBehaviour
{
    private const float ACTIVE_VELOCITY = 8f;
    private const float IDLE_VELOCITY = 1f;
    private const float HEIGHT_PER_ANGLE = 0.005f;

    public float radius;
    public int screwTurns;
    public Transform screw;

    private Rigidbody screwRigidBody;
    private SurfaceProperties surface;
    private Vector3 currentDirection;
    private Vector3 currentDirectionPerp;
    private float screwMaxAngle;
    private float currentScrewValue;

    private void Awake()
    {
        surface = GetComponent<SurfaceProperties>();
        screwRigidBody = screw.GetComponent<Rigidbody>();
        screwMaxAngle = screwTurns * 45f;
    }
    private void FixedUpdate()
    {
        if (Player.Instance.Screw != this)
        {
            if (!surface.landed) return;

            float dot = Vector3.Dot(screw.forward, Player.Instance.LookingDirection);
            if (dot > 0.9f)
            {
                Player.Instance.ScrewIn(this);
                SetScrewDirection(screw.forward);
            }
        }
        else
        {
            Vector3 target = Vector3.up * currentScrewValue * HEIGHT_PER_ANGLE;
            bool goingUp = screwRigidBody.position.y < target.y;
            screw.localPosition = Vector3.MoveTowards
                (screw.localPosition,
                target,
                ACTIVE_VELOCITY * Time.fixedDeltaTime);
        }
    }

    public Vector3 ConstraintPlayerRotation (Vector3 lookDirection)
    {
        Vector3 polarizedDirection = PolarizedDirection(lookDirection);

        float angle = Vector3.Angle(polarizedDirection, currentDirection);

        if (angle > 1f && angle < 50f)
        {
            float sign = Vector3.Dot(currentDirectionPerp, polarizedDirection) < 0.5f ? 1f : -1f;
            float newValue = currentScrewValue + angle * sign;

            if (newValue < 0f)
            {
                return currentDirection;
            }
            if (newValue > screwMaxAngle)
            {
                return currentDirection;
            }

            SetScrewDirection(polarizedDirection);
            currentScrewValue = newValue;

            return polarizedDirection;
        }

        return currentDirection;
    }
    private Vector3 PolarizedDirection (Vector3 direction )
    {
        Vector3 forward = Camera.main.RotateTowardsCamera(new Vector2(0f, 1f));
        Vector3 right = Camera.main.RotateTowardsCamera(new Vector2(1f, 0f));
        float angle = Vector3.Angle(forward, direction);
        float side = Mathf.Sign(Vector3.Dot(direction, right));
        float saturatedAngle = Mathf.Round(angle / 45f) * 45f * side;

        return Quaternion.AngleAxis(saturatedAngle, Vector3.up) * forward;
    } 
    private void SetScrewDirection (Vector3 dir)
    {
        currentDirection = dir;
        currentDirectionPerp = Quaternion.AngleAxis(90f, Vector3.up) * dir;
        screw.forward = currentDirection;
    }
    private void OnDrawGizmos()
    {
        GizmosExtensions.DrawWireCircle(transform.position, radius);
    }
}
