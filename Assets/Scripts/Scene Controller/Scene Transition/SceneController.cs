using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    public string sceneID;
    public KeyItemPair<Transform>[] spawnPoints;
    public Transform playerGroup;
    public Transform player;

    public System.Action<string> OnSceneStarts;

    Dictionary<string, Transform> spawnPointsDict;

    private void Awake()
    {
        spawnPointsDict = KeyItemPair<Transform>.GetDictionary(spawnPoints);
    }
    protected virtual void Start()
    {
        //if loading scene waits for setup to be called by scene loader
        if (SceneTransitioner.Instance.loadingScene)
            return;
 
        OnSceneStart(); //On scene start default if not called by loader
    }
    public virtual void OnSceneStart (string fromScene = "")
    {
        Instance = this;

        if (spawnPointsDict.ContainsKey(fromScene))
        {
            playerGroup.transform.position = spawnPointsDict[fromScene].position;
            player.transform.forward = spawnPointsDict[fromScene].forward;
        }

        OnSceneStarts?.Invoke(fromScene);
    }
}
