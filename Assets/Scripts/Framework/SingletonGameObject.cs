using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SingletonGameObject : MonoBehaviour
{
    private static Dictionary<string, SingletonGameObject> singletons =
        new Dictionary<string, SingletonGameObject>();

    public string singletonID;
    public bool dontDestroyOnLoad;
    public bool queuedToBeDestroyed { get; private set; } = false;

    public void Awake()
    {
        if (string.IsNullOrEmpty(singletonID))
        {
            Debug.LogError("Singleton key not set!");
            return;
        }

        //singleton exists and is healthy
        if (singletons.ContainsKey(singletonID) &&
            singletons[singletonID] != null)
        {
            //destroy this new copy
            Destroy(this.gameObject);
            queuedToBeDestroyed = true;
            return;
        }

        //set as new singleton
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        //singleton exists but has missing the reference
        if (singletons.ContainsKey(singletonID))
        {
            singletons[singletonID] = this;
            return;
        }

        singletons.Add(singletonID, this);
    }

    private void OnDestroy()
    {
        if (singletons.ContainsKey(singletonID) && singletons[singletonID] == this)
            singletons.Remove(singletonID);
    }

    public GameObject GetSingleton(string singletonID)
    {
        if (!singletons.ContainsKey(singletonID)
            || singletons[singletonID] == null)
            return null;

        return singletons[singletonID].gameObject;
    }
}
