using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public float moveSpeed;
    public float jumpImpulse;
    public float upwardsGravity;
    public float downwardsGravity;

    public VisualFeedback vfGetCoin;
    public GameObject meshPlayer;
    public GameObject meshPlayerHead;
    public Transform renderRoot;
    public ParticleSystem dust;
    public PlayerInput input;
    public BoxCollider coll;
    public Rigidbody body;
    public LayerMask groundMask;

    public event System.Action OnGroundedStart;
    public float VerticalVelocity {  get; private set; }
    public bool IsGrounded {  get; private set; }

    Vector3 lookDirection;
    Timestamp pressedJumpTimer;

    Vector3[] groundCheckPoints;
    float currentJumpForce;
    Vector3 lastVelocity;
    bool grounded;
    Vector3 ground;
    float jumpValue;

    Vector3 moveVelocity;
    Vector3 verticalVelocity;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        groundCheckPoints = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(coll.size.x, 0, coll.size.z) * 0.5f,
            new Vector3(-coll.size.x, 0, coll.size.z) * 0.5f,
            new Vector3(-coll.size.x, 0, -coll.size.z) * 0.5f,
            new Vector3(coll.size.x, 0, -coll.size.z) * 0.5f,
        };

        input.GetButton("ButtonSouth").onPressedDown += PressJump;
        input.GetButton("ButtonSouth").onRelease += ReleaseJump;
        input.onMovementAxisMove += MoveAxis;
    }
    private void Update()
    {
        if (pressedJumpTimer.remainingTime > 0f && grounded)
        {
            PressJump();
        }
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Vector3 forward = Camera.main.RotateTowardsCamera(new Vector2(0f, 1f));
            Vector3 right = Camera.main.RotateTowardsCamera(new Vector2(1f, 0f));
            float angle = Vector3.Angle(forward, lookDirection);
            float side = Mathf.Sign(Vector3.Dot(lookDirection, right));
            float saturatedAngle = Mathf.Round(angle / 45f) * 45f * side;
            renderRoot.forward = Quaternion.AngleAxis(saturatedAngle, Vector3.up) * forward;
        }
    }

    private void PressJump ()
    {
        if (grounded)
        {
            Jump(jumpImpulse);
        }
        else
        {
            pressedJumpTimer.Set(0.1f);
        }
    }
    private void ReleaseJump (float time)
    {
        JumpStop();
    }
    private void MoveAxis (Vector2 input, float val)
    {
        Vector3 dir = Camera.main.RotateTowardsCamera(input).normalized;
        lookDirection = dir;
        if (val > 0.1f) Move(dir, val);
        else Move(dir, 0f);
    }
    public void GetCoin ()
    {
        vfGetCoin.Play();
    }
    public void InsideObject (bool inside)
    {
        meshPlayer.SetActive(!inside);
        meshPlayerHead.SetActive(inside);
    }
    public void Move (Vector3 dir, float factor)
    {
        moveVelocity = dir * factor * moveSpeed;
    }
    public void Jump(float impulse, bool ignoreGrounded = false)
    {
        if (grounded || ignoreGrounded)
        {
            currentJumpForce = impulse;
            verticalVelocity.y = impulse * 0.7f;
            jumpValue = 1f;
            return;
        }

        jumpValue = Mathf.Clamp01(jumpValue - Time.deltaTime);
        verticalVelocity.y = currentJumpForce * jumpValue;
    }
    public void JumpStop ()
    {
        if (verticalVelocity.y < 0)
            return;

        verticalVelocity.y *= 0.5f;
    }
    private void FixedUpdate()
    {
        (bool found, Vector3 point) newGrounded = 
            verticalVelocity.y <= 0f ? CheckGrounded() : (false, Vector3.zero);

        if (newGrounded.found != grounded)
        {
            if (newGrounded.found)
            {
                OnGroundedStart?.Invoke();
                dust.Play();
            }
        }
        grounded = newGrounded.found;
        ground = newGrounded.point;

        Vector3 velocity = Vector3.zero;
        
        if (grounded) 
            verticalVelocity.y = Mathf.Max(verticalVelocity.y, 0f);
        else 
            verticalVelocity += (lastVelocity.y < 1f ? downwardsGravity : upwardsGravity) * Vector3.up * Time.fixedDeltaTime;
        if (verticalVelocity.y <= 0f && grounded)
            body.MovePosition(body.position + (ground.y - coll.bounds.min.y) * Vector3.up); //snap

        if (grounded)
            moveVelocity = Vector3.zero;

        velocity += verticalVelocity;
        velocity += moveVelocity;

        IsGrounded = grounded;
        VerticalVelocity = verticalVelocity.y;

        body.linearVelocity = velocity;
        lastVelocity = velocity;
    }
    private (bool, Vector3) CheckGrounded ()
    {
        RaycastHit r;
        float padding = 0.01f;
        Vector3 bottomCenter = transform.position + coll.center - Vector3.up * coll.size.y * 0.5f;
        bottomCenter += Vector3.up * padding;
        float distance = padding * 2f + -lastVelocity.y * Time.fixedDeltaTime;
        for (int i = 0; i < groundCheckPoints.Length; i++)
        {
            if (Physics.Raycast(bottomCenter + groundCheckPoints[i], Vector3.down, out r, distance, groundMask))
            {
                return (true, r.point);
            }
        }

        return (false, Vector3.zero);
    }

}
