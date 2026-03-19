using System.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float trackSpeed;
    public int life = 3;
    public TriggerEventHandler hitbox;
    public BoxCollider hurtbox;
    public ParticleSystem vfxDustTrail;

    private Entity me;
    private Entity source;
    private Rigidbody rb;
    private BounceOn bounceOn;
    private static LayerMask solidMask;

    private int currentLife;
    private bool dead;
    private Coroutine hitPlayerCoroutine;
    private Timestamp bouncedTimer;
    private Timestamp spawnedTimer;
    public void SetSource(Entity source) => this.source = source;

    private void Awake()
    {
        me = GetComponent<Entity>();
        rb = GetComponent<Rigidbody>();
        bounceOn = GetComponent<BounceOn>();
    }
    private void Start()
    {
        if (solidMask == default(LayerMask))
            solidMask = LayerMask.GetMask(
                GameDefinitions.LAYER_STATICSOLID, 
                GameDefinitions.LAYER_DYNAMICSOLID);

        bounceOn.OnBounce += _ => GetBounced();
        hitbox.OnTriggerEntered += c => HitSomething(c);
    }
    private void OnEnable()
    {
        StopAllCoroutines();

        dead = false;
        currentLife = life;
        hitPlayerCoroutine = null;

        hitbox.gameObject.SetActive(true);
        hurtbox.gameObject.SetActive(true);
        bounceOn.enabled = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localScale = Vector3.one;

        spawnedTimer.Set();
    }
    private void FixedUpdate()
    {
        if (dead) return;

        Vector3 deltaToPlayer = Player.Instance.transform.position - rb.position;
        if (trackSpeed > 0)
            transform.forward = Vector3.RotateTowards(transform.forward, deltaToPlayer.FlattenY().normalized, trackSpeed * Time.fixedDeltaTime, 100f);
        rb.linearVelocity = transform.forward * speed;
    }
    private void HitSomething (Collider something)
    {
        if (dead) return;

        if (solidMask.ContainsLayer(something.gameObject.layer))
        {
            Die();
        }
        else
        {
            Entity entity = something.gameObject.GetComponentInParent<Entity>();
            if (entity == null) return;
            if (entity == source && spawnedTimer.elapsed < 0.1f) return; //Avoid hitting itself on launch

            if (entity.type == Entity.Type.Player)
            {
                if (hitPlayerCoroutine == null)
                    hitPlayerCoroutine = StartCoroutine(HitPlayerEvaluation());
            }
            else
            {
                var combatHandler = entity.GetComponent<CombatHandler>();
                if (combatHandler != null)
                    combatHandler.GetHit(hitbox.gameObject, 1, CombatHandler.Weight.Heavy, "Bullet");

                Die();
            }
        }           
    }
    private IEnumerator HitPlayerEvaluation ()
    {
        if (Player.Instance.Invulnerable) yield break;
        yield return new WaitForFixedUpdate();
        if (bouncedTimer.remainingTime > 0) yield break; //Wait a frame and favour bounce instead of hit

        var combatHandler = Player.Instance.GetComponent<CombatHandler>();
        combatHandler.GetHit(hitbox.gameObject, 1, CombatHandler.Weight.Heavy, "Bullet");

        Die();
        hitPlayerCoroutine = null;
    }
    private void Die ()
    {
        dead = true;
        currentLife = 0;

        vfxDustTrail.Stop();
        hitbox.gameObject.SetActive(false);
        hurtbox.gameObject.SetActive(false);
        bounceOn.enabled = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.up * 3f;
        rb.angularVelocity = Vector3.Cross(transform.forward, Vector3.up) * 10f;

        StartCoroutine(gameObject.DisableAfter(2f));
    }
    private void GetBounced ()
    {
        currentLife--;
        if (currentLife <= 0)
        {
            Die();
        }
        bouncedTimer.Set(0.1f);
    }
}
