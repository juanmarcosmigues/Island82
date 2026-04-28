using UnityEngine;
using UnityEngine.Events;

public class SwitchButton : MonoBehaviour
{
    private const float DURATION = 0.5f;
    private const float PRESSED_HEIGHT = 0.09f;

    public ElasticFloat elasticValue;
    public ParticleSystem dust;
    public Transform surfaceTransform;

    public UnityEvent OnPressed;

    private bool pressed;
    private bool goingDown;

    private SurfaceProperties switchSurface;
    private ObjectSounds sound;
    private Vector3 startPos;
    private Vector3 endPos;

    private void Awake()
    {
        switchSurface = surfaceTransform.GetComponent<SurfaceProperties>();
        sound = GetComponent<ObjectSounds>();
        switchSurface.OnLanded += _ => sound.PlaySound("Squeak", 0.8f, 0.9f);
        startPos = surfaceTransform.localPosition;
        endPos = Vector3.up * PRESSED_HEIGHT;
    }

    private void FixedUpdate()
    {
        if (elasticValue.Value > 0.7f && !goingDown)
        {
            if (switchSurface.landed)
            {
                elasticValue.target = 0.7f;
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
        surfaceTransform.localPosition = Vector3.Lerp(startPos, endPos, 1 - elasticValue.Value);

        if (elasticValue.Value <= 0.13f && !pressed)
        {
            pressed = true;
            surfaceTransform.localPosition = endPos;
            MainCameraShaker.instance.Shake(0.1f, 0.2f, 0.1f);
            dust.gameObject.SetActive(true);

            OnPressed?.Invoke();

            enabled = false;
        }
    }
}
