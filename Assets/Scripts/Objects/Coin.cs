using UnityEngine;

public class Coin : MonoBehaviour
{
    public float magneticRadius = 3f;
    public Transform renderRoot;
    public bool fromDrop = false;

    public event System.Action<Coin> OnPickUp;

    private Rigidbody rb;
    private Collider coll;
    private float magneticRadiusSqr;

    private bool magnetic;
    private float magneticTime;
    private float magneticForce;
    private Timestamp timerAlive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
    }
    private void Start()
    {
        magneticRadiusSqr = magneticRadius * magneticRadius;    
    }
    private void OnEnable()
    {
        timerAlive.Set();
        magnetic = false;
        magneticForce = 0f;
        magneticTime = 0f;
        rb.useGravity = true;
        coll.enabled = true;
    }
    private void Update()
    {
        if (fromDrop && timerAlive.elapsed < 1f) return;

        if (!magnetic) 
        { 
            float distance = (Player.Instance.transform.position - transform.position).sqrMagnitude;
            if (distance < magneticRadiusSqr)
            {
                magnetic = true;
                coll.enabled = false;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
    private void FixedUpdate()
    {
        if (magnetic)
        {
            float distance = (Player.Instance.transform.position - transform.position).sqrMagnitude;

            magneticTime += Time.fixedDeltaTime * 2.5f;
            float height = Mathf.Sin(Mathf.Clamp01(magneticTime) * Mathf.PI) * 0.5f;
            Vector3 newPos = Vector3.MoveTowards(transform.position, Player.Instance.transform.position - Vector3.up * 0.2f, magneticForce * Time.fixedDeltaTime);
            renderRoot.localPosition = Vector3.up * height;

            rb.MovePosition(newPos);
            magneticForce += Time.fixedDeltaTime * 10f;

            if (distance < 0.15f)
            {
                gameObject.SetActive(false);
                Player.Instance.GetCoin();
                PoolManager.Instance.GetPool<VFXPool>("VFXGetCoin").GetObject().transform.position
                    =Player.Instance.transform.position;
                OnPickUp?.Invoke(this);
            }
        }
    }
    public void SetVelocity (Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }
}
