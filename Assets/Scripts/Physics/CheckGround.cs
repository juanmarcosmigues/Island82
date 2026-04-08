using UnityEngine;

public class CheckGround : MonoBehaviour
{
    public Collider coll;
    public LayerMask groundMask;
    public float maxGroundAngle = 60f;
    public bool autoCheck;
    public bool debug;

    protected bool grounded;
    protected GroundData groundData;

    public float MaxGroundDot { get; protected set; }
    public bool Grounded => grounded;
    public GroundData GroundData => groundData;

    public System.Action<GroundData> OnGroundedStart;
    public System.Action<GroundData> OnAirborneStart;

    protected virtual void Awake()
    {
#if UNITY_EDITOR
        if (debug)
        {
            OnAirborneStart += (gd) => Debug.Log("Now Airborne");
            OnGroundedStart += (gd) => Debug.Log("Now Grounded");
        }
#endif

        MaxGroundDot = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }
    protected virtual void FixedUpdate ()
    {
        if (autoCheck)
            Check();
    }
    public virtual bool Check(Vector3 velocity = default) 
    {
        grounded = false;
        return grounded;
    }
}
