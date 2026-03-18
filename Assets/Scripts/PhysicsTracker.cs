using UnityEngine;

public class PhysicsTracker : MonoBehaviour
{
    public Collider TrackedCollider;
    public Rigidbody TrackedRigidBody;

    public Vector3 LastFramePosition { get; private set; }
    public Bounds LastFrameBounds { get; private set; }
    public Vector3 LastFrameLinearVelocity { get; private set; }
    public Vector3 LastFrameAngularVelocity { get; private set; }

    private void Awake()
    {
        StartCoroutine(EndOfFixedUpdateLoop());
    }

    /// <summary>
    /// Runs AFTER all FixedUpdates and physics simulation each tick.
    /// </summary>
    private System.Collections.IEnumerator EndOfFixedUpdateLoop()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();   // cache once

        while (true)
        {
            yield return waitForFixedUpdate;

            if (TrackedRigidBody != null)
            {
                LastFramePosition = TrackedRigidBody.position;
                LastFrameLinearVelocity = TrackedRigidBody.linearVelocity;
                LastFrameAngularVelocity = TrackedRigidBody.angularVelocity;
            }
            if (TrackedCollider != null)
            {
                LastFrameBounds = TrackedCollider.bounds;
            }
        }
    }
}
