using System.Collections;
using UnityEngine;

public class Cutscene : MonoBehaviour, ITrigger
{
    public bool playerInControl;

    public bool Playing => cutscenePlayer != null;

    public event System.Action OnCutsceneStart;
    public event System.Action OnCutsceneEnd;
    public event System.Action<ITrigger> OnTriggered;

    protected Coroutine cutscenePlayer;
    protected Coroutine cutscene;

    public void Trigger ()
    {
        TriggerCutscene();
    }

    public void TriggerCutscene () 
    {
        if (cutscenePlayer == null)
            cutscenePlayer = StartCoroutine(CutscenePlayer());
    }

    private IEnumerator CutscenePlayer ()
    {
        OnCutsceneStart?.Invoke();

        if (!playerInControl)
            Player.Instance.PlayerInControl.Set(false, "Cutscene");

        this.cutscene = StartCoroutine(CustomCutscene());
        yield return cutscene;

        if (!playerInControl)
            Player.Instance.PlayerInControl.Set(true, "Cutscene");

        OnCutsceneEnd?.Invoke();

        cutscenePlayer = null;
    }
    protected virtual IEnumerator CustomCutscene ()
    {
        yield return null;
    }

}
