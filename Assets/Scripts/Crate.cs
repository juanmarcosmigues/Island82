using UnityEngine;

public class Crate : MonoBehaviour
{
    public float jumpImpulse;
    public BoxCollider trigger;
    public BoxCollider coll;
    public Animation anim;

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

        broken = true;

        enabled = false;
    }
}
