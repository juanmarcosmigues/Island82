using System.Collections;
using UnityEngine;

public class RigidbodyRadialExplosion : MonoBehaviour
{
    public float force;
    public float torque;

    private Rigidbody[] parts;
    private bool activated;

    private void Awake()
    {
        parts = GetComponentsInChildren<Rigidbody>();
    }
    private void OnEnable()
    {
        if (activated) return;

        foreach (var part in parts)
        {
            part.linearVelocity = -part.transform.localPosition.normalized * force;
            part.angularVelocity = Vector3.Cross(-part.transform.localPosition.normalized, Vector3.up).normalized * torque;
        }

        StartCoroutine(CheckForSleep());
        activated = true;
    }
    IEnumerator CheckForSleep ()
    {
        int sleepingPieces = 0;
        do
        {
            yield return new WaitForSeconds(1f);
            foreach (var piece in parts)
            {
                if (piece.linearVelocity.sqrMagnitude < 0.001f)
                {
                    piece.isKinematic = true;
                    piece.GetComponent<Collider>().enabled = false;
                    sleepingPieces++;
                }

                yield return null;
            }
        } while (sleepingPieces < parts.Length);
    }
}
