using System.Collections;
using UnityEngine;
using static CombatHandler;

public class EnemyTurret : MonoBehaviour
{
    public float shootFrequency = 3f;
    public Animation anim;
    public ParticleSystem vfxShoot;
    public Transform spawnBulletPoint;
    public Transform destroyedParts;
    public Transform renderRoot;

    private Entity me;
    private BounceOn bounceOn;
    private BoxCollider coll;
    private CombatHandler combatHandler;

    private Timestamp shootTimer;
    private Vector3 deltaToPlayer;
    private bool dead;
    private void Awake()
    {
        me = GetComponent<Entity>();
        coll = GetComponent<BoxCollider>();
        bounceOn = GetComponent<BounceOn>();
        combatHandler = GetComponent<CombatHandler>();
    }
    private void Start()
    {
        bounceOn.OnBounce += e => Die();
        combatHandler.OnGetHit += GetHit;

        shootTimer.Set();
    }
    private void Update()
    {
        if (shootTimer.elapsed > shootFrequency)
            Shoot();
    }
    private void FixedUpdate()
    {
        deltaToPlayer = Player.Instance.transform.position - transform.position;

        if (deltaToPlayer.FlattenY().sqrMagnitude > 2f)
            LookAtPlayer();
    }
    public void Shoot ()
    {
        anim.Play();
        vfxShoot.Play();
        PoolManager.Instance.GetPool<ObjectPool>("Bullet")
            .GetObject(spawnBulletPoint.position, Quaternion.LookRotation(renderRoot.forward, Vector3.up))
            .GetComponent<Bullet>().SetSource(me);
        shootTimer.Set();
    }
    public void LookAtPlayer ()
    {
        renderRoot.forward = GameWorld.LookAtDirection(deltaToPlayer.FlattenY().normalized);
    }
    public void Die (bool delay = true)
    {
        if (dead) return;
        dead = true;

        bounceOn.enabled = false;
        coll.enabled = false;

        StartCoroutine(DieAnimation(delay));
    }
    IEnumerator DieAnimation(bool delay)
    {
        if (delay) 
            yield return new WaitForSeconds(0.08f);

        destroyedParts.SetParent(null);
        destroyedParts.gameObject.SetActive(true);
        PoolManager.Instance.GetPool<VFXPool>("VFXDustExplosionFlat").GetObject().transform.position = transform.position;
        gameObject.SetActive(false);

        yield break;
    }
    void GetHit(GameObject source, int damage, Weight weight, string tag)
    {
        Die(false);
    }
}
