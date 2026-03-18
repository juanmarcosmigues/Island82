using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObjectSpawner : MonoBehaviour
{
    public void SpawnObject (string key)
    {
        PoolManager.Instance.GetPool<ObjectPool>(key)?.GetObject(transform.position, transform.rotation);
    }
}
