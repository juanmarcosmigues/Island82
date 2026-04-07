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

                while (rigidBody.position.SqrDistance(targetPosition) > 0.01f)
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
}
