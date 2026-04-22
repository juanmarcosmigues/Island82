using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    public int triggersAmount = 1;

    public UnityEvent OnOpen;

    private int currentTriggers;
    public void TriggerOpen() 
    {
        currentTriggers += 1;
        if (currentTriggers == triggersAmount)
        {
            Open();
        }
    }
    public void Open ()
    {
        OnOpen?.Invoke();
    }
}
