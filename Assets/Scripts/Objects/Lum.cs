using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class Lum : MonoBehaviour
{
    private const float MAX_GRAVITY = -12f;
    private const float GRAVITY = -6f;
    private const float MAGNETIC = 5f;
    private const float DRAG = 0.5f;
    private const float MAX_BOUNCE = 3f;
    private const float RANDOM_MAGNETIC = 0.3f;

    public Transform renderRoot;
    public bool fromDrop = false;
    public LayerMask groundMask;

    public event System.Action<Lum> OnPickUp;

    private Rigidbody rb;
    private Collider coll;

    private bool magnetic;
    private float magneticTimeStart;
    private float gravityInfluence;
    private float magneticInfluence;
    private Timestamp timerAlive;
    private Timestamp timerMagnetic;
    private int bounces = 3;
    private bool bounceConsumed;
    private Vector3 verticalVelocity;
    private Vector3 horizontalVelocity;
    private Vector3 followDirection;
    private Vector3 deltaToPlayer;
    private float distanceSqrToPlayer;
    private float sign;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
    }
    private void Start()
    {  
    }
    private void OnEnable()
    {
        timerAlive.Set();
        magnetic = false;
        gravityInfluence = 1f;
        magneticInfluence = 0f;
        bounces = 3;
        bounceConsumed = false;
        horizontalVelocity = Vector3.zero;
        verticalVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        coll.enabled = true;
        magneticTimeStart = 0.7f + Random.Range(-RANDOM_MAGNETIC, RANDOM_MAGNETIC);
    }
    private void Update()
    {
        if (!magnetic && timerAlive.elapsed > magneticTimeStart) 
        {
            sign = Mathf.Sign(Random.Range(-1f, 1));
            magnetic = true;
            coll.enabled = false;
            timerMagnetic.Set();
            followDirection = Quaternion.AngleAxis(sign * 20f, Vector3.up) * -deltaToPlayer.FlattenY();
            verticalVelocity = Vector3.up * 2f;       
        }
    }
    private void FixedUpdate()
    {
        deltaToPlayer = Player.Instance.transform.position - transform.position;
        distanceSqrToPlayer = deltaToPlayer.sqrMagnitude;
        Vector3 flatDelta = deltaToPlayer.FlattenY();

        if ((flatDelta.sqrMagnitude < 0.17f && Mathf.Abs(deltaToPlayer.y) < 0.7f) && timerAlive.elapsed > 0.5f)
        {
            gameObject.SetActive(false);
            Player.Instance.GetCoin();
            PoolManager.Instance.GetPool<VFXPool>("VFXGetCoin").GetObject().transform.position
                = Player.Instance.transform.position;
            OnPickUp?.Invoke(this);
        }

        PhysicsStep();

        if (magnetic)
            MagneticStep();
    }

    void MagneticStep ()
    {
        gravityInfluence = Mathf.Clamp01(gravityInfluence - Time.fixedDeltaTime);
        magneticInfluence = magneticInfluence + Time.fixedDeltaTime;
        float magneticInfluenceSqr = magneticInfluence * magneticInfluence;

        verticalVelocity.y = Mathf.MoveTowards(verticalVelocity.y, Mathf.Sign(deltaToPlayer.y) * MAGNETIC, magneticInfluence * 0.5f);

        Vector3 goalDirection = Quaternion.AngleAxis(sign * Mathf.Sin(magneticInfluence) * 45f * Mathf.Clamp01(2 - magneticInfluence), Vector3.up) * deltaToPlayer.FlattenY();
        followDirection = Vector3.RotateTowards
            (followDirection.normalized,
            goalDirection.normalized,
            magneticInfluenceSqr * 0.2f, magneticInfluence);

        horizontalVelocity = followDirection * Mathf.Clamp01(magneticInfluenceSqr * 0.5f) * MAGNETIC;

        ////Fixed vertical move
        //Vector3 goalPos = rb.position;
        //goalPos.y = Player.Instance.transform.position.y;
        //rb.MovePosition(Vector3.Lerp(rb.position, goalPos, magneticInfluence * 0.05f));
    }
    void PhysicsStep ()
    {
        if (bounces > 0 && verticalVelocity.y < 0f)
        {
            if (Physics.Raycast(transform.position, Vector3.down, 0.15f, groundMask) && !bounceConsumed)
            {
                bounceConsumed = true;
                verticalVelocity = Vector3.up * Mathf.Min(Mathf.Abs(verticalVelocity.y), MAX_BOUNCE) * (bounces / 3f);
                bounces--;
            }
            else
            {
                bounceConsumed = false;
            }
        }

        verticalVelocity += Vector3.up * GRAVITY * Time.fixedDeltaTime * gravityInfluence;
        verticalVelocity.y = Mathf.Max(verticalVelocity.y, MAX_GRAVITY);

        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, DRAG * Time.fixedDeltaTime);

        rb.linearVelocity = verticalVelocity + horizontalVelocity;
    }
    public void SetVelocity (Vector3 velocity)
    {
        verticalVelocity.y = velocity.y;
        horizontalVelocity.x = velocity.x;
        horizontalVelocity.z = velocity.z;
    }
}
