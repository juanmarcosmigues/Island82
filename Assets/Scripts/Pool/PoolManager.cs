using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("Pools").AddComponent<PoolManager>();
            }

            return instance;
        }
    }

    protected static PoolManager instance;

    public Dictionary<string, ObjectPool> pools;

    private void Start()
    {
        instance = this;
        pools = new Dictionary<string, ObjectPool>();

        var poolsInstances = FindObjectsByType<ObjectPool>(FindObjectsSortMode.InstanceID);
        foreach (var pool in poolsInstances) 
        { 
            pools.Add(pool.gameObject.name, pool);
            pool.transform.SetParent(this.transform);
        }
    }

    public T GetPool<T>(string key) where T : ObjectPool
    {
        return pools[key] as T;
    }
}
