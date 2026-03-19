using UnityEngine;
using static CombatHandler;

public class Pot : MonoBehaviour
{
    public Mesh[] meshes;
    public MeshFilter mesh;
    public ParticleSystem breakParticles;
    public Transform vfxParts;
    public BoxCollider trigger;
    public BoxCollider coll;
    public Animation anim;
    public CombatHandler combatHandler;
    public Vector3 parentOffset;
    [Range(0f, 1f)]
    public float dropChance;
    public string drop;

    bool playerInside;
    int life;
    Timestamp timer;

    private void Awake()
    {
        trigger.isTrigger = true;
        life = meshes.Length;
        combatHandler.OnGetHit += GetHit;
    }
    private void FixedUpdate()
    {
        if (playerInside) return;

        if ((coll.bounds.max.y - Player.Instance.coll.bounds.min.y) <= 0.01f  && 
            Player.Instance.VerticalVelocity < 1f && 
            trigger.bounds.Intersects(Player.Instance.coll.bounds))
        {
            PlayerIn();
        }
    }
    void PlayerIn()
    {
        coll.enabled = false;
        Player.Instance.InsideObject(true);
        transform.SetParent(Player.Instance.transform);
        transform.localPosition = parentOffset;
        anim.Play();
        timer.Set();

        Player.Instance.OnGroundedStart += Land;

        enabled = false;
        playerInside = true;
    }

    void Land ()
    {
        if (timer.elapsed < 0.2f) return;

        life--;
        if (life <= 0 )
        {
            Break();
        }
        else
        {
            mesh.mesh = meshes[meshes.Length-life];
            breakParticles.Play();
            anim.Play();
        }
    }
    void Break ()
    {
        if (playerInside)
        {
            Player.Instance.InsideObject(false);
            Player.Instance.OnGroundedStart -= Land;
            transform.SetParent(null);

            playerInside = false;
        }

        vfxParts.transform.SetParent(null);
        vfxParts.gameObject.SetActive(true);
        gameObject.SetActive(false);
              
        if (Random.value <= dropChance && drop.Length > 0)
        {
            PoolManager.Instance.GetPool<ObjectPool>(drop).GetObject().transform.position = transform.position;
        }
    }
    void GetHit(GameObject source, int damage, Weight weight, string tag)
    {
        Break();
    }
}
