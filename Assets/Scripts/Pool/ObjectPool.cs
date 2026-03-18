using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject obj;
    public int poolSize;

    protected GameObject[] objPool;

    public void Start()
    {
        objPool = new GameObject[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            objPool[i] = Instantiate(obj);
            ResetClone(objPool[i]);
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < objPool.Length; i++)
        {
            if (!objPool[i].activeSelf && objPool[i].transform.parent != transform)
                ResetClone(objPool[i]);
        }
    }

    protected virtual void ResetClone (GameObject clone)
    {
        clone.SetActive(false);
        clone.transform.SetParent(this.transform);
        clone.transform.localPosition = Vector3.zero;
        clone.transform.rotation = Quaternion.identity;
        clone.transform.localScale = Vector3.one;
    }

    public virtual GameObject GetObject()
    {
        for (int i = 0; i < objPool.Length; i++)
        {
            if (objPool[i].activeSelf) continue;

            objPool[i].SetActive(true);
            objPool[i].transform.SetParent(null);

            return objPool[i];
        }

        return null;
    }
    protected virtual int GetNextAvailableIndex ()
    {
        for (int i = 0; i < objPool.Length; i++)
        {
            if (objPool[i].activeSelf) continue;

            return i;
        }

        return -1;
    }

    public virtual GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        int index = GetNextAvailableIndex();
        if (index == -1) return null;

        objPool[index].SetActive(true);
        objPool[index].transform.SetParent(null);
        objPool[index].transform.SetPositionAndRotation(position, rotation);

        return objPool[index];
    }
}
