using UnityEngine;

public class TriggerEventHandler : MonoBehaviour, ITrigger
{
    public event System.Action<Collider> OnTriggerEntered;
    public event System.Action<Collider> OnTriggerOut;
    public event System.Action<ITrigger> OnTriggered;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggered?.Invoke(this);
        OnTriggerEntered?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        OnTriggerOut?.Invoke(other);
    }
}
