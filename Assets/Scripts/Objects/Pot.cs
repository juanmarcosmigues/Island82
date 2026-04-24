using UnityEngine;
using static CombatHandler;

public class Pot : MonoBehaviour
{
    public int life;
    public Mesh[] meshes;
    public MeshFilter mesh;
    public ParticleSystem breakParticles;
    public Transform vfxParts;
    public BoxCollider trigger;
    public BoxCollider coll;
    public Animation anim;
    public Vector3 parentOffset;
    [Range(0f, 1f)]
    public float dropChance;
    public string drop;

    JumpOn jumpOn;
    bool playerInside;
    int currentLife;
    Timestamp timer;

    private void Awake()
    {
        jumpOn = GetComponent<JumpOn>();    
        trigger.isTrigger = true;
        currentLife = life;

        jumpOn.OnJumpedOn += JumpedOn;
    }
    void JumpedOn (JumpOn _)
    {
        if (Player.Instance.SpiritMode) return;
        if (Player.Instance.InsidePot) return;

        if (Player.Instance.HeavyFalling)
            Break();
        else
            PlayerIn();
    }
    void PlayerIn()
    {
        coll.enabled = false;
        Player.Instance.EnterPot(this);
        transform.SetParent(Player.Instance.transform);
        transform.localPosition = parentOffset;
        anim.Play();
        timer.Set();

        Player.Instance.OnGroundedStart += Land;

        enabled = false;
        playerInside = true;
        jumpOn.enabled =false;
    }

    void Land (Vector3 velocity, bool heavyFall)
    {
        if (timer.elapsed < 0.2f) return;

        if (!heavyFall)
            RecieveDamage();
        else
            Break();
    }
    public void RecieveDamage ()
    {
        currentLife--;
        if (currentLife <= 0)
        {
            Break();
        }
        else
        {
            mesh.mesh = meshes[Mathf.FloorToInt((1 - currentLife / (float)life) * meshes.Length)];
            breakParticles.Play();
            anim.Play();
        }
    }
    void Break ()
    {
        if (playerInside)
        {
            Player.Instance.ExitPot();
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
}
