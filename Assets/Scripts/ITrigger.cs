using UnityEngine;

public interface ITrigger
{
    public event System.Action<ITrigger> OnTriggered;
    public void Trigger() { }
}
