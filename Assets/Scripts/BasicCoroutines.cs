using UnityEngine;
using System.Collections;

public static class BasicCoroutines
{
    public static IEnumerator DisableAfter(this GameObject me, float time)
    {
        yield return new WaitForSeconds(time);
        me.SetActive(false);
    }
}
