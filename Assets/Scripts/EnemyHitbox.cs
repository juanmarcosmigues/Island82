using UnityEngine;
using System.Collections;

public class EnemyHitbox : MonoBehaviour
{
    public string hitboxTag;
    public CombatHandler.Weight weight;
    public int damage;
    public LayerMask hitMask;

    private Entity owner;
    private JumpOn jumpOn;
    private Coroutine hitPlayerCoroutine;

    private void Awake()
    {
        owner = GetComponentInParent<Entity>();
        jumpOn = GetComponentInParent<JumpOn>();
    }
    private void OnTriggerEnter(Collider other)
    {
        HitSomething(other);
    }
    private void HitSomething(Collider something)
    {
        if (hitMask.ContainsLayer(something.gameObject.layer))
        {
            Entity entity = something.gameObject.GetComponentInParent<Entity>();
            if (entity == null) return;
            if (entity == owner) return; //Avoid hitting itself

            if (entity.type == Entity.Type.Player)
            {
                if (hitPlayerCoroutine == null)
                    hitPlayerCoroutine = StartCoroutine(HitPlayerEvaluation());
            }
            else
            {
                var combatHandler = entity.GetComponent<CombatHandler>();
                if (combatHandler != null)
                    combatHandler.GetHit(gameObject, damage, weight, hitboxTag);
            }
        }
    }
    private IEnumerator HitPlayerEvaluation()
    {
        Debug.Log("starting damage");
        if (Player.Instance.Invulnerable) yield break;
        Debug.Log("starting damage 2");
        yield return new WaitForFixedUpdate();
        if (jumpOn != null && jumpOn.timerJumpedOn.elapsed < 0.1f) yield break; //Wait a frame and favour bounce instead of hit
        Debug.Log("starting damage 3");
        var combatHandler = Player.Instance.GetComponent<CombatHandler>();
        combatHandler.GetHit(gameObject, damage, weight, hitboxTag);

        hitPlayerCoroutine = null;
    }
}
