using UnityEngine;

public class PresenceBaseAction : MonoBehaviour
{
    public float maxTimeSinceExit = 1f;

    public bool Active => isOn || disabledAlive;
    public System.Action<bool, float> OnActive;
    public System.Action<bool, float> OnDisabled;

    Timestamp startTime;
    Timestamp endTime;
    bool stimulatedThisFrame;
    bool disabledAlive;
    bool isOn;

    public void Tick(object source = null)
    {
        stimulatedThisFrame = true;
    }

    void LateUpdate()
    {
        if (!isOn && !disabledAlive && !stimulatedThisFrame) return;

        if (stimulatedThisFrame && !isOn)
        {
            isOn = true;
            startTime.Set();
            OnActive?.Invoke(true, 0f);
        }
        else if (stimulatedThisFrame && isOn)
        {
            OnActive?.Invoke(false, startTime.elapsed);
        }
        else if (!stimulatedThisFrame && isOn)
        {
            isOn = false;
            endTime.Set();
            OnDisabled?.Invoke(true, 0f);
        }
        else
        {
            if (disabledAlive)
            {
                OnDisabled?.Invoke(false, endTime.elapsed);
            }
        }

        disabledAlive = endTime.elapsed < maxTimeSinceExit;
        stimulatedThisFrame = false;
    }
}
