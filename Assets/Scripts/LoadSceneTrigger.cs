using UnityEngine;

public class LoadSceneTrigger : MonoBehaviour
{
    public string sceneName;
    public BoxCollider trigger;
    public Color color;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        var player = other.gameObject.GetComponent<Player>();
        if (player == null) return;

        player.input.inputEnabled = false;
        SceneTransitioner.LoadScene(sceneName, 1, 1, 2);

        AmbiencePlayer.FadeOutAll(1f);
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
