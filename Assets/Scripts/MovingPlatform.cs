using System.Collections;
using UnityEngine;
using static Coroutines;

public class MovingPlatform : MonoBehaviour
{
    public Transform from, to;
    public float moveSpeed;
    public float waitTime;
    public bool once;

    protected Rigidbody rigidBody;
    protected Coroutine move;
    protected Vector3 fromPosition;
    protected Vector3 toPosition;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        fromPosition = from.position;
        toPosition = to.position;   
    }
    private void OnEnable()
    {
        move = StartCoroutine(Move());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        move = null;
    }
    public IEnumerator Move ()
    {
        bool target = (transform.position - fromPosition).sqrMagnitude > (transform.position - toPosition).sqrMagnitude;
        try
        {
            while (true)
            {
                Vector3 targetPosition = target ? fromPosition : toPosition;
                target = !target;

                while (rigidBody.position.SqrDistance(targetPosition) > 0.001f)
                {
                    rigidBody.MovePosition(Vector3.MoveTowards(rigidBody.position, targetPosition, moveSpeed * Time.fixedDeltaTime));
                    yield return new WaitForFixedUpdate();
                }

                yield return new WaitForSeconds(waitTime);

                if (once) break;
            }
        }
        finally
        {
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 a = from ? from.position : transform.position;
        Vector3 b = to ? to.position : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(a, b);

        // Arrow head
        Vector3 dir = (b - a).normalized;
        float arrowSize = 0.5f;

        if (dir != Vector3.zero)
        {
            Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
            if (right == Vector3.zero) right = Vector3.Cross(dir, Vector3.forward).normalized;

            Vector3 arrowBase = b - dir * arrowSize;
            Gizmos.DrawLine(b, arrowBase + right * arrowSize * 0.5f);
            Gizmos.DrawLine(b, arrowBase - right * arrowSize * 0.5f);

            Vector3 up = Vector3.Cross(dir, right).normalized;
            Gizmos.DrawLine(b, arrowBase + up * arrowSize * 0.5f);
            Gizmos.DrawLine(b, arrowBase - up * arrowSize * 0.5f);
        }

        // Mark endpoints
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(a, 0.15f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(b, 0.15f);
    }
}
