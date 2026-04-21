using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEvent : MonoBehaviour
{
    public KeyItemPair<UnityEvent>[] events;
    public KeyItemPair<UnityEvent<bool, float>>[] stateEvents;
    protected Dictionary<string, UnityEvent> dictionaryEvents;
    protected Dictionary<string, UnityEvent<bool, float>> dictionaryStateEvents;

    private void Awake()
    {
        dictionaryEvents = KeyItemPair<UnityEvent>.GetDictionary(events);
        dictionaryStateEvents = KeyItemPair<UnityEvent<bool, float>>.GetDictionary(stateEvents);
    }
    public void CallEvent(string key)
    {
        if (dictionaryEvents.ContainsKey(key))
            dictionaryEvents[key].Invoke();
        else
            Debug.Log("Animation Event with the key: " + key + " does not exists.");
    }
    public void CallStateEvent(string key, bool active, float time)
    {
        if (dictionaryStateEvents.ContainsKey(key))
            dictionaryStateEvents[key].Invoke(active, time);
        else
            Debug.Log("Animation Event with the key: " + key + " does not exists.");
    }

    public UnityEvent GetEvent (string key)
    {
        if (dictionaryEvents.ContainsKey(key))
            return dictionaryEvents[key];

        return null;
    }
    public UnityEvent<bool, float> GetStateEvent(string key)
    {
        if (dictionaryStateEvents.ContainsKey(key))
            return dictionaryStateEvents[key];

        return null;
    }
}
