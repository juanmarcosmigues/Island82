using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtOrthographicCamera : MonoBehaviour
{
    public Camera target;

    private void OnEnable()
    {
        if (target == null)
            target = Camera.main;
    }

    private void LateUpdate()
    {
        transform.forward = -target.transform.forward;
    }
}
