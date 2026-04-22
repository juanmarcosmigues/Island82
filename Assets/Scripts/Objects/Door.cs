using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    public const float DURATION = 2.3f;

    public int triggersAmount = 1;
    public Transform doorMesh;
    public Transform doorTarget;

    public UnityEvent OnOpen;

    private Collider coll;
    private ObjectSounds sound;
    private int currentTriggers;

    private void Awake()
    {
        sound = GetComponent<ObjectSounds>();
        coll = GetComponent<Collider>();
    }
    public void TriggerOpen() 
    {
        currentTriggers += 1;
        if (currentTriggers == triggersAmount)
        {
            Open();
        }
    }
    public void Open ()
    {
        StartCoroutine(OpenAnimation());
        
    }

    IEnumerator OpenAnimation ()
    {
        float t = 0f;
        Vector3 pos = new Vector3(-1, 0, 0);
        Vector3 origin = doorMesh.transform.position;

        sound.PlaySound("Open");

        MainCameraShaker.instance.Shake(0.04f, DURATION, 0.1f);
        while (t < 1f)
        {
            t = Mathf.Clamp01(t + Time.deltaTime / DURATION) ;
            doorMesh.transform.position = Vector3.Lerp(origin, doorTarget.position, t);
            doorMesh.transform.localPosition += pos * Mathf.Sign(Mathf.Sin(t * 200f)) * 0.03f;
            yield return null;
        }
        MainCameraShaker.instance.Shake(0.2f, 0.2f, 0.1f);

        coll.enabled = false;
        doorMesh.position = doorTarget.position;
        OnOpen?.Invoke();
    }
}
