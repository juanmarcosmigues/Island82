using UnityEngine;

[DefaultExecutionOrder(GameDefinitions.EXECUTION_ORDER_PHYSICS_CALCULATIONS)]
public class JumpOn : MonoBehaviour, ITrigger
{
    public BoxCollider trigger;
    public bool onlyHeavyFall;
    public bool debug;

    public Timestamp timerJumpedOn;
    public event System.Action<JumpOn> OnJumpedOn;
    public event System.Action<ITrigger> OnTriggered;

    Bounds nextFrameBounds;
    Rigidbody rigidBody;

    void Awake ()
    {
        rigidBody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        nextFrameBounds = trigger.bounds;
        if (rigidBody != null )
            nextFrameBounds.center += rigidBody.linearVelocity * Time.fixedDeltaTime;

        if (CheckJumpOn())
        {
            JumpedOn();
        }
    }
    public virtual bool CheckJumpOn (bool ignoreIntersection = false)
    {
        return
        (ignoreIntersection || IsPlayerIntersecting()) && //Is now intersecting?
        !Player.Instance.Grounded && //Is airborne
        Player.Instance.VerticalVelocity < 3f && //Is kinda falling? 
        (onlyHeavyFall && Player.Instance.HeavyFalling || !onlyHeavyFall); //Heavy falling evaluation
    }
    bool IsPlayerIntersecting()
    {
        Bounds playerBounds = Player.Instance.coll.bounds;
        playerBounds.Encapsulate(Player.Instance.NextFrameBounds);
        Bounds triggerBounds = trigger.bounds;
        triggerBounds.Encapsulate(nextFrameBounds);

        if (triggerBounds.Intersects(playerBounds))
        {
            if (Player.Instance.coll.bounds.min.y > trigger.bounds.max.y)
                return true;
        }

        return false;
    }
    public virtual void JumpedOn ()
    {
        timerJumpedOn.Set();
        OnTriggered?.Invoke(this);
        OnJumpedOn?.Invoke(this);
    }
}
