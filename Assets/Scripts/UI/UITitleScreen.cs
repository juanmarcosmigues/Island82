using TMPro;
using UnityEngine;

public class UITitleScreen : MonoBehaviour
{
    public Camera uiCamera;
    public UIOptionBox optionBox;

    private void Start()
    {
        GameRender.AddOverlayCamera(uiCamera);
    }

    //private void OnDestroy()
    //{
    //    GameRender.RemoveOverlayCamera(uiCamera);
    //}

    public void Show ()
    {
        gameObject.SetActive(true);
        optionBox.Show("Continue?", "Yes", "No", Continue, NewGame, 
            UIOptionBox.OptionType.Positive, UIOptionBox.OptionType.Negative);
    }
    public void Continue ()
    {

    }
    public void NewGame ()
    {

    }
}
