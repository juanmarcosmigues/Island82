using UnityEngine;

public class LoadSceneTrigger : MonoBehaviour
{
    public string sceneName;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        var player = other.gameObject.GetComponent<Player>();
        if (player == null) return;

        player.input.inputEnabled = false;
        SceneTransitioner.LoadScene(sceneName, 1, 1, 2);

        AmbiencePlayer.FadeOutAll(1f);
    }
}
