using UnityEngine;

public class InteractionDialogue : InteractionObject
{
    public override bool Consumable => false;
    public override string InteractionName  => "Trigger Dialogue";

    public Dialogue[] dialogues;

    public override void Interact()
    {
        UIDialogueBox.Instance.Show(dialogues);
    }
}
