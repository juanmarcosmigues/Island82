using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHand : MonoBehaviour
{
    public GameObject hand;
    public RangeValue spawnRadius;
    public float innerRadius;
    public float followSpeed;
    public RangeValue spawnTimeRange;
    public Arc catchArc;
    public LayerMask solidMask;

    private bool active;
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
        if (!active)
            DisabledUpdated();
    }
    private void FixedUpdate()
    {
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
        Quaternion rot = Quaternion.RotateTowards(rigidBody.rotation,
            Quaternion.LookRotation(delta.FlattenY().normalized, Vector3.up),
            speed * 1.5f);

        rigidBody.linearVelocity = delta.normalized * speed;
        rigidBody.MoveRotation(rot);

        if (catchArc.Contains(rigidBody.position, rigidBody.rotation, player.transform.position))
        {

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

        StartCoroutine(_Spawn(spawns[Random.Range(0, spawns.Count)]));
    }

    IEnumerator _Spawn (Vector3 pos)
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
    private void OnDrawGizmos()
    {
       catchArc.Debug(transform.position, transform.rotation, Color.red);
    }
}
