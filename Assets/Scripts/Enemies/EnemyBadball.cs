using System.Collections;
using static DebugDraw;
using UnityEngine;
using Utils;


public class EnemyBadBall : MonoBehaviour
{
    public float radius;
    public float jumpHeight;
    public Transform root;
    public Transform dieVFXs;
    public Animation bounceAnim;
    [Range(0f, 1f)]
    public float dropChance;
    public string drop;

    private Locomotion locomotion;
    private JumpOn jumpOn;
    private EnemyHitbox hitbox;
    private ObjectSounds sounds;

    bool dead = false;
    bool hostile = false;
    float sqrRadius;
    Vector3 origin;
    Vector3 target;
    Vector3 deltaToTarget;
    Vector3 moveDirection;

    private void Awake()
    {
        jumpOn = GetComponent<JumpOn>();
        locomotion = GetComponent<Locomotion>();
        hitbox = GetComponentInChildren<EnemyHitbox>();
        sounds = GetComponentInChildren<ObjectSounds>();

        jumpOn.OnJumpedOn += _ => Die(true);
        locomotion.OnLand += Bounce;
    }
    private void Start()
    {
        origin = transform.position;
        sqrRadius = radius * radius;
    }
    void Bounce(GroundData ground)
    {
        StartCoroutine(_Bounce());
    }
    IEnumerator _Bounce ()
    {
        yield return null;
        locomotion.Jump(jumpHeight);
        bounceAnim.Stop();
        bounceAnim.Play("animEnemyBallBounce");

        Vector3 playerFromRadius = (Player.Instance.transform.position - origin).FlattenY();

        if (playerFromRadius.sqrMagnitude < sqrRadius != hostile)
        {
            hostile = !hostile;
        }

        if (!hostile)
            target = origin;
        else
            target = origin + Vector3.ClampMagnitude(playerFromRadius, radius);

        deltaToTarget = (target - transform.position).FlattenY();
        moveDirection = deltaToTarget.normalized;
    }
    private void Update()
    {

        deltaToTarget = (target - transform.position).FlattenY();
        if (deltaToTarget.sqrMagnitude > 0.01f && moveDirection.sqrMagnitude > 0.001f)
        {
            locomotion.Move(moveDirection);
        }     

        //root.rotation = Quaternion.LookRotation(perpAxis * Mathf.Sign(Vector3.Dot(perpAxis, delta)), Vector3.up);
        //DebugDraw.Sphere(target, 0.1f, Quaternion.identity, Color.rebeccaPurple, 20, 0.1f);
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
        sounds.PlaySound("DieSFX");

        if (delay)
            yield return new WaitForSeconds(0.08f);

        //dieVFXs.SetParent(null);
        //dieVFXs.gameObject.SetActive(true);
        gameObject.SetActive(false);

        if (Random.value <= dropChance && drop.Length > 0)
        {
            PoolManager.Instance.GetPool<ObjectPool>(drop).GetObject().transform.position = transform.position;
        }

        yield break;
    }
    private void OnDrawGizmos()
    {
        Vector3 o = Application.isPlaying ? origin : transform.position;
        Gizmos.color = Color.yellow;
        GizmosExtensions.DrawWireCircle(o, radius);
    }
}
