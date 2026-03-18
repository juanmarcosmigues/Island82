using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float trackSpeed;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        Vector3 deltaToPlayer = Player.Instance.transform.position - rb.position;
        if (trackSpeed > 0)
            transform.forward = Vector3.RotateTowards(transform.forward, deltaToPlayer.FlattenY().normalized, trackSpeed * Time.fixedDeltaTime, 100f);
        rb.linearVelocity = transform.forward * speed;
    }
}
