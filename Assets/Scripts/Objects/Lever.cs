using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    private const float DURATION = 0.5f;

    public ElasticFloat elasticValue;
    public ParticleSystem dust;
    public Transform rotationTarget;

    public UnityEvent OnPressed;

    private bool pressed;
    private bool goingDown;

    private Quaternion startRotation;
    private Quaternion endRotation;
    private SurfaceProperties lever;
    private ObjectSounds sound;

    private void Awake()
    {
        lever = GetComponentInChildren<SurfaceProperties>();
        sound = GetComponent<ObjectSounds>();
        lever.OnLanded += _ => sound.PlaySound("Squeak", 0.8f, 0.9f);
        startRotation = lever.transform.localRotation;
        endRotation = rotationTarget.localRotation;
    }

    private void FixedUpdate()
    {
        if (elasticValue.Value > 0.8f && !goingDown)
        {
            if (lever.landed)
            {
                elasticValue.target = 0.8f;
            }
            else
            {
                elasticValue.target = 1f;
            }
        }
        else if (!goingDown)
        {
            goingDown = true;
            elasticValue.target = 0f;
            sound.PlaySound("Pressed", 0.6f);
        }

        elasticValue.Update(Time.fixedDeltaTime);
        lever.transform.localRotation = Quaternion.LerpUnclamped(startRotation, endRotation, 1 - elasticValue.Value);

        if (elasticValue.Value <= 0.13f && !pressed)
        {
            pressed = true;
            lever.transform.localRotation = endRotation;
            MainCameraShaker.instance.Shake(0.1f, 0.2f, 0.1f);
            dust.gameObject.SetActive(true);

            OnPressed?.Invoke();

            enabled = false;
        }
    }
}
