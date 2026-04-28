using UnityEngine;
using UnityEngine.Events;

public class SwitchButton : MonoBehaviour
{
    public ElasticFloat elasticValue;
    public ParticleSystem dust;
    public float pressedHeight = 0.04f;

    public UnityEvent OnPressed;

    private bool pressed;
    private float startHeight;
    private SurfaceProperties nail;

    private void Awake()
    {
        nail = GetComponentInChildren<SurfaceProperties>();
        nail.OnLanded += Landed;

        startHeight = nail.transform.localPosition.y;
    }

    private void FixedUpdate()
    {
        if (elasticValue.Value <= 0.13f && !pressed)
        {
            pressed = true;
            nail.transform.localPosition = Vector3.up * pressedHeight;
            MainCameraShaker.instance.Shake(0.1f, 0.2f, 0.1f);
            dust.gameObject.SetActive(true);   

            OnPressed?.Invoke();

            enabled = false;
        }

        if (!pressed)
        {
            if (nail.landed)
            {
                elasticValue.target = 0.8f;
            }
            else
            {
                elasticValue.target = 1f;
            }
            elasticValue.Update(Time.fixedDeltaTime);

            nail.transform.localPosition = Vector3.up * Mathf.LerpUnclamped(pressedHeight, startHeight, elasticValue.Value);
        }
    }
    void Landed (Vector3 vel)
    {
        if (vel.y <= -Player.HEAVY_FALL_VELOCITY * 0.8f)
            elasticValue.ApplyForce(-10f);
    }
}
