using UnityEngine;

public class TriggerEventHandler : MonoBehaviour
{
    public event System.Action<Collider> OnTriggerEntered;
    public event System.Action<Collider> OnTriggerOut;
    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEntered?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        OnTriggerOut?.Invoke(other);
    }
}
