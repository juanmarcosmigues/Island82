using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbiencePlayer : MonoBehaviour
{
    private static Dictionary<string, AmbiencePlayer> Ambiences = new Dictionary<string, AmbiencePlayer>();
    public static AmbiencePlayer Get(string name) => Ambiences[name];

    public static void FadeOutAll(float duration)
    {
        foreach (var a in Ambiences)
        {
            a.Value.FadeOut(duration, true);
        }
    }

    public string ambienceName;
    public bool startFading;
    public bool dontDestroy;

    float targetVolume;
    AudioSource source;
    Coroutine fading;

    private void OnDestroy()
    {
        Ambiences.Remove(ambienceName);
    }
    private void Awake()
    {
        if (dontDestroy)
        {
            if (Ambiences.ContainsKey(ambienceName))
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }

        source = GetComponent<AudioSource>();
        targetVolume = source.volume;
        Ambiences.Add(ambienceName, this);

        if (startFading)
            FadeIn(2f);
    }

    public void FadeOut (float duration, bool force = false)
    {
        if (fading != null)
        {
            if (force)
            {
                StopCoroutine(fading);
                fading = null;
            }
            else
                return;
        }

        fading = StartCoroutine(Fade(duration, 0f));
    }

    public void FadeIn (float duration, bool force = false)
    {
        if (fading != null)
        {
            if (force)
            {
                StopCoroutine(fading);
                fading = null;
            }
            else
                return;
        }

        fading = StartCoroutine(Fade(duration, targetVolume));
    }
    private IEnumerator Fade (float duration, float targetVolume)
    {
        float t = 0f;
        float startVolume = source.volume;
        while (t < 1f)
        {
            t = Mathf.Clamp01(t += Time.deltaTime / duration);
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        fading = null;
    }
}
