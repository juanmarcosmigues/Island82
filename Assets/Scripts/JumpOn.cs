using UnityEngine;

public class JumpOn : MonoBehaviour
{
    public BoxCollider trigger;

    public event System.Action<JumpOn> OnJumpedOn;

    private void FixedUpdate()
    {
        if (trigger.bounds.Intersects(Player.Instance.coll.bounds) &&//Is now intersecting?
            !Player.Instance.IsGrounded && //Is airborne
            Player.Instance.VerticalVelocity < 1f && //Is kinda falling?
            (Player.Instance.MaxAirBounds.Value.min.y - trigger.bounds.max.y) > -0.1f) //Was on top at some previous frame? 
        {
            JumpedOn();
        }
    }
    public virtual void JumpedOn ()
    {
        OnJumpedOn?.Invoke(this);
    }
}
