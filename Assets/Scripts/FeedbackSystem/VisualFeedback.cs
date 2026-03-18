using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualFeedback : MonoBehaviour
{
    public float duration;

    Coroutine currentPlay;
    public virtual void Clear() 
    {
        if (currentPlay != null)
            StopCoroutine(currentPlay);
    }
    public virtual void Play(float speedMultiplier = 1.0f)
    {
        if (currentPlay != null)
            StopCoroutine(currentPlay);
        currentPlay = StartCoroutine(PlayAnimation(speedMultiplier));
    }
    public virtual void PlayTime (float t)
    {

    }
    protected virtual IEnumerator PlayAnimation (float speedMultiplier)
    {
        yield break;
    }
}
