using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStaticAssets : MonoBehaviour
{
    public static GameStaticAssets Instance { get; private set; }

    public SingletonGameObject singletonComponent;
    public KeyItemPair<UnityEngine.Object>[] assets;

    private Dictionary<string, UnityEngine.Object> loadedAssets;
    private void Awake()
    {
        if (singletonComponent.queuedToBeDestroyed) return;

        Instance = this;
        loadedAssets = KeyItemPair<UnityEngine.Object>.GetDictionary(assets);
    }

    public static T Get<T>(string name) where T : Object
    {
        if (!Instance.loadedAssets.ContainsKey(name)) return null;
        if (Instance.loadedAssets[name] is T t) return t;
        return null;
    }
}
