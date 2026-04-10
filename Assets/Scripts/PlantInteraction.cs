using UnityEngine;

public class PlantInteraction : MonoBehaviour
{
    public static Player player;

    public Transform root;
    public Transform shadow;
    public float horizontalRadius;
    public float verticalRadius;
    public float maxAngle;
    public float shadowInfluence;

    float hRadiusSqr; 
    float influence;

    private void Awake()
    {
        hRadiusSqr = horizontalRadius * horizontalRadius;
    }
    private void Start()
    {
        if (player == null)
            player = Player.Instance;
    }
    private void LateUpdate()
    {
        Vector3 delta = root.position - player.transform.position;
        Vector3 deltaFlatten = delta.FlattenY();

        if (deltaFlatten.sqrMagnitude >= hRadiusSqr) return;

        influence = (1-Mathf.Clamp01(deltaFlatten.sqrMagnitude / hRadiusSqr)) *
                (1-Mathf.Clamp01(-delta.y / verticalRadius));
        Vector3 perp = Vector3.Cross(deltaFlatten.normalized, Vector3.up);
        root.rotation = Quaternion.AngleAxis(-maxAngle * influence, perp);

        shadow.localPosition = deltaFlatten.normalized * influence * shadowInfluence;
    }
}
