using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBadBall : MonoBehaviour
{
    public float radius;
    public float jumpHeight;
    public Transform root;
    public Transform dieVFXs;
    [Range(0f, 1f)]
    public float dropChance;
    public string drop;

    private Locomotion locomotion;
    private JumpOn jumpOn;
    private EnemyHitbox hitbox;
    private ObjectSounds sounds;

    bool dead = false;

    private void Awake()
    {
        jumpOn = GetComponent<JumpOn>();
        locomotion = GetComponent<Locomotion>();
        hitbox = GetComponentInChildren<EnemyHitbox>();
        sounds = GetComponentInChildren<ObjectSounds>();

        jumpOn.OnJumpedOn += _ => Die(true);
        locomotion.OnLand += Bounce;
    }
    void Bounce(GroundData ground)
    {
        locomotion.Jump(jumpHeight);
    }
    private void Update()
    {
        Vector3 moveDirection = (Player.Instance.transform.position - transform.position).FlattenY().normalized;
        float factor = Mathf.Clamp01(Mathf.Max(Vector3.Dot(transform.forward, moveDirection.normalized) + 0.5f, 0.1f));
        locomotion.Move(moveDirection, factor);
    }
    public void Die(bool delay = true)
    {
        Debug.Log("die");
        locomotion.KillVerticalVelocity();
        return;
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
}
