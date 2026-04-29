using UnityEngine;

[DefaultExecutionOrder(-1)]
public class UIBootstrap : MonoBehaviour
{
    [SerializeField] private UIHud hud;
    [SerializeField] private UIDialogueBox dialogueBox;
    [SerializeField] private UIInteractionMarker interactionMarker;
    [SerializeField] private UIGameOver gameOver;

    private void Awake()
    {
        SingletonGameObject parent = GetComponentInParent<SingletonGameObject>();
        if (parent != null)
        {
            if (parent.queuedToBeDestroyed)
                return;
        }

        UIHud.SetInstance(hud);
        UIDialogueBox.SetInstance(dialogueBox);
        UIInteractionMarker.SetInstance(interactionMarker);
        UIGameOver.SetInstance(gameOver);
    }
}
