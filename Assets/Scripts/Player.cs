using System.Collections;
using UnityEngine;
using static CombatHandler;

public class Player : MonoBehaviour
{
    private const float TIME_INVULNERABLE = 3f;
    private const float SPEED_KNOCKBACK = 7f;
    public static Player Instance { get; private set; }

    [Header("Physics")]
    public float moveSpeed;
    public float jumpImpulse;
    public float upwardsGravity;
    public float downwardsGravity;
    public float drag;
    public float airDrag;
    public LayerMask groundMask;

    [Header("References")]
    public GameObject meshPlayer;
    public GameObject meshPlayerHead;
    public Transform directionRoot;
    public Transform renderRoot;
    public ParticleSystem dust;
    public VisualFeedback vfGetCoin;

    [HideInInspector] public PlayerInput input;
    [HideInInspector] public BoxCollider coll;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public CombatHandler combatHandler;

    public event System.Action<int> OnPickUpCoin;
    public event System.Action OnGroundedStart;
    public event System.Action<int> OnGetHurt;

    public int CurrentLife { get; private set; } = 5;
    public bool Invulnerable { get; private set; } = false;

    Timestamp gotHitTimer;
    Coroutine invulnerableBlinkCoroutine;

    public Bounds? MaxAirBounds { get; private set; }
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
    Vector3 horizontalVelocity;

    private void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody>();
        coll = GetComponent<BoxCollider>();
        input = GetComponent<PlayerInput>();
        combatHandler = GetComponent<CombatHandler>();
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

        combatHandler.OnGetHit += GetHit;
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
            directionRoot.forward = Quaternion.AngleAxis(saturatedAngle, Vector3.up) * forward;
        }

        if (gotHitTimer.remainingTime < 0f && invulnerableBlinkCoroutine != null)
        {
            StopCoroutine(invulnerableBlinkCoroutine);
            invulnerableBlinkCoroutine = null;
            renderRoot.gameObject.SetActive(true);
            Invulnerable = false;
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
        OnPickUpCoin?.Invoke(1);
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
            rb.MovePosition(rb.position + (ground.y - coll.bounds.min.y) * Vector3.up); //snap

        if (grounded)
        {
            moveVelocity = Vector3.zero;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, drag * Time.fixedDeltaTime);
        }
        else
        {
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, airDrag * Time.fixedDeltaTime);
        }

        velocity += verticalVelocity;
        velocity += moveVelocity;
        velocity += horizontalVelocity;

        IsGrounded = grounded;
        VerticalVelocity = verticalVelocity.y;

        rb.linearVelocity = velocity;
        lastVelocity = velocity;

        if (grounded)
        {
            MaxAirBounds = null;
        }
        else
        {
            if (!MaxAirBounds.HasValue)
                MaxAirBounds = coll.bounds;

            MaxAirBounds = coll.bounds.center.y > MaxAirBounds.Value.center.y 
                ? coll.bounds : MaxAirBounds.Value;
        }
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
    void GetHit(GameObject source, int damage, Weight weight, string tag)
    {
        if (Invulnerable) { return; }

        CurrentLife -= damage;
        Invulnerable = true;
        gotHitTimer.Set(TIME_INVULNERABLE);

        invulnerableBlinkCoroutine = 
            StartCoroutine(renderRoot.gameObject.Blink(0.05f, TIME_INVULNERABLE));

        FrameFreeze.Freeze(0.3f, () =>
        {
            MainCameraShaker.instance.Shake(0.2f, 0.3f, 0.2f);
            horizontalVelocity += (transform.position - source.transform.position).FlattenY().normalized * SPEED_KNOCKBACK;
        });

        OnGetHurt?.Invoke(damage);
    }
}
