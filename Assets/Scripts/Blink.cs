using UnityEngine;

public class Blink : MonoBehaviour
{
    [Tooltip("The component to toggle on and off.")]
    public Behaviour target;

    [Tooltip("Time in seconds between each toggle.")]
    public float blinkInterval = 0.5f;

    private float _timer;

    private void Update()
    {
        if (target == null) return;

        _timer += Time.deltaTime;

        if (_timer >= blinkInterval)
        {
            _timer = 0f;
            target.enabled = !target.enabled;
        }
    }
}
