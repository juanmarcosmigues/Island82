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
        Player.Instance.input.inputEnabled = false;

        yield return new WaitForSeconds(1);

        UIDialogueBox.Instance.Show(
            (header: "Maca:", body: "Taru are you coming?"),
            (header: "Maca:", body: "Ronnie made cookies, I’ll save you some. Meet us at the watchtower.")
            );

        UIHud.Instance.gameObject.SetActive(true);
    }
}
