using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CombatHandler;

[DefaultExecutionOrder(GameDefinitions.EXECUTION_ORDER_PLAYER)]
public class Player : MonoBehaviour, IDynamicObject
{
    private const float TIME_INVULNERABLE = 3f;
    private const float SPEED_KNOCKBACK = 7f;
    private const float MAX_GRAVITY = 16f;
    public const float HEAVY_FALL_VELOCITY = 16f;
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

    public event System.Action OnGroundedStart;

    private List<IInteractable> availableInteractions = new();

    public bool PlayerRotation {  get; set; } = true;
    public GatedBool PlayerInControl { get; } = new();
    public int CurrentLife { get; private set; } = 5;
    public bool Invulnerable { get; private set; } = false;
    public Vector3 LookingDirection => directionRoot.forward;
    public Bounds NextFrameBounds => nextFrameBounds;
    public float VerticalVelocity => verticalVelocity.y;
    public bool IsGrounded => grounded;
    public bool IsHeavyFalling => heavyFalling;
    public bool Sunk => sunkValue > 0f;
    public Screw Screw => currentScrew;

    Timestamp gotHitTimer;
    Coroutine invulnerableBlinkCoroutine;

    Vector3 lookDirection;
    Timestamp pressedJumpTimer;

    float currentJumpForce;
    Vector3 lastVelocity;
    bool grounded;
    bool heavyFalling;
    float jumpValue;
    Vector3 checkpoint;
    MovingSurface movingSurface;
    SurfaceProperties surfaceProperties;
    GroundData? groundData;
    float sunkValue;
    Vector3 sunkPosition;
    Bounds nextFrameBounds;
    float bounceModifier;

    Vector3 moveVelocity;
    Vector3 verticalVelocity;
    Vector3 horizontalVelocity;

    float pickUpCoinCombo;

    Screw currentScrew;

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

    #region Controls
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
        if (!PlayerInControl.True) return;

        if (currentScrew != null)
        {
            ScrewOut();
            Jump(jumpImpulse);
            return;
        }

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
        if (!PlayerInControl.True) return;
        if (Sunk) return;

        Vector3 dir = Camera.main.RotateTowardsCamera(input).normalized;

        if (currentScrew == null)
        {
            if (PlayerRotation) LookAt(dir);
        }
        else
            currentScrew.PlayerRotationInput(dir, val);

