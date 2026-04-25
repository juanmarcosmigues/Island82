using UnityEngine;
using UnityEngine.Rendering;

public class ElasticPot : Pot
{
    public float[] bounces;
    public LayerMask collisionMask;

    private int currentBounces;
    private Timestamp timerBounce;

    protected override void Awake()
    {
        base.Awake();
        Player.Instance.OnPlayerJump += Jump;
    }
    private void FixedUpdate()
    {
        if (timer.elapsed < 0.1f) return;

        RaycastHit r;
        Rigidbody rb = Player.Instance.rb;
        Vector3 velocity = rb.linearVelocity.FlattenY();
        Vector3 direction = velocity.normalized;
        float speed = velocity.magnitude + 0.1f;

        if (Physics.SphereCast(rb.position,
            Player.Instance.coll.radius,
            direction,
            out r,
            speed * Time.fixedDeltaTime,
            collisionMask))
        {

            Vector3 bounceForce =
                (Player.Instance.transform.position - r.point).normalized * 5f;

            Player.Instance.SetHorizontalForce(bounceForce);
            timerBounce.Set();
        }
    }
    protected override void PlayerIn()
    {
        base.PlayerIn();
        ResetBounces();
    }
    protected void Jump ()
    {
        if (currentBounces < 0)
            ResetBounces();
    }
    protected void ResetBounces ()
    {
        currentBounces = bounces.Length-1;
    }
    protected override void Land(Vector3 velocity, bool heavyFall)
    {
        if (heavyFall)
        {
            Break();
            return;
        }
            
        if (timer.elapsed < 0.2f) return;

        if (currentBounces >= 0)
        {
            Player.Instance.SetVerticalForce(bounces[currentBounces]);
            currentBounces--;    
        }
        else
        {
            RecieveDamage();
        }
    }
}
