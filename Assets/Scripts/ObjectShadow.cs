using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShadow : MonoBehaviour
{
    public Transform sourceObject;
    public Transform shadow;
    public float maxDistance = 1f;
    public LayerMask shadowMask;

    private float originHeight;
    private float originScale;
    private Vector3 originLocalPosition;
    private Quaternion originRotation;

    private void Awake()
    {
        originHeight = shadow.localPosition.y;
        originScale = shadow.localScale.x;
        originLocalPosition = shadow.localPosition;
        originRotation = shadow.rotation;
    }

    void LateUpdate()
    {
        Vector3 position = sourceObject.position;
        Vector3 direction = Vector3.up;
        RaycastHit hit;
        if (Physics.Raycast(sourceObject.transform.position, Vector3.down, out hit, 100f, shadowMask, QueryTriggerInteraction.Ignore))
        {
            position.y = hit.point.y + 0.02f;
            direction = hit.normal;

            if (!shadow.gameObject.activeSelf)
                shadow.gameObject.SetActive(true);
        }
        else
        {
            position.y = sourceObject.transform.position.y + originHeight;

            if (shadow.gameObject.activeSelf)
                shadow.gameObject.SetActive(false);
        }

        float scale = Mathf.Clamp01(1f-(sourceObject.transform.position.y - hit.point.y)/maxDistance);

        shadow.position = position;
        shadow.forward = direction;
        shadow.localScale = Vector3.one * Mathf.Clamp(scale, 0.1f, 1f);
    }
}
