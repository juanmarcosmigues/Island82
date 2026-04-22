using UnityEngine;

public interface IInteractable
{
    bool Consumable { get; }
    string InteractionName { get; }
    void Interact();
}
