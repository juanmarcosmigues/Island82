using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Coroutines;

public class RigidBodyExplosion : MonoBehaviour
{
    public Rigidbody[] parts;
    public RangeVector3 directionRange;
    public RangeValue forceRange;
    public RangeValue torqueRange;
    public RangeValue lifetime;

    protected Vector3[] originalPositions;
    protected float longerLifetime;
    private void Awake()
    {
        originalPositions = new Vector3[parts.Length];
        for (int i = 0; i < originalPositions.Length; i++)
        {
            originalPositions[i] = parts[i].transform.localPosition;
        }
    }
    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(WaitFrames(1, () => Explode()));
    }
    protected void Clear()
    {
        for (int i = 0; i < originalPositions.Length; i++)
        {
            parts[i].transform.localPosition = originalPositions[i];
            parts[i].gameObject.SetActive(true);
        }
        longerLifetime = 0f;
    }
    private void OnDisable()
    {
        Clear();      
    }
    public void Explode ()
    {
        foreach (var part in parts)
        {                       
            var partGameObject = part.gameObject;
            float partLifeTime = lifetime.GetRandomValue();

            if (partLifeTime > longerLifetime) longerLifetime = partLifeTime;

            partGameObject.SetActive(true);
            //disable part
            StartCoroutine(Wait(partLifeTime, () => { partGameObject.SetActive(false); }));
            ShootPiece(part);
        }

        //disable explosion prefab after the last piece dissapeared
        StartCoroutine(Wait(longerLifetime + 0.1f, () => { gameObject.SetActive(false); Clear(); }));
    }
    public void ShootPiece (Rigidbody rb)
    {
        float torque = torqueRange.GetRandomValue();

        rb.AddForce(transform.TransformDirection(directionRange.GetRandomValue().normalized) * forceRange.GetRandomValue(), ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * torque, ForceMode.Impulse);
    }
}
