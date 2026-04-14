using UnityEngine;

public class JumpOn : MonoBehaviour, ITrigger
{
    public BoxCollider trigger;
    public bool onlyHeavyFall;
    public bool debug;

    public Timestamp timerJumpedOn;
    public event System.Action<JumpOn> OnJumpedOn;
    public event System.Action<ITrigger> OnTriggered;

    private void FixedUpdate()
    {
        if (CheckJumpOn())
        {
            JumpedOn();
        }
    }
    public virtual bool CheckJumpOn (bool ignoreIntersection = false)
    {
        if (debug)
        {
            bool playerIntersecting = IsPlayerIntersecting() || ignoreIntersection;
            bool playerInTheAir = !Player.Instance.IsGrounded;
            bool playerVerticalVelocity = Player.Instance.VerticalVelocity < 1f;
            bool playerWasOnTop = Player.Instance.MaxAirBounds.HasValue && (Player.Instance.MaxAirBounds.Value.min.y - trigger.bounds.max.y) > -0.1f;
            bool playerOnHeavyFall = (onlyHeavyFall && Player.Instance.IsHeavyFalling || !onlyHeavyFall);

            if (IsPlayerIntersecting())
            {
                Debug.Log("Player in the air: " + playerInTheAir + "\n" +
                    "Player falling: " + playerVerticalVelocity + "\n" +
                    "Player was on Top: " + playerWasOnTop + "\n" +
                    "Player heavy fall: " + playerOnHeavyFall);
            }
        }

        return
        (ignoreIntersection || IsPlayerIntersecting()) && //Is now intersecting?
        !Player.Instance.IsGrounded && //Is airborne
        Player.Instance.VerticalVelocity < 1f && //Is kinda falling?
        (Player.Instance.MaxAirBounds.Value.min.y - trigger.bounds.max.y) > -0.1f && //Was on top at some previous frame? 
        (onlyHeavyFall && Player.Instance.IsHeavyFalling || !onlyHeavyFall); //Heavy falling evaluation
    }
    bool IsPlayerIntersecting()
    {
        Bounds swept = Player.Instance.coll.bounds;
        swept.Encapsulate(Player.Instance.NextFrameBounds);
        return trigger.bounds.Intersects(swept);
    }
    public virtual void JumpedOn ()
    {
        timerJumpedOn.Set();
        OnTriggered?.Invoke(this);
        OnJumpedOn?.Invoke(this);
    }
}
