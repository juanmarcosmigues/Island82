using UnityEngine;
using System.Collections;

public class EnemySkullSpider : MonoBehaviour
{
    public Transform dieVFXs;
    public LocomotionPath path;
    public float patrolThreshold;
    [Range(0f, 1f)]
    public float dropChance;
    public string drop;

    private Locomotion locomotion;
    private JumpOn jumpOn;
    private Patrol patrol;
    private EnemyHitbox hitbox;
    private ObjectSounds sounds;

    Vector3 lastMoveDirection;
    bool dead = false;

    private void Awake()
    {
        locomotion = GetComponent<Locomotion>();
        patrol = new Patrol(path, patrolThreshold);
        jumpOn = GetComponent<JumpOn>();
        hitbox = GetComponentInChildren<EnemyHitbox>();
        sounds = GetComponentInChildren<ObjectSounds>();

        jumpOn.OnJumpedOn += _ => Die(true);
    }

    private void Start()
    {
        patrol.SetNextPointByPosition(transform.position);
    }

    private void Update()
    {
        Vector3 moveDirection = patrol.GetTargetDirection(transform.position);
        float factor = Mathf.Clamp01(Mathf.Max(Vector3.Dot(transform.forward, moveDirection.normalized) + 0.5f, 0.1f));
        locomotion.Move(moveDirection, factor);
    }
    public void Step ()
    {
        sounds.PlaySound("Step", 0.2f, Random.Range(0.9f, 1.1f), 5f);
    }
    public void Die(bool delay = true)
    {
        if (dead) return;
        dead = true;

        jumpOn.enabled = false;
        hitbox.gameObject.SetActive(false);

        StartCoroutine(DieAnimation(delay));
    }
    IEnumerator DieAnimation(bool delay)
    {
        sounds.PlaySound("DieCry");
        sounds.PlaySound("DieSFX");

        if (delay)
            yield return new WaitForSeconds(0.08f);

        dieVFXs.SetParent(null);
        dieVFXs.gameObject.SetActive(true);
        gameObject.SetActive(false);

        if (Random.value <= dropChance && drop.Length > 0)
        {
            PoolManager.Instance.GetPool<ObjectPool>(drop).GetObject().transform.position = transform.position;
        }

        yield break;
    }
}
