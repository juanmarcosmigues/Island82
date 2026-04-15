using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static CombatHandler;

[DefaultExecutionOrder(GameDefinitions.EXECUTION_ORDER_PLAYER)]
public class Player : MonoBehaviour, IDynamicObject
{
    private const float TIME_INVULNERABLE = 3f;
    private const float SPEED_KNOCKBACK = 7f;
    private const float MAX_GRAVITY = 16f;
    private const float HEAVY_FALL_VELOCITY = 16f;
    private const float SINK_HEIGHT = 0.7f;
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
    public Transform dirtHole;

    [HideInInspector] public PlayerInput input;
    [HideInInspector] public Collider coll;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public CombatHandler combatHandler;
    [HideInInspector] public ObjectSounds sounds;
    [HideInInspector] public CheckGround groundChecker;

    public event System.Action<int> OnPickUpCoin;
    public event System.Action OnGroundedStart;
    public event System.Action<int> OnGetHurt;

    private List<IInteractable> availableInteractions = new();

    public bool PlayerInControl { get; set; } = true;
    public int CurrentLife { get; private set; } = 5;
    public bool Invulnerable { get; private set; } = false;

    Timestamp gotHitTimer;
    Coroutine invulnerableBlinkCoroutine;

    public Bounds NextFrameBounds => nextFrameBounds;
    public Bounds? MaxAirBounds { get; private set; }
    public float VerticalVelocity {  get; private set; }
    public bool IsGrounded {  get; private set; }
    public bool IsHeavyFalling { get; private set; }
    public bool Sunk => sunkValue > 0f;

    Vector3 lookDirection;
    Timestamp pressedJumpTimer;

    float currentJumpForce;
    Vector3 lastVelocity;
    bool grounded;
    float jumpValue;
    Vector3 checkpoint;
    MovingSurface movingSurface;
    SurfaceProperties surfaceProperties;
    GroundData? groundData;
    float sunkValue;
    Vector3 sunkPosition;
    Bounds nextFrameBounds;

    Vector3 moveVelocity;
    Vector3 verticalVelocity;
    Vector3 horizontalVelocity;

    float pickUpCoinCombo;

