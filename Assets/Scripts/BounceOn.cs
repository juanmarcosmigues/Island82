using UnityEngine;

public class BounceOn : MonoBehaviour
{
    public float jumpImpulse;
    public BoxCollider trigger;
    public Animation anim;
    public VisualFeedback vfb;

    public event System.Action<BounceOn> OnBounce;
    private void FixedUpdate()
    {
        if (trigger.bounds.Intersects(Player.Instance.coll.bounds) &&//Is now intersecting?
            !Player.Instance.IsGrounded && //Is airborne
            Player.Instance.VerticalVelocity < 1f && //Is kinda falling?
            (Player.Instance.MaxAirBounds.Value.min.y - trigger.bounds.max.y) > -0.1f) //Was on top at some previous frame? 
        {
            Bounce();
        }
    }

    public void Bounce ()
    {
        Player.Instance.Jump(jumpImpulse, true);

        if (anim != null)
            anim.Play();
        if (vfb != null)
            vfb.Play();

        OnBounce?.Invoke(this);
    }
}
