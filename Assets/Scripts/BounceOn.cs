using UnityEngine;

public class BounceOn : MonoBehaviour
{
    public float jumpImpulse;
    public BoxCollider trigger;
    public BoxCollider coll;
    public Animation anim;

    public event System.Action<BounceOn> OnBounce;
    private void FixedUpdate()
    {
        if ((coll.bounds.max.y - Player.Instance.coll.bounds.min.y) <= 0.01f &&
            Player.Instance.VerticalVelocity < 1f &&
            trigger.bounds.Intersects(Player.Instance.coll.bounds))
        {
            Bounce();
        }
    }

    public void Bounce ()
    {
        Player.Instance.Jump(jumpImpulse, true);
        anim?.Play();

        OnBounce?.Invoke(this);
    }
}
