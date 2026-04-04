using Unity.VisualScripting;
using UnityEngine;
using static CombatHandler;

public class Crate : MonoBehaviour
{
    public BounceOn bounceOn;
    public BoxCollider coll;
    public Animation anim;
    public CombatHandler combatHandler;
    [Range(0f, 1f)]
    public float dropChance;
    public string drop;

    bool broken;

    private void Awake()
    {
        broken = false;
        bounceOn.OnBounce += b => BreakCrate();
        combatHandler.OnGetHit += GetHit;
    }
    void BreakCrate ()
    {
        if (broken) return;

        bounceOn.enabled = false;
        bounceOn.trigger.enabled = false;
        coll.enabled = false;
        anim.Play();

        if (Random.value <= dropChance && drop.Length > 0) 
        {
            PoolManager.Instance.GetPool<ObjectPool>(drop).GetObject().transform.position = transform.position;
        }

        broken = true;

        enabled = false;
    }
    void GetHit(GameObject source, int damage, Weight weight, string tag, bool knockback = true)
    {
        BreakCrate();
    }
}
