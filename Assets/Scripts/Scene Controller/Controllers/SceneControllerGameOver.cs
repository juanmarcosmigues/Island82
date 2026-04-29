using System.Collections;
using UnityEngine;

public class SceneControllerGameOver : SceneController
{
    public override void OnSceneStart(string fromScene = "")
    {
        base.OnSceneStart(fromScene);

        StartCoroutine(StartCutscene());
    }

    IEnumerator StartCutscene ()
    {
        UIHud.Instance.gameObject.SetActive(false);
        UIGameOver.Instance.Show();

        yield return new WaitForSeconds(1);     
    }
}
