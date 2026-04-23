using System.IO;
using UnityEngine;

public class PathPlatform : MonoBehaviour
{
    public float speed;
    public float turningSpeed;
    public Patrol patrol;

    private Rigidbody rigidBody;

    (Vector3 point, Vector3 direction) currentTarget;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        currentTarget = patrol.GetTarget(transform.position);
        if (patrol.flat) currentTarget.point.y = rigidBody.position.y;
        Vector3 newPos = Vector3.MoveTowards(rigidBody.position, currentTarget.point, speed * Time.fixedDeltaTime);
        Quaternion newRot = Quaternion.RotateTowards(rigidBody.rotation, Quaternion.LookRotation(currentTarget.direction, Vector3.up), turningSpeed * Time.fixedDeltaTime);
        rigidBody.MovePosition(newPos);
        rigidBody.MoveRotation(newRot);
    }
}
