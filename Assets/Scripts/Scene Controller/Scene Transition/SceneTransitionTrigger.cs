using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SceneTransitionTrigger : MonoBehaviour
{
    public LayerMask mask;
    public string sceneToLoad;
    public float fadeInSpeed, fadeOutSpeed, yieldTime;
    public UnityEvent onTrigger;

    protected bool loadingScene = false;
    private void OnTriggerEnter(Collider other)
    {
        if (loadingScene) return;

        if (mask == (mask | (1 << other.gameObject.layer)))
        {
            loadingScene = SceneTransitioner.LoadScene(sceneToLoad, fadeInSpeed, fadeOutSpeed, yieldTime);
            onTrigger?.Invoke();
        }
    }
}
