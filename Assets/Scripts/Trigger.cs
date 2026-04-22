using UnityEditor;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public bool consume;
    public string targetTag;
    public BoxCollider trigger;
    public ITrigger[] targets;
    public Color color;

    private void Awake()
    {
        gameObject.layer = LayerMask.GetMask(GameDefinitions.LAYER_TRIGGER);
        trigger.isTrigger = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (targetTag.Length > 0)
            if (other.tag != targetTag) return;

        targets.ForEach(t => t.Trigger());

        if (consume)
            gameObject.SetActive(false);
    }

    void OnDrawGizmos()
    {
        if (trigger == null) return;

        Gizmos.matrix = Matrix4x4.TRS(
        trigger.transform.TransformPoint(trigger.center),
        trigger.transform.rotation,
        Vector3.Scale(trigger.transform.lossyScale, trigger.size)
        );

        Gizmos.color = color;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Color c = color;
        c.a = 0.2f;
        Gizmos.color = c;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = Matrix4x4.identity; // reset so other gizmos aren't affected
    }
}
