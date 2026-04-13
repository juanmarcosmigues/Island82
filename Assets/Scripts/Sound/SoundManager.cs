using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public SingletonGameObject singletonComponent;
    public int audioSourceAmount = 99;

    protected Dictionary<string, AudioSource> playingSources;
    protected List<AudioSource> sources;
    protected List<AudioSource> busySources;

    private void Awake()
    {
        if (singletonComponent.queuedToBeDestroyed) return;

        sources = new List<AudioSource>();
        busySources = new List<AudioSource>();

        for(int i = 0; i < audioSourceAmount; i++)
        {
            var source = new GameObject("AudioSource:" + i).AddComponent<AudioSource>();
            source.transform.SetParent(transform,false);
            source.gameObject.SetActive(false);

            sources.Add(source);
        }

        instance = this;
    }

    public void PlaySound (AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, float spatialBlend = 0.5f, float distance = 10f, bool loop = false)
    {
        var source = GetFreeSource();
        ActivateSource(source);

        source.transform.position = position;

        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.maxDistance = distance;
        source.spatialBlend = spatialBlend;

        source.Play();
    }

    private void LateUpdate()
    {
        for(int i = 0;i < busySources.Count; i++)
        {
            if (!busySources[i].isPlaying)
            {
                DeactivateSource(busySources[i]);
                i--;
            }
        }
    }
    protected void ActivateSource (AudioSource source)
    {
        sources.Remove(source);
        busySources.Add(source);
        source.gameObject.SetActive(true);
    }
    protected void DeactivateSource(AudioSource source)
    {
        sources.Add(source);
        busySources.Remove(source);
        source.gameObject.SetActive(false);
    }
    protected AudioSource GetFreeSource ()
    {
        return sources[0];
    }
}
