using UnityEditor.ShaderGraph;
using UnityEngine;

public class Locomotion : MonoBehaviour
{
    public float speed;
    public float gravity;
    public float drag;
    public float airDrag;
    public float rotationSpeed;

    private Rigidbody rigidBody;
    private new Collider collider;
    private CheckGround groundChecker;

    public System.Action <GroundData> OnLand;

    bool tickedMove;

    bool grounded;

    Vector3 lastVelocity;
    Vector3 moveVelocity;
    Vector3 verticalVelocity;
    Vector3 horizontalVelocity;
    GroundData? groundData;
    MovingSurface movingSurface;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        groundChecker = GetComponent<CheckGround>();
        collider = GetComponent<Collider>();
    }
    void FixedUpdate ()
    {
        //Grounded Step ----------------------------->
        bool newGrounded = verticalVelocity.y <= 0f ? groundChecker.Check(lastVelocity) : false;
        if (newGrounded != grounded)
        {
            movingSurface = null;
            groundData = null;
            if (newGrounded)
            {
                movingSurface = groundChecker.GroundData.coll.GetComponent<MovingSurface>();
                Land(groundChecker.GroundData);
            }
        }
        grounded = newGrounded;
        groundData = groundChecker.GroundData;

        //Velocity Step ----------------------------->
        Vector3 velocity = Vector3.zero;
        Vector3 addedPosition = Vector3.zero;

        if (!tickedMove) 
            moveVelocity = Vector3.zero;

        if (grounded)
            verticalVelocity.y = Mathf.Max(verticalVelocity.y, 0f);
        else
            verticalVelocity += gravity * Vector3.up * Time.fixedDeltaTime;

        if (grounded)
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, drag * Time.fixedDeltaTime);
        else
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, airDrag * Time.fixedDeltaTime);

        velocity += verticalVelocity;
        velocity += moveVelocity;
        velocity += horizontalVelocity;

        Vector3 nextPosition = rigidBody.position + velocity * Time.fixedDeltaTime;
        Vector3 nextFeetPosition = nextPosition - collider.bounds.extents.y * Vector3.up;
        if (grounded)
        {
            //Ground Snap
            if (verticalVelocity.y <= 0f)
                addedPosition += (groundData.Value.point.y - nextFeetPosition.y) * Vector3.up;
            if (movingSurface)
                addedPosition += movingSurface.GetFinalFrameTranslation(transform.position);
        }

        velocity += addedPosition / Time.fixedDeltaTime;

        rigidBody.linearVelocity = velocity;
        lastVelocity = velocity;

        //Other --------------------------------------->
        Vector3 lookDirection = Vector3.RotateTowards(transform.forward, moveVelocity.normalized, rotationSpeed * Time.fixedDeltaTime, rotationSpeed * Time.fixedDeltaTime);
        if (moveVelocity.sqrMagnitude < 0.001f)
            lookDirection = transform.forward;

        Quaternion rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        rigidBody.MoveRotation(rotation);

        //End Step ------------------------------------>
        tickedMove = false;
    }
    void Land (GroundData ground)
    {
        OnLand?.Invoke(ground);
    }

    public void Move(Vector3 direction, float factor = 1f)
    {
        moveVelocity = direction * speed * factor;
        tickedMove = true;
    }
    public void Jump(float force)
    {
        verticalVelocity = Vector3.up * force;
    }

    public void KillVerticalVelocity ()
    {
        verticalVelocity = Vector3.zero;
    }
}
