using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class EnemyHand : MonoBehaviour
{
    private const float GRAB_DURATION = 4f;
    private const float TO_GRAB_TIME = 0.4f;
    private const float MAX_LIVE_TIME = 25f;

    public static EnemyHand Instance {  get; private set; }

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

    public bool Grabbing => grabbing;

    private bool active;
    private bool grabbing;
    private Player player;
    private Rigidbody rigidBody;
    private Collider coll;
    private TubeTrail trail;
    private Timestamp timerToSpawn;
    private Timestamp timerSinceSpawn;
    private Timestamp timerTrySpawn;
    private float toGrabTime;

    private void Awake()
    {
        Instance = this;
    }
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
        Vector3 targetPosition = player.transform.position;
        targetPosition.y += 0.5f;
        Vector3 delta = targetPosition - transform.position;
        float dist = delta.magnitude;
        float speed = Mathf.Lerp(0.1f, 1f, (dist - innerRadius) / 2f);
        Quaternion rot = Quaternion.Lerp(rigidBody.rotation,
            Quaternion.LookRotation(delta.FlattenY().normalized, Vector3.up),
            speed * speed * 10f * Time.fixedDeltaTime);
        float handRotLerp = 1-Mathf.InverseLerp(catchArc.radius, catchArc.radius * 2f, dist);
        handRoot.localRotation = Quaternion.AngleAxis(Mathf.Lerp(0, 40f, handRotLerp), Vector3.right);
        Vector3 velocity = delta.FlattenY().normalized * speed;
        velocity.y = delta.y * 0.2f;
        bool onVerticalPlane = Mathf.Abs(delta.y) < 0.5f;

        rigidBody.linearVelocity = velocity;
        rigidBody.MoveRotation(rot);

        if (onVerticalPlane && 
            catchArc.Contains(rigidBody.position, rigidBody.rotation, player.transform.position))
        {
            toGrabTime += Time.fixedDeltaTime;
        }
        else
        {
            toGrabTime -= Time.fixedDeltaTime;
            toGrabTime = Mathf.Max(toGrabTime, 0f);
        }

        if (toGrabTime >= TO_GRAB_TIME || 
            (dist < catchRadius && onVerticalPlane))
        {
            GrabPlayer();
        }

        if ((timerSinceSpawn.elapsed > MAX_LIVE_TIME && dist > spawnRadius.min * 0.5f)
            || (timerSinceSpawn.elapsed > MAX_LIVE_TIME * 0.5f && Mathf.Abs(delta.y) > 3f)
            || timerSinceSpawn.remainingTime < 0f && dist > spawnRadius.min)
        {
            Leave();
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
            toGrabTime = 0f;
            transform.position = pos + dir * 10f;
            hand.SetActive(true);
            trail.emitting = true;
            trail.offset = 0f;
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

        player.EnterGrabbed();
        player.transform.SetParent(grabTarget);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;

        grabbing = true;

        StartCoroutine(Shake(GRAB_DURATION, handRoot));
        StartCoroutine(Damage(GRAB_DURATION, 3));
        StartCoroutine(Leave(GRAB_DURATION));

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
            float f = 0.05f;
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

        IEnumerator Leave (float duration)
        {
            Vector3 originPoint = transform.position;
            Vector3 currentPos = originPoint;
            Vector3 currentDirection = transform.forward;

            yield return null;

            trail.emitting = false;
            float t = 0f;
            float factor = 0f;
            float lightValue = 1f;
            float lightSteps = 1f / 10f;
            float dist = float.MaxValue;

            while (t < 1f && lightValue > 0f)
            {
                t = Mathf.Clamp01(t + Time.deltaTime * 0.05f * factor);
                factor = Mathf.Clamp01(factor + Time.deltaTime / 4f);

                if (t > 0.1f)
                    lightValue = Mathf.Clamp01(lightValue - Time.deltaTime/3f);

                trail.GetTipAtOffset(t, out currentPos, out currentDirection);
                trail.offset = t;
                transform.position = currentPos - trail.transform.localPosition;
                transform.forward = currentDirection;

                dist = (originPoint - transform.position).magnitude;

                float steppedLightValue = Mathf.FloorToInt(lightValue / lightSteps) * lightSteps;
                Shader.SetGlobalFloat("_DarkRoomFactor", steppedLightValue);

                yield return null;
            }

            GameplayManager.Instance.HandEndGrab();
        }
    }
    public void Leave ()
    {
        enabled = false;

        StartCoroutine(_(2f));
        IEnumerator _(float duration)
        {
            Vector3 originPoint = transform.position;
            Vector3 currentPos = originPoint;
            Vector3 currentDirection = transform.forward;

            trail.emitting = false;
            coll.enabled = false;
            float t = 0f;
            float factor = 0f;

            while (t < 1f)
            {
                t = Mathf.Clamp01(t + (Time.deltaTime / duration) * factor);
                factor = Mathf.Clamp01(factor + Time.deltaTime * 2f);

                trail.GetTipAtOffset(t, out currentPos, out currentDirection);
                trail.offset = t;
                transform.position = currentPos - trail.transform.localPosition;
                transform.forward = currentDirection;

                yield return null;
            }

            Despawn();
            enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        catchArc.Debug(transform.position, transform.rotation, Color.red);
        GizmosExtensions.DrawWireCircle(transform.position, catchRadius);
    }
}
