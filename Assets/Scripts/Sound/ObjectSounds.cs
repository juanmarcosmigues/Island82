using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectSounds : MonoBehaviour
{
    public KeyValuePair<AudioClip>[] clip;

    protected Dictionary<string, AudioClip> clipsDictionary;

    private void Awake()
    {
        clipsDictionary = KeyValuePair<AudioClip>.GetDictionary(clip);
    }
    public AudioClip GetClip(string key) => clipsDictionary[key];
    public void PlaySound (string key)
    {
        SoundManager.instance.PlaySound(GetClip(key), transform.position);
    }
    public void PlaySound (string key, float volume, float pitch = 1f)
    {
        SoundManager.instance.PlaySound(GetClip(key), transform.position, volume, pitch);
    }
    public void PlaySound(string key, float volume, float pitch, float distance)
    {
        SoundManager.instance.PlaySound(GetClip(key), transform.position, volume, pitch, 1f, distance);
    }
}