        if (val > 0.1f) Move(dir, val);
        else Move(dir, 0f);
    }
    public void LookAt (Vector3 dir)
    {
        lookDirection = dir;       
    }
    public void Move (Vector3 dir, float factor)
    {
        moveVelocity = dir * factor * moveSpeed;
    }
    public void Jump() => Jump(jumpImpulse);
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

    #endregion

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
            pickUpCoinCombo = Mathf.Clamp01(pickUpCoinCombo - Time.deltaTime * 0.5f);

        Shader.SetGlobalVector("_PlayerPosition", transform.position);
    }
    private void FixedUpdate()
    {
        if (rb.isKinematic)
        {
            moveVelocity = Vector3.zero;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = Vector3.zero;
            return;
        }
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
                    Land(groundChecker.GroundData, surface, lastVelocity);
                else if (surface != null)
                    newGrounded = false;
            }
            else
            {
                if (surfaceProperties != null)
                    surfaceProperties.Leave(lastVelocity);

                surfaceProperties = null;
            }
        }
        else if (newGrounded)
        {
            if (groundChecker.GroundData.coll != groundData.Value.coll)
            {
                surfaceProperties = groundChecker.GroundData.coll.GetComponent<SurfaceProperties>();
                movingSurface = groundChecker.GroundData.coll.GetComponent<MovingSurface>();
            }
        }
        grounded = newGrounded;
        groundData = groundChecker.GroundData;

        //Velocity Step ----------------------------->
        Vector3 velocity = Vector3.zero;
        Vector3 addedPosition = Vector3.zero;

        bounceModifier = Mathf.Clamp01(bounceModifier - Time.fixedDeltaTime);

        if (grounded)
        {
            verticalVelocity.y = Mathf.Max(verticalVelocity.y, 0f);
        }
        else
        {
            verticalVelocity +=
                (lastVelocity.y < 1f ?
                downwardsGravity :
                upwardsGravity * (1 - bounceModifier))
                * Vector3.up * Time.fixedDeltaTime;
        }

        verticalVelocity.y = Mathf.Clamp(verticalVelocity.y, -MAX_GRAVITY, Mathf.Infinity);

        if (grounded)
        {
            moveVelocity = Vector3.zero;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, drag * Time.fixedDeltaTime);
        }
        else
        {
            moveVelocity *= 1 + (bounceModifier * 0.6f);
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, airDrag * Time.fixedDeltaTime);
        }

        if (!grounded && verticalVelocity.y <= -HEAVY_FALL_VELOCITY)
            heavyFalling = true;

        velocity += verticalVelocity;
        velocity += moveVelocity;
        velocity += horizontalVelocity;

        Vector3 nextPosition = rb.position + velocity * Time.fixedDeltaTime;
        Vector3 nextFeetPosition = nextPosition - coll.bounds.extents.y * Vector3.up;
        if (grounded)
        {
            //Ground Snap
            if (verticalVelocity.y <= 0f)
                addedPosition += (groundData.Value.point.y - nextFeetPosition.y) * Vector3.up;
            if (movingSurface)
                addedPosition += movingSurface.GetFinalFrameTranslation(transform.position);
        }

        velocity += addedPosition / Time.fixedDeltaTime;

        rb.linearVelocity = velocity;

        //Caches ---------------------------------------------->
        lastVelocity = velocity;

        nextFrameBounds = coll.bounds;
        nextFrameBounds.center += rb.linearVelocity * Time.fixedDeltaTime;
    }

    #region Reactions

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

        GameplayManager.Instance?.PlayerHurt(this, damage);
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
    public void BounceOff()
    {
        bool perfectBounce = pressedJumpTimer.remainingTime > 0f;
        bounceModifier = perfectBounce ? 0.7f : 0.5f;
        float force = perfectBounce ? 13f : 11f;
        Jump(force, true);

        if (perfectBounce)
            StartCoroutine(PerfectBounceAnimation(1f));
    }
    public void GetCoin()
    {
        vfGetCoin.Play();
        float pitch = Mathf.Lerp(0.9f, 1.2f, Mathf.Clamp01(pickUpCoinCombo));
        sounds.PlaySound("PickUp", 0.5f);
        pickUpCoinCombo += 0.15f;

        GameplayManager.Instance?.AddCoins(1);
    }
    public void InsideObject(bool inside)
    {
        meshPlayer.SetActive(!inside);
        meshPlayerHead.SetActive(inside);
    }
    public void ScrewIn (Screw screw)
    {
        rb.isKinematic = true;
        currentScrew = screw;
        renderRoot.localPosition = Vector3.up * -0.2f;
        GetComponentInChildren<ObjectShadow>(true).gameObject.SetActive(false);
    }
    public void ScrewOut ()
    {
        rb.isKinematic = false;
        currentScrew = null;
        renderRoot.localPosition = Vector3.zero;
        GetComponentInChildren<ObjectShadow>(true).gameObject.SetActive(true);
    }

    #endregion

    public void AddInteraction (IInteractable interactable)
    {
        //Debug.Log("Added interaction:" + interactable.InteractionName);
        availableInteractions.Add(interactable);
    }
    public void RemoveInteraction (IInteractable interactable)
    {
        //Debug.Log("Removed interaction:" + interactable.InteractionName);
        availableInteractions.Remove(interactable);
    }
    void Land(GroundData ground, SurfaceProperties surface, Vector3 velocity)
    {
        //Case when the surface change without jumping for some reason.
        if (surfaceProperties != surface && surfaceProperties != null)
        {
            surfaceProperties.Clear();
        }

        groundData = groundChecker.GroundData;
        checkpoint = transform.position;
        surfaceProperties = surface;
        movingSurface = groundData.Value.coll.GetComponent<MovingSurface>();


        if (heavyFalling)
        {
            Sink(surface, ground);
            heavyFalling = false;
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

            surface.Landed(velocity);
        }

        sounds.PlaySound(sound);

        OnGroundedStart?.Invoke();
        dust.Play();
    }
    IEnumerator PerfectBounceAnimation (float duration)
    {
        PlayerRotation = false;
        float t = 0f;
        float val = 0f;
        Vector3 startRotation = lookDirection;

        yield return null;

        while (t < 1f && !grounded)
        {
            t = Mathf.Clamp01(t += Time.deltaTime/duration);
            val = 1-Mathf.Pow(1-t, 2);

            lookDirection = Quaternion.AngleAxis(val * 720f, Vector3.up) * startRotation;

            yield return null;
        }

        lookDirection = startRotation;
        PlayerRotation = true;
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        DrawArrow(origin, horizontalVelocity, Color.red);    // Horizontal (knockback / external)
        DrawArrow(origin, moveVelocity, Color.green);  // Move (player input)
        DrawArrow(origin, verticalVelocity, Color.blue);   // Vertical (gravity / jump)
        DrawArrow(origin, lastVelocity, Color.white);   // Vertical (gravity / jump)
    }
    private void DrawArrow(Vector3 origin, Vector3 vector, Color color, float headSize = 0.25f, float headAngle = 20f)
    {
        if (vector.sqrMagnitude < 0.0001f) return;

        Gizmos.color = color;
        Vector3 tip = origin + vector;
        Gizmos.DrawLine(origin, tip);

        Quaternion rot = Quaternion.LookRotation(vector.normalized);
        Vector3 right = rot * Quaternion.Euler(0f, 180f + headAngle, 0f) * Vector3.forward;
        Vector3 left = rot * Quaternion.Euler(0f, 180f - headAngle, 0f) * Vector3.forward;
        Vector3 up = rot * Quaternion.Euler(180f + headAngle, 0f, 0f) * Vector3.forward;
        Vector3 down = rot * Quaternion.Euler(180f - headAngle, 0f, 0f) * Vector3.forward;

        Gizmos.DrawLine(tip, tip + right * headSize);
        Gizmos.DrawLine(tip, tip + left * headSize);
        Gizmos.DrawLine(tip, tip + up * headSize);
        Gizmos.DrawLine(tip, tip + down * headSize);
    }
}
