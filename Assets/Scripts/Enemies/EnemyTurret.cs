using System.Collections;
using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    public Transform destroyedParts;
    public Transform renderRoot;

    private BounceOn bounceOn;
    private BoxCollider coll;

    private Vector3 deltaToPlayer;
    private bool dead;
    private void Awake()
    {
        coll = GetComponent<BoxCollider>();
        bounceOn = GetComponent<BounceOn>();
        bounceOn.OnBounce += e => Die();
    }
    private void FixedUpdate()
    {
        deltaToPlayer = Player.Instance.transform.position - transform.position;

        if (deltaToPlayer.FlattenY().sqrMagnitude > 2f)
            LookAtPlayer();
    }
    public void LookAtPlayer ()
    {
        renderRoot.forward = GameWorld.LookAtDirection(deltaToPlayer.FlattenY().normalized);
    }
    public void Die ()
    {
        if (dead) return;
        dead = true;

        bounceOn.enabled = false;
        coll.enabled = false;

        StartCoroutine(DieAnimation());
    }
    IEnumerator DieAnimation()
    {
        yield return new WaitForSeconds(0.08f);
        destroyedParts.SetParent(null);
        destroyedParts.gameObject.SetActive(true);
        PoolManager.Instance.GetPool<VFXPool>("VFXDustExplosionFlat").GetObject().transform.position = transform.position;
        gameObject.SetActive(false);
    }
}
