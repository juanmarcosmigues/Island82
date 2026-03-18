using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPool : ObjectPool
{
    public float defaultDuration;

    public override GameObject GetObject()
    {
        var obj = base.GetObject();

        if (obj != null)
            StartCoroutine(Lifespan(obj, defaultDuration));

        return obj;
    }
    public virtual GameObject GetObject(float duration)
    {
        var obj = base.GetObject();

        if (obj != null)
            StartCoroutine(Lifespan(obj, duration));

        return obj;
    }
    public override GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        var obj = base.GetObject(position, rotation);

        if (obj != null)
            StartCoroutine(Lifespan(obj, defaultDuration));

        return obj;
    }
    public GameObject GetObject(Vector3 position, Vector3 direction)
    {
        var obj = base.GetObject(position, Quaternion.identity);

        if (obj != null)
        {
            obj.transform.forward = direction;
            StartCoroutine(Lifespan(obj, defaultDuration));
        }

        return obj;
    }
    IEnumerator Lifespan (GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
    }
}
