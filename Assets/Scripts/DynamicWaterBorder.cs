using UnityEngine;

public class DynamicWaterBorder : MonoBehaviour
{
    public Transform source;
    public float checkDistance = 1f;
    public LayerMask waterMask;
    public SkinnedMeshRenderer waterShore;
    public AnimationCurve borderBlend;
    public ParticleSystem splash;
    [Range(0f, 1f)]
    public float borderThickness = 1f;

    private RaycastHit hit;
    private Vector3 lastPos;

    private void Start()
    {
        waterShore.material.SetFloat("_Amount", borderThickness);
    }
    private void LateUpdate()
    {
        if (Physics.Raycast(
            source.transform.position + Vector3.up * checkDistance, 
            Vector3.down, 
            out hit, 
            checkDistance, 
            waterMask))
        {
            if (!waterShore.gameObject.activeSelf)
            {
                waterShore.gameObject.SetActive(true);
                splash.Play();
            }

            waterShore.transform.position = hit.point;
            splash.transform.position = hit.point;

            waterShore.SetBlendShapeWeight(0,
                borderBlend.Evaluate(waterShore.transform.localPosition.y / checkDistance) * 100f);

            if ((lastPos-waterShore.transform.position).sqrMagnitude > 0.05f)
            {
                lastPos = waterShore.transform.position;
                splash.Play();
            }      
        }
        else
        {
            if (waterShore.gameObject.activeSelf)
            {
                waterShore.gameObject.SetActive(false);
                splash.Play();
            }
        }
    }
}
