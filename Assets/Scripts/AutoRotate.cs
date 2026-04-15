using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second.")]
    public float speed = 45f;

    [Tooltip("Axis to rotate around.")]
    public Vector3 axis = Vector3.up;

    void Update()
    {
        transform.Rotate(axis * speed * Time.deltaTime);
    }
}
