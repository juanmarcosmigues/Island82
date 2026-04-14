using UnityEngine;

[DefaultExecutionOrder(GameDefinitions.EXECUTION_ORDER_MOVING_SURFACE)]
public class MovingSurface : MonoBehaviour
{

    public Vector3 force => displacement;
    public Vector3 GetRotationForce(Vector3 worldPosition)
    {
        Vector3 center = transform.position;
        Vector3 offset = worldPosition - center;
        Vector3 rotatedOffset = rotation * offset;

        return rotatedOffset - offset;
    }
    public Vector3 GetFinalFrameTranslation(Vector3 worldPosition)
    {
        return force + GetRotationForce(worldPosition);
    }

    protected Vector3 displacement;
    protected Quaternion rotation;
    protected Vector3 lastPos;
    protected Quaternion lastRotation;
    protected int disableForFrames;

    protected virtual void Awake ()
    {
        this.gameObject.layer = LayerMask.NameToLayer(GameDefinitions.LAYER_DYNAMICSOLID);
    }
    protected virtual void FixedUpdate ()
    {
        if (disableForFrames > 0)
        {
            disableForFrames--;
        }
        else
        {
            rotation = transform.rotation * Quaternion.Inverse(lastRotation);
            displacement = transform.position - lastPos;
        }

        lastPos = transform.position;
        lastRotation = transform.rotation;
    }

    public void DisableForFrames (int frames) => disableForFrames = frames;
}
