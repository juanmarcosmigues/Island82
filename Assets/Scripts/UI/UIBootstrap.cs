using UnityEngine;

[DefaultExecutionOrder(GameDefinitions.EXECUTION_ORDER_SYSTEM)]
public class UIBootstrap : MonoBehaviour
{
    [SerializeField] private UIHud hud;
    [SerializeField] private UIDialogueBox dialogueBox;
    [SerializeField] private UIInteractionMarker interactionMarker;
    [SerializeField] private Camera uiCamera;

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
    }

    private void Start()
    {
        GameRender.AddOverlayCamera(uiCamera);
    }

    //private void OnDestroy()
    //{
    //    GameRender.RemoveOverlayCamera(uiCamera);
    //}
}
