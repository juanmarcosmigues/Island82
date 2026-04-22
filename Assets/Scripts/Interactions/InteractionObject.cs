using UnityEngine;

public class InteractionObject : MonoBehaviour, IInteractable
{
    public virtual bool Consumable => true;
    public virtual string InteractionName => "Default";

    public Transform markerPosition;
    public BoxCollider interactionZone;
    public Color color;

    protected bool showingMarker;

    public virtual void Interact()
    {
        
    }

    private void LateUpdate()
    {
        if (Player.Instance == null) return;

        if (interactionZone.bounds.Contains(Player.Instance.transform.position))
        {
            if (!showingMarker)
            {
                UIInteractionMarker.Instance.ShowMarker(markerPosition.position);
                Player.Instance.AddInteraction(this);
                showingMarker = true;
            }
        }
        else
        {
            if (showingMarker)
            {
                UIInteractionMarker.Instance.HideMarker();
                Player.Instance.RemoveInteraction(this);
                showingMarker = false;
            }
        }
    }

    void OnDrawGizmos ()
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(interactionZone.bounds.center, interactionZone.bounds.size);
        Color c = color;
        c.a = 0.2f;

        Gizmos.color = c;
        Gizmos.DrawCube(interactionZone.bounds.center, interactionZone.bounds.size);
    }
}
