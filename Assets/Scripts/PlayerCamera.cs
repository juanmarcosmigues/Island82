using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float heightVelocity;

    private float heightTarget;
    private Vector3 pos;
    private void FixedUpdate()
    {
        Vector3 newPos = Player.Instance.transform.position.FlattenY();
        if (Camera.main.WorldToViewportPoint(Player.Instance.transform.position).y > 0.5f)
        {
            if (Player.Instance.Grounded)
                if (Mathf.Abs(Player.Instance.transform.position.y - heightTarget) > 0.9f)
                    heightTarget = Player.Instance.transform.position.y;

            newPos.y = Mathf.MoveTowards(pos.y, heightTarget, heightVelocity * Time.fixedDeltaTime);
        }
        else
        {
            heightTarget = Player.Instance.transform.position.y;
            newPos.y = heightTarget;
        }
        pos = newPos;

        transform.position = pos;
    }
}
