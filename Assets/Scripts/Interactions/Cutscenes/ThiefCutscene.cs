using System.Collections;
using UnityEngine;

public class ThiefCutscene : Cutscene
{
    public GameObject thief;
    public Transform[] jumpPositions;
    public float speed;
    public LocomotionPath path;
    public Dialogue[] dialogue;

    protected override IEnumerator CustomCutscene()
    {
        yield return new WaitForSeconds(1);

        yield return PlayerMove();

        yield return new WaitForSeconds(0.5f);

        thief.gameObject.SetActive(true);
        thief.transform.position = path.GetPoint(0);

        yield return MoveToTarget(path.GetPoint(1));

        StartCoroutine(LookAt(Player.Instance.transform.position));

        UIDialogueBox.Instance.Show(dialogue);

        while (!UIDialogueBox.Instance.HasEnded) yield return null;

        StartCoroutine(LookAt(path.GetPoint(2)));

        yield return MoveToTarget(path.GetPoint(2));

        yield return MoveToTarget(path.GetPoint(3));

        yield return MoveToTarget(path.GetPoint(4));

        thief.gameObject.SetActive(false);
    }
    IEnumerator LookAt(Vector3 target)
    {
        Vector3 delta = (target - thief.transform.position).FlattenY();
        Quaternion targetRot = Quaternion.identity;

        while (Vector3.Dot(thief.transform.forward, delta.normalized) < 0.99f) 
        {
            delta = (target - thief.transform.position).FlattenY();
            targetRot = Quaternion.LookRotation(delta.normalized, Vector3.up);
            thief.transform.rotation = Quaternion.RotateTowards(thief.transform.rotation, targetRot, 180 * Time.deltaTime);
            yield return null;
        }
    }
    IEnumerator MoveToTarget (Vector3 target)
    {
        Vector3 delta = target - thief.transform.position;
        while (delta.sqrMagnitude > 0.1f)
        {
            thief.transform.position = Vector3.MoveTowards(thief.transform.position, target, speed * Time.deltaTime);
            delta = target - thief.transform.position;
            yield return null;
        }
        
    }

    IEnumerator PlayerMove ()
    {
        Player player = Player.Instance;

        while (!player.IsGrounded) yield return null;

        Vector3 jumpTarget = Vector3.zero;
        float minDelta = Mathf.Infinity;
        for (int i = 0; i < jumpPositions.Length; i++)
        {
            Vector3 delta = jumpPositions[i].position - player.transform.position;
            if (delta.sqrMagnitude < minDelta)
            {
                jumpTarget = jumpPositions[i].position;
                minDelta = delta.sqrMagnitude;
            }
        }

        Vector3 jumpDir = (jumpTarget - player.transform.position).FlattenY();

        do
        {
            player.Jump();
            player.Move(jumpDir.normalized, 1f);
            yield return new WaitForFixedUpdate();   

        } while (!player.IsGrounded);


    }
}