    private void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
        input = GetComponent<PlayerInput>();
        combatHandler = GetComponent<CombatHandler>();
        sounds = GetComponent<ObjectSounds>();
        groundChecker = GetComponent<CheckGround>();
    }
    private void Start()
    {
        input.GetButton("ButtonSouth").onPressedDown += PressJump;
        input.GetButton("ButtonSouth").onRelease += ReleaseJump;
        input.GetButton("ButtonWest").onPressedDown += PressInteract;
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

        if (pickUpCoinCombo > 0f)
            pickUpCoinCombo = Mathf.Clamp01(pickUpCoinCombo-Time.deltaTime*0.5f);

        Shader.SetGlobalVector("_PlayerPosition", transform.position);
    }

    private void PressInteract()
    {
        string interactables = "Interactions List: \n";
        availableInteractions.ForEach(x => interactables += x.InteractionName + "\n");
        Debug.Log(interactables);
        if (availableInteractions.Count > 0) 
        {
            var interactionIndex = availableInteractions.Count-1;
            var interaction = availableInteractions.Last();
            interaction.Interact();

            if (interaction.Consumable)
                availableInteractions.RemoveAt(interactionIndex);
        }
    }
    private void PressJump ()
    {
        if (!PlayerInControl) return;

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
        if (!PlayerInControl) return;
        if (Sunk) return;

        Vector3 dir = Camera.main.RotateTowardsCamera(input).normalized;
        lookDirection = dir;
        if (val > 0.1f) Move(dir, val);
        else Move(dir, 0f);
    }
    public void GetCoin ()
    {
        vfGetCoin.Play();
        float pitch = Mathf.Lerp(0.9f, 1.2f, Mathf.Clamp01(pickUpCoinCombo));
        sounds.PlaySound("PickUp", 0.5f);
        pickUpCoinCombo += 0.15f;
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
        if (Sunk)
        {
            UnSink();
            return;
        } 
        
        if (grounded || ignoreGrounded)
        {
            currentJumpForce = impulse;
            verticalVelocity.y = impulse * 0.7f;
            jumpValue = 1f;

            sounds.PlaySound("Jump");

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
        //Grounded Step ----------------------------->
        bool newGrounded = verticalVelocity.y <= 0f ? groundChecker.Check(lastVelocity) : false;
        if (newGrounded != grounded)
        {
            movingSurface = null;
            groundData = null;

            if (newGrounded)
            {
                var surface = groundChecker.GroundData.coll.GetComponent<SurfaceProperties>();
                if ((surface != null && surface.CanLand()) || surface == null)
                    Land(groundChecker.GroundData, surface);
                else if (surface != null)
                    newGrounded = false;
            }
        }
        grounded = newGrounded;

        //Velocity Step ----------------------------->
        Vector3 velocity = Vector3.zero;
        
        if (grounded) 
            verticalVelocity.y = Mathf.Max(verticalVelocity.y, 0f);
        else 
            verticalVelocity += (lastVelocity.y < 1f ? downwardsGravity : upwardsGravity) * Vector3.up * Time.fixedDeltaTime;

        verticalVelocity.y = Mathf.Clamp(verticalVelocity.y, -MAX_GRAVITY, Mathf.Infinity);

        if (verticalVelocity.y <= 0f && grounded)
            rb.MovePosition(rb.position + (groundData.Value.point.y - coll.bounds.min.y) * Vector3.up); //snap

        if (grounded)
        {
            moveVelocity = Vector3.zero;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, drag * Time.fixedDeltaTime);
        }
        else
        {
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, airDrag * Time.fixedDeltaTime);
        }

        if (!grounded && verticalVelocity.y <= -HEAVY_FALL_VELOCITY)
            IsHeavyFalling = true;

        velocity += verticalVelocity;
        velocity += moveVelocity;
        velocity += horizontalVelocity;

        IsGrounded = grounded;
        VerticalVelocity = verticalVelocity.y;

        Vector3 feetPosition = rb.position;
        feetPosition.y = coll.bounds.min.y;

        if (movingSurface)
            rb.MovePosition(rb.position + movingSurface.GetFinalFrameTranslation(feetPosition));

        if (Sunk)
        {
            velocity = Vector3.zero;
            moveVelocity = Vector3.zero;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = Vector3.zero;
        }

        rb.linearVelocity = velocity;
        
        //Caches ---------------------------------------------->
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

        nextFrameBounds = coll.bounds;
        nextFrameBounds.center += rb.linearVelocity * Time.fixedDeltaTime;
    }
    void Land (GroundData ground, SurfaceProperties surface)
    {
        groundData = groundChecker.GroundData;
        checkpoint = transform.position;
        surfaceProperties = surface;
        movingSurface = groundData.Value.coll.GetComponent<MovingSurface>();


        if (IsHeavyFalling)
        {
            Sink(surface, ground);
            IsHeavyFalling = false;
        }

        string sound = "StepRock";
        if (surface != null)
        {
            sound = surface.material switch
            {
                SurfaceProperties.Material.Wood => "StepWood",
                SurfaceProperties.Material.Grass => "StepGrass",
                SurfaceProperties.Material.Rock => "StepRock",
                SurfaceProperties.Material.Sand => "StepSand",
                SurfaceProperties.Material.Water => "StepWater",
                _ => "StepRock"
            };
        }

        sounds.PlaySound(sound);

        OnGroundedStart?.Invoke();
        dust.Play();
    }
    void GetHit(GameObject source, int damage, Weight weight, string tag, bool knockback = true)
    {
        if (Invulnerable) { return; }

        CurrentLife -= damage;
        Invulnerable = true;
        gotHitTimer.Set(TIME_INVULNERABLE);

        invulnerableBlinkCoroutine = 
            StartCoroutine(renderRoot.gameObject.Blink(0.05f, TIME_INVULNERABLE));

        float knockbackForce = SPEED_KNOCKBACK;
        knockbackForce *= weight switch
        {
            Weight.Light => 0.8f,
            Weight.Medium => 1f,
            Weight.Heavy => 1.2f,
            _ => 1.0f 
        };
        knockbackForce *= knockback ? 1f : 0f;  

        FrameFreeze.Freeze(0.3f, () =>
        {
            MainCameraShaker.instance.Shake(0.2f, 0.3f, 0.2f);
            if (source != null)
                horizontalVelocity += (transform.position - source.transform.position).FlattenY().normalized * knockbackForce;
        });

        OnGetHurt?.Invoke(damage);
    }
    public void OutOfBounds ()
    {
        GetHit(null, 1, Weight.Light, "Fall", false);
        transform.position = checkpoint;      
    }
    public void Sink  (SurfaceProperties surface, GroundData ground)
    {
        sunkValue = 1f;
        rb.isKinematic = true;
        directionRoot.localPosition = -SINK_HEIGHT * Vector3.up;
        dirtHole.gameObject.SetActive(true);

        sunkPosition = transform.position;
        sunkPosition.y = ground.point.y;

        dirtHole.transform.rotation = Quaternion.LookRotation(directionRoot.forward, ground.normal);
        //dirtHole.transform.position = sunkPosition;

        MainCameraShaker.instance.Shake(0.2f, 0.3f, 0.2f);
    }
    public void UnSink ()
    {
        sunkValue -= 0.3f;
        directionRoot.localPosition = -SINK_HEIGHT * sunkValue * Vector3.up;
        dirtHole.gameObject.SetActive(false);

        if (sunkValue <= 0.1f)
        {
            sunkValue = 0f;
            directionRoot.localPosition = Vector3.zero;
            rb.isKinematic = false;
            Jump(jumpImpulse * 0.5f);         
        }
        else
        {
            dirtHole.gameObject.SetActive(true);
        }
    }
    public void AddInteraction (IInteractable interactable)
    {
        Debug.Log("Added interaction:" + interactable.InteractionName);
        availableInteractions.Add(interactable);
    }
    public void RemoveInteraction (IInteractable interactable)
    {
        Debug.Log("Removed interaction:" + interactable.InteractionName);
        availableInteractions.Remove(interactable);
    }
}
