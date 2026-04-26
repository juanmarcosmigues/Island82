using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class EnemyHand : MonoBehaviour
{
    private const float GRAB_DURATION = 4f;

    public GameObject hand;
    public Mesh meshHandRest;
    public Mesh meshHandGrab;
    public MeshFilter handMesh;
    public Transform handRoot;
    public Transform grabTarget;
    public RangeValue spawnRadius;
    public float innerRadius;
    public float followSpeed;
    public RangeValue spawnTimeRange;
    public Arc catchArc;
    public float catchRadius;
    public LayerMask solidMask;

    private bool active;
    private bool grabbing;
    private Player player;
    private Rigidbody rigidBody;
    private Collider coll;
    private TubeTrail trail;
    private Timestamp timerToSpawn;
    private Timestamp timerSinceSpawn;
    private Timestamp timerTrySpawn;

    private void Start()
    {
        player = Player.Instance;
        rigidBody = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
        trail = GetComponentInChildren<TubeTrail>();
    }
    private void Update()
    {
        if (grabbing) return;

        if (!active)
            DisabledUpdated();
    }
    private void FixedUpdate()
    {
        if (grabbing) return;

        if (active)
            ActiveUpdate();
    }
    void DisabledUpdated ()
    {          
        if (!Player.Instance.Grounded) return;
        if (timerToSpawn.remainingTime > 0f) return;
        if (timerTrySpawn.remainingTime > 0f) return;

        Spawn();
        timerTrySpawn.Set(0.4f);
    }
    void ActiveUpdate ()
    {
        Vector3 delta = player.transform.position - transform.position;
        float dist = delta.magnitude;
        float speed = Mathf.Lerp(0.1f, 1f, (dist - innerRadius) / 2f);
        Quaternion rot = Quaternion.Lerp(rigidBody.rotation,
            Quaternion.LookRotation(delta.FlattenY().normalized, Vector3.up),
            speed * speed * 10f * Time.fixedDeltaTime);
        float handRotLerp = 1-Mathf.InverseLerp(catchArc.radius, catchArc.radius * 2f, dist);
        handRoot.localRotation = Quaternion.AngleAxis(Mathf.Lerp(0, 40f, handRotLerp), Vector3.right);

        rigidBody.linearVelocity = delta.normalized * speed;
        rigidBody.MoveRotation(rot);

        if (catchArc.Contains(rigidBody.position, rigidBody.rotation, player.transform.position) 
            || dist < catchRadius)
        {
            GrabPlayer();
        }

        if (timerSinceSpawn.remainingTime < 0f && dist > spawnRadius.min)
        {
            Despawn();
        }
    }
    public void Despawn ()
    {
        timerToSpawn.Set(spawnTimeRange.GetRandomValue());
        hand.SetActive(false);
        coll.enabled = false;
        rigidBody.isKinematic = true;
        active = false;    
    }
    public void Spawn ()
    {
        Vector3 playerPos = player.transform.position;
        List<Vector3> spawns = new();
        RaycastHit r;

        for (int i = 0; i < Directions8.All.Length; i++)
        {           
            Vector3 point = playerPos + Directions8.All[i] * spawnRadius.max;
            if (Physics.Linecast(playerPos, point, out r, solidMask))
            {
                if (r.distance < spawnRadius.min)
                    continue;
            }

            spawns.Add(point);
        }

        if (spawns.Count <= 0) return;

        StartCoroutine(_(spawns[Random.Range(0, spawns.Count)]));

        IEnumerator _(Vector3 pos)
        {
            Vector3 dir = pos - player.transform.position;
            trail.Clear();
            timerSinceSpawn.Set(5f);
            transform.position = pos + dir * 10f;
            hand.SetActive(true);
            coll.enabled = true;
            rigidBody.isKinematic = false;
            active = true;

            yield return null;

            transform.position = pos;
        }
    }
    public void GrabPlayer ()
    {
        coll.enabled = false;
        rigidBody.isKinematic = true;

        handMesh.mesh = meshHandGrab;
        handRoot.localRotation = Quaternion.Euler(0f, 90f, 90f);
        handRoot.localPosition = Vector3.up * -0.2f;

        Vector3 delta = player.transform.position - grabTarget.position;
        Vector3 point = transform.position + delta * 0.5f;
        point.y = transform.position.y;
        transform.position = point;

        player.Grabbed();
        player.transform.SetParent(grabTarget);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;

        grabbing = true;

        StartCoroutine(Shake(GRAB_DURATION, handRoot));
        StartCoroutine(Damage(GRAB_DURATION, 3));

        IEnumerator Damage (float duration, int amount)
        {
            float t = 0f;
            float interval = duration / amount;

            while
                (t < duration)
            {
                yield return new WaitForSeconds(interval);
                t += interval;
                player.GetHit(this.gameObject, 1, CombatHandler.Weight.Light, "MonsterHand", false, true);
            }
        }

        IEnumerator Shake (float duration, Transform target)
        {
            float f = 0.1f;
            float t = 0f;

            Vector3 pos = new Vector3(-1, 0, 1);
            Vector3 originalPos = target.localPosition;

            while (t < duration)
            {
                pos *= -1f;
                pos.Normalize();

                pos *= 0.07f;

                target.localPosition = originalPos + pos;

                yield return new WaitForSeconds(f);

                t += f;
            }

            target.localPosition = originalPos;
        }
    }

    private void OnDrawGizmos()
    {
        catchArc.Debug(transform.position, transform.rotation, Color.red);
        GizmosExtensions.DrawWireCircle(transform.position, catchRadius);
    }
}
