using System.Collections;
using UnityEngine;

public class SceneControllerTitleScreen : SceneController
{
    public UITitleScreen titleScreen;
    public override void OnSceneStart(string fromScene = "")
    {
        base.OnSceneStart(fromScene);

        StartCoroutine(StartCutscene());
    }

    IEnumerator StartCutscene ()
    {
        titleScreen.Show();

        yield return new WaitForSeconds(1);     
    }
}
