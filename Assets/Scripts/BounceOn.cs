using UnityEngine;

public class BounceOn : JumpOn
{
    public float jumpImpulse;
    public bool dontBounceOnHeavyFall;
    
    public Animation anim;
    public VisualFeedback vfb;

    public event System.Action<BounceOn> OnBounce;
    public override void JumpedOn()
    {
        base.JumpedOn();
        Bounce();
    }

    public void Bounce ()
    {
        if (!Player.Instance.IsHeavyFalling || dontBounceOnHeavyFall)
            Player.Instance.Jump(jumpImpulse, true);

        if (anim != null)
            anim.Play();
        if (vfb != null)
            vfb.Play();

        OnBounce?.Invoke(this);
    }
}
