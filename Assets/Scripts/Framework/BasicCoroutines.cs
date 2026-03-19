using UnityEngine;
using System.Collections;

public static class BasicCoroutines
{
    public static IEnumerator DisableAfter(this GameObject me, float time)
    {
        yield return new WaitForSeconds(time);
        me.SetActive(false);
    }
    public static IEnumerator Blink (this GameObject me, float interval, float time)
    {
        float t = 0f;
        var yieldInterval = new WaitForSeconds(interval);
        while (t < time)
        {
            me.SetActive(!me.activeSelf);

            yield return yieldInterval;

            t += interval;
        }
        me.SetActive(true);
    }
}
