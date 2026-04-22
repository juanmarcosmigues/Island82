using System.Collections;
using UnityEngine;

public class ThiefCutscene : Cutscene
{
    public GameObject thief;
    public float speed;
    public LocomotionPath path;
    public Dialogue[] dialogue;

    protected override IEnumerator CustomCutscene()
    {
        thief.gameObject.SetActive(true);
        thief.transform.position = path.GetPoint(0);

        yield return MoveToTarget(path.GetPoint(1));

        StartCoroutine(LookAt(Player.Instance.transform.position));

        UIDialogueBox.Instance.Show(dialogue);

        while (!UIDialogueBox.Instance.HasEnded) yield return null;

        StartCoroutine(LookAt(path.GetPoint(2)));

        yield return MoveToTarget(path.GetPoint(2));

        yield return MoveToTarget(path.GetPoint(3));

        thief.gameObject.SetActive(false);
    }
    IEnumerator LookAt(Vector3 target)
    {
        Vector3 delta = target - thief.transform.position;
        Quaternion targetRot = Quaternion.identity;

        while (Vector3.Dot(thief.transform.forward, delta.normalized) < 0.99f) 
        {
            delta = target - thief.transform.position;
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
}
