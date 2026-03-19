using UnityEngine;
using System.Collections;

public class Shaker : MonoBehaviour
{
    public Transform target;
    public bool freezeZ;

    protected virtual void Awake()
    {
        if (target == null)
            target = this.transform;
    }

    public void Shake(float amount, float duration, float smoothness, float stillness = 0.5f) =>
        StartCoroutine(ShakeAnimation(amount, duration, smoothness, stillness));
    private IEnumerator ShakeAnimation (float amount, float duration, float smoothness, float stillness = 0.5f)
    {
        Vector3 origin = target.localPosition;
        float currentAmount = amount;
        float startTime = Time.time;
        int ticks = 0;
        int lastChangeAtTicks = 0;
        Vector3 goal = Random.insideUnitSphere * currentAmount;
        float t = 0.0f;

        while (t < 3f)
        {
            t = (Time.time - startTime) / duration;
            currentAmount = amount * Mathf.Clamp01(1 - t);
            ticks++;
            if (ticks - lastChangeAtTicks > 10 * stillness)
            {
                goal = Random.insideUnitSphere * currentAmount;
                if (freezeZ)
                    goal.z = 0;
                lastChangeAtTicks = ticks;
            }
            target.localPosition = Vector3.Lerp(target.localPosition, origin + goal, 1f - smoothness);

            if (target.localPosition == origin && t >= 1f)
                break;

            yield return null;
        }
    }
}