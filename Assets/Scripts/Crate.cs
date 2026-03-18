using UnityEngine;

public class Crate : MonoBehaviour
{
    public float jumpImpulse;
    public BoxCollider trigger;
    public BoxCollider coll;
    public Animation anim;
    [Range(0f, 1f)]
    public float dropChance;
    public string drop;

    bool broken;

    private void Awake()
    {
        trigger.isTrigger = true;
        broken = false;
    }
    private void FixedUpdate()
    {
        if ((coll.bounds.max.y - Player.Instance.coll.bounds.min.y) <= 0.01f  && 
            Player.Instance.VerticalVelocity < 1f && 
            trigger.bounds.Intersects(Player.Instance.coll.bounds))
        {
            BreakCrate();
        }
    }
    void BreakCrate ()
    {
        if (broken) return;

        coll.enabled = false;
        Player.Instance.Jump(jumpImpulse, true);
        anim.Play();

        if (Random.value <= dropChance && drop.Length > 0) 
        {
            PoolManager.Instance.GetPool<ObjectPool>(drop).GetObject().transform.position = transform.position;
        }

        broken = true;

        enabled = false;
    }
}
