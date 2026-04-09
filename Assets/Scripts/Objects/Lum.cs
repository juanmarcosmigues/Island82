using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class Lum : MonoBehaviour
{
    private const float MAX_GRAVITY = -5f;
    private const float GRAVITY = -2f;
    private const float MAGNETIC = 7f;
    private const float DRAG = 6f;
    private const float RANDOM_MAGNETIC = 0.4f;

    public Transform renderRoot;
    public bool fromDrop = false;
    public LayerMask bounceMask;

    public event System.Action<Lum> OnPickUp;

    private Rigidbody rb;
    private Collider coll;

    private bool magnetic;
    private float magneticTimeStart;
    private float gravityInfluence;
    private float magneticInfluence;
    private Timestamp timerAlive;
    private int bounces;

    private Vector3 dropVelocity;
    private Vector3 magneticVelocity;
    private Vector3 verticalVelocity;
    private Vector3 bounceVelocity;

    private Vector3 followDirection;
    private Vector3 deltaToPlayer;

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
        dropVelocity = Vector3.zero;
        magneticVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        coll.enabled = true;
        magneticTimeStart = 0.6f + Random.Range(-RANDOM_MAGNETIC, RANDOM_MAGNETIC);
        bounces = 3;
    }
    private void Update()
    {
        if (!magnetic && timerAlive.elapsed > magneticTimeStart) 
        {
            magnetic = true;
            coll.enabled = false;
            followDirection = deltaToPlayer.FlattenY();
            verticalVelocity.y = 2f;
        }
    }
    private void FixedUpdate()
    {
        deltaToPlayer = Player.Instance.transform.position - transform.position;
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
    }

    void PhysicsStep ()
    {
        if (magnetic)
        {
            gravityInfluence = Mathf.Clamp01(gravityInfluence - Time.fixedDeltaTime);
            magneticInfluence = magneticInfluence + Time.fixedDeltaTime;
        }

        float magneticInfluenceSqr = magneticInfluence * magneticInfluence;

        followDirection = Vector3.RotateTowards
            (followDirection.normalized,
            deltaToPlayer.normalized,
            magneticInfluenceSqr * 0.2f, magneticInfluence);

        magneticVelocity = followDirection * Mathf.Clamp01(magneticInfluenceSqr * 0.5f) * MAGNETIC;

        verticalVelocity += Vector3.up * GRAVITY * Time.fixedDeltaTime * gravityInfluence;
        verticalVelocity.y = Mathf.Max(verticalVelocity.y, MAX_GRAVITY);

        dropVelocity = Vector3.MoveTowards(dropVelocity, Vector3.zero, DRAG * Time.fixedDeltaTime);
        bounceVelocity = Vector3.MoveTowards(bounceVelocity, Vector3.zero, DRAG * Time.fixedDeltaTime);

        rb.linearVelocity = dropVelocity + magneticVelocity + verticalVelocity + bounceVelocity;

        //if (bounces > 0)
        //{
        //    RaycastHit hit;
        //    Vector3 frameVelocity = rb.linearVelocity * Time.fixedDeltaTime;
        //    if (Physics.SphereCast(rb.position, 0.2f, frameVelocity.normalized, out hit, frameVelocity.magnitude, bounceMask))
        //    {
        //        Vector3 reflection = Vector3.Reflect(rb.linearVelocity, hit.normal);
        //        verticalVelocity.y = 0f;
        //        dropVelocity.y = 0f;
        //        bounceVelocity = reflection.FlattenY();

        //        bounces--;
        //    }
        //}
    }
    public void SetVelocity (Vector3 velocity)
    {
        dropVelocity = velocity;
    }
}
