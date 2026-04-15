using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySlime : MonoBehaviour
{
    public float delay;
    public float castTime;
    public float spikesTime;
    public float idleTime;
    public Transform spikes;
    public Transform root;
    public Transform dieVFXs;
    public VisualFeedback vfSpikesOn;
    public VisualFeedback vfSpikesOff;
    [Range(0f, 1f)]
    public float dropChance;
    public string drop;

    private JumpOn jumpOn;
    private EnemyHitbox hitbox;
    private ObjectSounds sounds;

    bool dead = false;

    private void Awake()
    {
        jumpOn = GetComponent<JumpOn>();
        hitbox = GetComponentInChildren<EnemyHitbox>();
        sounds = GetComponentInChildren<ObjectSounds>();

        jumpOn.OnJumpedOn += _ => Die(true);
    }
    private void Start()
    {
        StartCoroutine(Behaviour());
    }
    IEnumerator Behaviour ()
    {
        spikes.gameObject.SetActive(false);
        hitbox.gameObject.SetActive(false);
        jumpOn.enabled = true;

        yield return new WaitForSeconds(delay);

        while (true)
        {
            yield return Cast(castTime);

            vfSpikesOn.Play();
            sounds.PlaySound("Spikes", 1f, 1f, 7f);
            spikes.gameObject.SetActive(true);
            hitbox.gameObject.SetActive(true);
            jumpOn.enabled = false;

            yield return new WaitForSeconds(spikesTime);

            vfSpikesOff.Play();
            spikes.gameObject.SetActive(false);
            hitbox.gameObject.SetActive(false);
            jumpOn.enabled = true;

            yield return new WaitForSeconds(idleTime);
        }
    }
    IEnumerator Cast (float duration)
    {
        float f = 0.2f;
        float t = 0f;
        float v = 0f;
        Vector3 pos = new Vector3(-1, 0, 1);

        while (t < duration)
        {
            v = Mathf.Max(0.1f, t / duration);
            f = 0.2f * Mathf.Max(0.07f, 1-v);

            pos *= -1f;
            pos.Normalize();

            pos *= v * 0.04f;

            root.localPosition = pos;
            yield return new WaitForSeconds (f);

            t += f;
        }

        root.localPosition = Vector3.zero;
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
}
