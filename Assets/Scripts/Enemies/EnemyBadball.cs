using System.Collections;
using static DebugDraw;
using UnityEngine;


public class EnemyBadBall : MonoBehaviour
{
    public Vector2 zone;
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
    Vector3 axis;
    Vector3 perpAxis;
    Vector3 origin;
    Vector3 target;
    Vector3 deltaToTarget;

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
        axis = Quaternion.AngleAxis(zone.y, Vector3.up) * Vector3.right;
        perpAxis = Quaternion.AngleAxis(zone.y, Vector3.up) * Vector3.forward;
        origin = transform.position;
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
    }
    private void Update()
    {
        Vector3 delta = (Player.Instance.transform.position - origin).FlattenY();
        target = origin + Vector3.ClampMagnitude(Vector3.Project(delta, axis), zone.x * 0.5f);

        Vector3 newDeltaToTarget = (target - transform.position).FlattenY();
        float dot = Vector3.Dot(deltaToTarget, newDeltaToTarget);
        if (deltaToTarget.sqrMagnitude > 0.01f && dot >= 0f)
        {
            locomotion.Move(deltaToTarget.normalized);
        }
        deltaToTarget = newDeltaToTarget;

        root.rotation = Quaternion.LookRotation(perpAxis * Mathf.Sign(Vector3.Dot(perpAxis, delta)), Vector3.up);
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
        Matrix4x4 oldMatrix = Gizmos.matrix;

        Quaternion rotation = Quaternion.AngleAxis(zone.y, Vector3.up);
        Vector3 size = new Vector3(zone.x, 0f, 0f); // y is arbitrary height

        Gizmos.matrix = Matrix4x4.TRS(o, rotation, Vector3.one);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, size);

        Gizmos.matrix = oldMatrix; // restore so later gizmos aren't affected
    }
}
